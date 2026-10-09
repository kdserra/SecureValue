using System;
using System.Reflection;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	// Shares the isolated TamperNotifier collection (see ThreadSafetyTests):
	// exact event counts below would break if another collection triggered
	// tamper concurrently.
	[Collection("TamperNotifier")]
	public class SecurityTests
	{
		public SecurityTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		// ---------- avalanche effect ----------

		/// <summary>Counts differing bits between two 64-bit words.</summary>
		private static int Hamming(ulong a, ulong b) =>
			System.Numerics.BitOperations.PopCount(a ^ b);

		[Fact]
		public void Avalanche_OneBitPlaintextChange_ChangesAboutHalfTheCipherBits()
		{
			// Feistel + final avalanche mixing must diffuse a single input bit
			// flip across the entire ciphertext word (~32 of 64 bits on average)
			const int samples = 2000;
			long totalFlips = 0;
			var rng = new Random(12345);
			for (int i = 0; i < samples; i++)
			{
				ulong plain = (ulong)rng.NextInt64() * (ulong)rng.NextInt64();
				ulong salt = Vault.NextRandom();
				ulong flipped = plain ^ 1UL << rng.Next(64);
				int flips = Hamming(
					Vault.Seal(plain, salt).Cipher,
					Vault.Seal(flipped, salt).Cipher
				);
				totalFlips += flips;
			}
			double average = totalFlips / (double)samples;
			Assert.InRange(average, 24.0, 40.0); // ideal is 32
		}

		[Fact]
		public void Avalanche_CipherBearsNoResemblanceToPlaintext()
		{
			// encrypting the same plaintext twice with different salts must
			// produce completely different-looking output
			const int samples = 1000;
			long totalDistance = 0;
			var rng = new Random(6789);
			for (int i = 0; i < samples; i++)
			{
				ulong plain = (ulong)rng.NextInt64() * (ulong)rng.NextInt64();
				ulong c1 = Vault.Seal(plain, Vault.NextRandom()).Cipher;
				ulong c2 = Vault.Seal(plain, Vault.NextRandom()).Cipher;
				totalDistance += Hamming(c1, plain);
			}
			double averageDistance = totalDistance / (double)samples;
			Assert.InRange(averageDistance, 24.0, 40.0);
		}

		[Fact]
		public void Avalanche_StringCells_DifferForSimilarInputs()
		{
			SecureString a = "AAAA";
			SecureString b = "AAAB";
			// decrypted values stay correct
			Assert.Equal("AAAA", (string?)a);
			Assert.Equal("AAAB", (string?)b);
			// but the raw encrypted words must be totally different
			Assert.NotEqual(GetFirstCipher(a), GetFirstCipher(b));
		}

		[Fact]
		public void Cell128_LoChange_DiffusesThroughBothWords()
		{
			// Cross-word chaining is lo->hi one-directional by construction
			// (the hi subkeys derive from the lo ciphertext): flipping the lo
			// word must move BOTH output words. (Flipping hi moves only cHi;
			// tamper detection still covers both halves via the shared tag.)
			const ulong salt = 0x123456789ABCDEF0UL;
			(ulong cLo1, ulong cHi1, _) = BlockCipher.Seal128(
				0x1111111111111111UL,
				0x2222222222222222UL,
				salt,
				Keys.ProcessSet
			);
			(ulong cLo2, ulong cHi2, _) = BlockCipher.Seal128(
				0x1111111111111110UL,
				0x2222222222222222UL,
				salt,
				Keys.ProcessSet
			);
			Assert.NotEqual(cLo1, cLo2);
			Assert.NotEqual(cHi1, cHi2);
		}

		private static ulong GetFirstCipher(SecureString s)
		{
			const System.Reflection.BindingFlags flags =
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
			var arr = (ulong[]?)typeof(SecureString).GetField("_ciphers", flags)!.GetValue(s);
			if (arr is { Length: > 0 })
			{
				return arr[0];
			}
			// Inline store: first word lives in a field.
			return (ulong)typeof(SecureString).GetField("_w0", flags)!.GetValue(s)!;
		}

		// ---------- tampering notifier ----------

		[Fact]
		public void TamperingDetected_EventFiresBeforeThrow()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cells = new Cell[1];
			cells[0].Protect(999);
			cells[0].CorruptBothForTesting();
			Assert.Throws<TamperedException>(() => cells[0].Unprotect());
			Assert.Equal(1, fired);

			SecureInt p = 5;
			object boxed = p;
			var cellField = typeof(SecureInt).GetField(
				"_cell",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			var cell = cellField.GetValue(boxed)!;
			cell.GetType()
				.GetMethod(
					"CorruptBothForTesting",
					System.Reflection.BindingFlags.NonPublic
						| System.Reflection.BindingFlags.Instance
				)!
				.Invoke(cell, null);
			cellField.SetValue(boxed, cell);
			var toString = typeof(SecureInt).GetMethod("ToString", Type.EmptyTypes)!;
			Assert.Throws<TamperedException>(() =>
			{
				try
				{
					toString.Invoke(boxed, null);
				}
				catch (System.Reflection.TargetInvocationException e)
				{
					System
						.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException!)
						.Throw();
				}
			});
			Assert.Equal(1, fired);
		}

		[Fact]
		public void TamperingDetected_HandlersCanBeRemoved()
		{
			// Listeners can be unsubscribed (test teardown, domain reload).
			int fired = 0;
			Action handler = () => fired++;
			TamperingNotifier.TamperingDetected += handler;
			TamperingNotifier.TamperingDetected -= handler;

			var cells = new Cell[1];
			cells[0].Protect(999);
			cells[0].CorruptBothForTesting();
			Assert.Throws<TamperedException>(() => cells[0].Unprotect());
			Assert.Equal(0, fired);
		}

		[Fact]
		public void HasDetectedTampering_FalseBeforeTampering()
		{
			Assert.False(TamperingNotifier.HasDetectedTampering);
		}

		[Fact]
		public void HasDetectedTampering_TrueAfterTampering()
		{
			var cells = new Cell[1];
			cells[0].Protect(999);
			cells[0].CorruptBothForTesting();
			Assert.Throws<TamperedException>(() => cells[0].Unprotect());
			Assert.True(TamperingNotifier.HasDetectedTampering);
		}

		[Fact]
		public void HasDetectedTampering_StaysTrueWhenEventCapped()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// The first tamper read dispatches; the second is suppressed,
			// but HasDetectedTampering stays true.
			for (int i = 0; i < 2; i++)
			{
				var cells = new Cell[1];
				cells[0].Protect((ulong)i + 1UL);
				cells[0].CorruptBothForTesting();
				Assert.Throws<TamperedException>(() => cells[0].Unprotect());
			}
			Assert.Equal(1, fired);
			Assert.True(TamperingNotifier.HasDetectedTampering);
		}

		// ---------- zero-out attacks ----------

		[Fact]
		public void ZeroedLiveCell_ReadsDefaultWithoutNotifying()
		{
			// Simulates the attacker's low-skill move: zero every backing field
			// of a live value (e.g. resetting a "spent upgrades" counter to 0).
			// All-default backing fields read as default (never a tamper event:
			// nothing was modified into a plausible forgery).
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			SecureInt spent = 7;
			object boxed = spent;
			FieldInfo cellField = typeof(SecureInt).GetField(
				"_cell",
				BindingFlags.NonPublic | BindingFlags.Instance
			)!;
			object cell = cellField.GetValue(boxed)!;
			Type cellType = cell.GetType();
			cellType
				.GetField("_salt", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0UL);
			cellType
				.GetField("_cipher", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0UL);
			cellType
				.GetField("_tag", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0U);
			cellType
				.GetField("_saltB", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0UL);
			cellType
				.GetField("_cipherB", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0UL);
			cellType
				.GetField("_tagB", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(cell, 0U);
			cellField.SetValue(boxed, cell);
			SecureInt zeroed = (SecureInt)boxed;

			Assert.Equal(0, (int)zeroed);
			Assert.Equal(baseline, fired);
		}

		// ---------- interchangeability with the underlying primitive ----------

		[Fact]
		public void Wrappers_InteroperateWithUnderlyingPrimitives()
		{
			SecureInt p = 10;

			// arithmetic against plain primitives in both operand orders
			Assert.Equal(15, (int)(p + 5));
			Assert.Equal(15, (int)(5 + p));
			Assert.Equal(5, (int)(p - 5));
			Assert.Equal(5, (int)(15 - p));
			Assert.Equal(20, (int)(p * 2));
			Assert.Equal(5, (int)(p / 2));

			// comparisons against plain primitives in both operand orders
			Assert.True(p < 20);
			Assert.True(20 > p);
			Assert.True(p <= 10);
			Assert.True(10 >= p);
			Assert.True(p > 5);
			Assert.True(p == 10);
			Assert.True(10 == p);
			Assert.True(p != 11);

			// literals of narrower/wider widths convert through the implicit operators
			SecureLong big = 7;
			Assert.Equal(14L, (long)(big + 7));
			Assert.True(big > 3);
		}
	}
}
