using System;
using System.Threading;
using System.Threading.Tasks;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	// Tests touching the global TamperingNotifier must not run in parallel
	// with each other: the event is process-global, so one
	// collection's tamper trigger would fire another's handlers and break
	// exact event counts. This collection runs in isolation.
	[CollectionDefinition("TamperNotifier", DisableParallelization = true)]
	public class TamperNotifierCollection { }

	/// <summary>
	/// Concurrency tests: per-thread PRNG state, shared reads, and the global
	/// tampering notifier under parallel execution.
	/// </summary>
	[Collection("TamperNotifier")]
	public class ThreadSafetyTests
	{
		public ThreadSafetyTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Fact]
		public void ParallelWrites_Reads_RoundTrip()
		{
			// Distinct wrapper per iteration; every thread draws salts and
			// seals/verifies independently (ThreadStatic PRNG state).
			Parallel.For(
				0,
				8 * 500,
				i =>
				{
					SecureInt p = i;
					Assert.Equal(i, (int)p);
					SecureDouble d = i * 0.5;
					Assert.Equal(i * 0.5, (double)d);
					SecureString s = "t" + i;
					Assert.Equal("t" + i, (string?)s);
				}
			);
		}

		[Fact]
		public void FirstWriteOnDifferentThreads_UsesDifferentSaltStreams()
		{
			var start = new ManualResetEventSlim(false);
			ulong[] salts = new ulong[2];
			Thread[] threads = new Thread[2];
			for (int i = 0; i < threads.Length; i++)
			{
				int slot = i;
				threads[i] = new Thread(() =>
				{
					start.Wait();
					SecureInt value = 123;
					object boxed = value;
					var cellField = typeof(SecureInt).GetField(
						"_cell",
						System.Reflection.BindingFlags.Instance
							| System.Reflection.BindingFlags.NonPublic
					)!;
					Cell cell = (Cell)cellField.GetValue(boxed)!;
					Span<ulong> words = stackalloc ulong[Cell.WordCount];
					cell.CopyWords(words);
					salts[slot] = words[0];
				});
				threads[i].Start();
			}
			start.Set();
			foreach (Thread thread in threads)
			{
				thread.Join();
			}

			Assert.NotEqual(0UL, salts[0]);
			Assert.NotEqual(0UL, salts[1]);
			Assert.NotEqual(salts[0], salts[1]);
		}

		[Fact]
		public void ConcurrentReads_Agree()
		{
			// Reads are pure: any number of threads may read one live value.
			SecureInt p = 123456;
			int errors = 0;
			Parallel.For(
				0,
				10000,
				_ =>
				{
					if ((int)p != 123456)
					{
						Interlocked.Increment(ref errors);
					}
				}
			);
			Assert.Equal(0, errors);
		}

		[Fact]
		public void Notifier_ConcurrentSubscribe_DoesNotThrow()
		{
			// The event must tolerate concurrent subscription; every
			// subscriber fires exactly once per tamper. Other test collections
			// share the global event, so only a lower bound is asserted.
			int fired = 0;
			Parallel.For(
				0,
				64,
				_ =>
				{
					TamperingNotifier.TamperingDetected += () => Interlocked.Increment(ref fired);
				}
			);
			var cells = new Cell[1];
			cells[0].Protect(1);
			cells[0].CorruptBothForTesting();
			Assert.Throws<TamperedException>(() => cells[0].Unprotect());
			Assert.True(fired >= 64);
		}
	}
}
