#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit ThreadSafetyTests for the Unity-native wrappers:
	/// per-thread PRNG state under parallel writes, pure concurrent reads, and the
	/// global tampering notifier under parallel subscription. The tests never touch
	/// Unity API (main-thread restriction does not apply), and the runner executes
	/// EditMode tests sequentially, so no cross-test isolation attributes are needed.
	/// </summary>
	public class ThreadSafetyTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Test]
		public void ParallelWrites_Reads_RoundTrip()
		{
			// Distinct wrapper per iteration; every thread draws salts and
			// seals/verifies independently (thread-local PRNG state).
			Parallel.For(
				0,
				8 * 500,
				i =>
				{
					SecureValue.Unity.SecureVector3 p = new Vector3(i, i + 1, i + 2);
					Assert.AreEqual(new Vector3(i, i + 1, i + 2), (Vector3)p);
					SecureValue.Unity.SecureVector2Int q = new Vector2Int(i, -i);
					Assert.AreEqual(new Vector2Int(i, -i), (Vector2Int)q);
					SecureValue.Unity.SecureColor c = new Color(i * 0.001f, 0.5f, 0.25f, 1f);
					Assert.AreEqual(new Color(i * 0.001f, 0.5f, 0.25f, 1f), (Color)c);
				}
			);
		}

		[Test]
		public void ConcurrentReads_Agree()
		{
			// Reads are pure: any number of threads may read one live value.
			SecureValue.Unity.SecureVector3 p = new Vector3(1f, 2f, 3f);
			int errors = 0;
			Parallel.For(
				0,
				10000,
				_ =>
				{
					if ((Vector3)p != new Vector3(1f, 2f, 3f))
					{
						Interlocked.Increment(ref errors);
					}
				}
			);
			Assert.AreEqual(0, errors);
		}

		[Test]
		public void Notifier_ConcurrentSubscribe_DoesNotThrow()
		{
			// The event must tolerate concurrent subscription; only a lower
			// bound is asserted (other tests share the global event).
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
			Assert.GreaterOrEqual(fired, 64);
		}
	}
}
#endif
