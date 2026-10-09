#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit MacTests fault-detection cases (1-6): salt-only,
	/// tag-only and hi-word faults, the per-word sweep, same-fault-both-copies,
	/// swap/reorder, and string truncate/extend/splice through the shared seal and
	/// tag paths. Fully-qualified wrappers, no dynamic. Statistics (uniformity,
	/// avalanche) and BigInteger/decimal extremes stay xUnit-only: they duplicate
	/// exactly and the statistics need no Unity-CLR proof.
	/// </summary>
	public class MacTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

		private static bool IsCellTagWord(int index)
		{
			return index == 2 || index == 5;
		}

		private static bool IsCell128TagWord(int index)
		{
			return index == 3 || index == 7;
		}

		private static readonly ulong[] Masks64 = new ulong[]
		{
			1UL,
			0xFFFFFFFFFFFFFFFFUL,
			0xAAAAAAAAAAAAAAAAUL,
			0x9E3779B97F4A7C15UL,
		};

		private static readonly ulong[] Masks32 = new ulong[]
		{
			1UL,
			0xFFFFFFFFUL,
			0xAAAAAAAAUL,
			0x9E3779B9UL,
		};

		private static void FlipCellWord(ref Cell cell, int index, ulong mask)
		{
			Span<ulong> words = stackalloc ulong[Cell.WordCount];
			cell.CopyWords(words);
			words[index] ^= mask;
			cell.RestoreWords(words);
		}

		private static void SwapCellWords(ref Cell cell, int a, int b)
		{
			Span<ulong> words = stackalloc ulong[Cell.WordCount];
			cell.CopyWords(words);
			ulong t = words[a];
			words[a] = words[b];
			words[b] = t;
			cell.RestoreWords(words);
		}

		private static void FlipCell128Word(ref Cell128 cell, int index, ulong mask)
		{
			Span<ulong> words = stackalloc ulong[Cell128.WordCount];
			cell.CopyWords(words);
			words[index] ^= mask;
			cell.RestoreWords(words);
		}

		private static void SwapCell128Words(ref Cell128 cell, int a, int b)
		{
			Span<ulong> words = stackalloc ulong[Cell128.WordCount];
			cell.CopyWords(words);
			ulong t = words[a];
			words[a] = words[b];
			words[b] = t;
			cell.RestoreWords(words);
		}

		private static object GetField(object box, string name)
		{
			return box.GetType().GetField(name, Flags).GetValue(box);
		}

		private static void SetField(object box, string name, object value)
		{
			box.GetType().GetField(name, Flags).SetValue(box, value);
		}

		private static SecureString HeapString(string plain)
		{
			// Lengths over 16 characters take the heap store by construction.
			SecureString s = new SecureString(plain);
			Assert.Greater(s.Length, 16);
			return s;
		}

		[Test]
		public void Cell_SaltFault_SingleCopy_Heals()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int word in new int[] { 0, 3 })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				Cell cell = default;
				cell.Protect(0x0BADF00DDEADBEEFUL);
				FlipCellWord(ref cell, word, 1UL);
				Assert.AreEqual(0x0BADF00DDEADBEEFUL, cell.Unprotect());
				Assert.AreEqual(baseline + 1, fired);
				Assert.AreEqual(0x0BADF00DDEADBEEFUL, cell.Unprotect());
				Assert.AreEqual(baseline + 1, fired);
			}
		}

		[Test]
		public void Cell_SaltFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Cell cell = default;
			cell.Protect(7UL);
			FlipCellWord(ref cell, 0, 1UL);
			FlipCellWord(ref cell, 3, 1UL);
			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void Cell_TagFault_SingleCopy_Heals()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int word in new int[] { 2, 5 })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				Cell cell = default;
				cell.Protect(999UL);
				FlipCellWord(ref cell, word, 1UL);
				Assert.AreEqual(999UL, cell.Unprotect());
				Assert.AreEqual(baseline + 1, fired);
			}
		}

		[Test]
		public void Cell_TagFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Cell cell = default;
			cell.Protect(7UL);
			FlipCellWord(ref cell, 2, 1UL);
			FlipCellWord(ref cell, 5, 1UL);
			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void Cell128_SaltAndTagFaults_HealSingle_ThrowBoth()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int word in new int[] { 0, 4, 3, 7 })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				Cell128 cell = default;
				cell.Protect(0x1111111111111111UL, 0x2222222222222222UL);
				FlipCell128Word(ref cell, word, 1UL);
				Assert.AreEqual((0x1111111111111111UL, 0x2222222222222222UL), cell.Unprotect());
				Assert.AreEqual(baseline + 1, fired);
			}

			TamperingNotifier.ResetThrottleForTesting();
			int both = fired;
			Cell128 dead = default;
			dead.Protect(1UL, 2UL);
			FlipCell128Word(ref dead, 0, 1UL);
			FlipCell128Word(ref dead, 4, 1UL);
			Assert.Throws<TamperedException>(() => dead.Unprotect());
			Assert.AreEqual(both + 1, fired);
		}

		[Test]
		public void Cell128_HiWordFault_HealSingle_ThrowBoth()
		{
			// The pair tag covers both halves: a hi-only flip is detected.
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int word in new int[] { 2, 6 })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				Cell128 cell = default;
				cell.Protect(0x5555555555555555UL, 0x6666666666666666UL);
				FlipCell128Word(ref cell, word, 1UL);
				Assert.AreEqual((0x5555555555555555UL, 0x6666666666666666UL), cell.Unprotect());
				Assert.AreEqual(baseline + 1, fired);
			}

			TamperingNotifier.ResetThrottleForTesting();
			int both = fired;
			Cell128 dead = default;
			dead.Protect(1UL, 2UL);
			FlipCell128Word(ref dead, 2, 1UL);
			FlipCell128Word(ref dead, 6, 1UL);
			Assert.Throws<TamperedException>(() => dead.Unprotect());
			Assert.AreEqual(both + 1, fired);
		}

		[Test]
		public void Cell_WordSweep_SingleCopy_Heals()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int heals = 0;

			for (int word = 0; word < Cell.WordCount; word++)
			{
				ulong[] masks = IsCellTagWord(word) ? Masks32 : Masks64;
				foreach (ulong mask in masks)
				{
					TamperingNotifier.ResetThrottleForTesting();
					int baseline = fired;
					Cell cell = default;
					cell.Protect(123456789UL);
					FlipCellWord(ref cell, word, mask);
					Assert.AreEqual(123456789UL, cell.Unprotect());
					Assert.AreEqual(baseline + 1, fired);
					heals++;
				}
			}
			Assert.AreEqual(Cell.WordCount * 4, heals);
		}

		[Test]
		public void Cell128_WordSweep_SingleCopy_Heals()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int heals = 0;

			for (int word = 0; word < Cell128.WordCount; word++)
			{
				ulong[] masks = IsCell128TagWord(word) ? Masks32 : Masks64;
				foreach (ulong mask in masks)
				{
					TamperingNotifier.ResetThrottleForTesting();
					int baseline = fired;
					Cell128 cell = default;
					cell.Protect(0x7777777777777777UL, 0x8888888888888888UL);
					FlipCell128Word(ref cell, word, mask);
					Assert.AreEqual((0x7777777777777777UL, 0x8888888888888888UL), cell.Unprotect());
					Assert.AreEqual(baseline + 1, fired);
					heals++;
				}
			}
			Assert.AreEqual(Cell128.WordCount * 4, heals);
		}

		[Test]
		public void SameFaultBothCopies_Throws()
		{
			// No copy survives to heal from: symmetric faults fail closed.
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			Cell cell = default;
			cell.Protect(42UL);
			FlipCellWord(ref cell, 1, 1UL);
			FlipCellWord(ref cell, 4, 1UL);
			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.AreEqual(baseline + 1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			Cell128 pair = default;
			pair.Protect(42UL, 43UL);
			FlipCell128Word(ref pair, 2, 0xFFFFFFFFFFFFFFFFUL);
			FlipCell128Word(ref pair, 6, 0xFFFFFFFFFFFFFFFFUL);
			Assert.Throws<TamperedException>(() => pair.Unprotect());
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void SwappedCopies_Detected()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// Ciphers exchanged across copies: neither tag verifies, throws.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			Cell cell = default;
			cell.Protect(77UL);
			SwapCellWords(ref cell, 1, 4);
			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.AreEqual(baseline + 1, fired);

			// Salts exchanged across copies: both tags die, throws.
			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			Cell salts = default;
			salts.Protect(77UL);
			SwapCellWords(ref salts, 0, 3);
			Assert.Throws<TamperedException>(() => salts.Unprotect());
			Assert.AreEqual(baseline + 1, fired);

			// cLo/cHi swapped inside one pair copy: order-sensitive tag kills
			// that copy, the backup heals the read.
			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			Cell128 pair = default;
			pair.Protect(0x9999999999999999UL, 0xAAAAAAAAAAAAAAAAUL);
			SwapCell128Words(ref pair, 1, 2);
			Assert.AreEqual((0x9999999999999999UL, 0xAAAAAAAAAAAAAAAAUL), pair.Unprotect());
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void String_HeapTruncate_HealsSingle_ThrowsBoth()
		{
			const string plain = "backup me, this string is long enough for the heap";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object box = HeapString(plain);
			ulong[] arr = (ulong[])GetField(box, "_ciphers");
			SetField(box, "_ciphers", arr.Take(arr.Length / 2).ToArray());
			SecureString p = (SecureString)box;
			Assert.AreEqual(plain, p.Decrypted);
			Assert.AreEqual(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object box2 = HeapString(plain);
			ulong[] a2 = (ulong[])GetField(box2, "_ciphers");
			ulong[] b2 = (ulong[])GetField(box2, "_ciphersB");
			SetField(box2, "_ciphers", a2.Take(a2.Length / 2).ToArray());
			SetField(box2, "_ciphersB", b2.Take(b2.Length / 2).ToArray());
			SecureString q = (SecureString)box2;
			Assert.Throws<TamperedException>(() =>
			{
				string _ = q.Decrypted;
			});
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void String_HeapExtend_HealsSingle_ThrowsBoth()
		{
			const string plain = "backup me, this string is long enough for the heap";
			ulong[] extra = new ulong[] { 1UL, 2UL, 3UL };
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object box = HeapString(plain);
			ulong[] arr = (ulong[])GetField(box, "_ciphers");
			SetField(box, "_ciphers", arr.Concat(extra).ToArray());
			SecureString p = (SecureString)box;
			Assert.AreEqual(plain, p.Decrypted);
			Assert.AreEqual(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object box2 = HeapString(plain);
			SetField(
				box2,
				"_ciphers",
				((ulong[])GetField(box2, "_ciphers")).Concat(extra).ToArray()
			);
			SetField(
				box2,
				"_ciphersB",
				((ulong[])GetField(box2, "_ciphersB")).Concat(extra).ToArray()
			);
			SecureString q = (SecureString)box2;
			Assert.Throws<TamperedException>(() =>
			{
				string _ = q.Decrypted;
			});
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void String_InlineLengthSmuggle_Throws()
		{
			// The inline tag covers length plus live words: a smuggled length
			// breaks both copies (the length is shared state).
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object box = new SecureString("hi");
			SetField(box, "_length", 3);
			SecureString p = (SecureString)box;
			Assert.Throws<TamperedException>(() =>
			{
				string _ = p.Decrypted;
			});
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void String_CrossValueSplice_Heals()
		{
			const string keeper = "KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK";
			const string donor = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// Cipher words smuggled from another sealed value no longer match
			// the target's tag: the surviving copy heals the read.
			object box = HeapString(keeper);
			object donorBox = HeapString(donor);
			SetField(box, "_ciphers", ((ulong[])GetField(donorBox, "_ciphers")).ToArray());
			SecureString p = (SecureString)box;
			Assert.AreEqual(keeper, p.Decrypted);
			Assert.AreEqual(1, fired);

			// Backup salt overwritten with the primary salt: copy B's tag
			// (bound to the old salt) dies, copy A heals the read.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object box2 = HeapString(keeper);
			SetField(box2, "_saltB", GetField(box2, "_salt"));
			SecureString q = (SecureString)box2;
			Assert.AreEqual(keeper, q.Decrypted);
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void IntExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int value in new int[] { int.MinValue, int.MaxValue })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				object box = (SecureInt)value;
				object cell = box.GetType().GetField("_cell", Flags).GetValue(box);
				Type cellType = cell.GetType();
				FieldInfo cipher = cellType.GetField("_cipher", Flags);
				cipher.SetValue(cell, (ulong)cipher.GetValue(cell) ^ 1UL);
				box.GetType().GetField("_cell", Flags).SetValue(box, cell);
				SecureInt p = (SecureInt)box;
				Assert.AreEqual(value, p.Decrypted);
				Assert.AreEqual(baseline + 1, fired);
			}
		}

		[Test]
		public void ComputeTag_KeySetAndRawK3_Equivalent()
		{
			KeySet rk = Vault.DeriveProcessKeys(0x9E3779B97F4A7C15UL);
			foreach (
				ulong cipher in new ulong[] { 0UL, 1UL, 0xFFFFFFFFFFFFFFFFUL, 0x123456789ABCDEF0UL }
			)
			{
				Assert.AreEqual(Mac.ComputeTag(cipher, rk.K3), Mac.ComputeTag(cipher, rk));
			}
			Assert.AreEqual(
				Mac.ComputeTag(0xAAAAAAAAAAAAAAAAUL, 0x5555555555555555UL, rk.K3),
				Mac.ComputeTag(0xAAAAAAAAAAAAAAAAUL, 0x5555555555555555UL, rk)
			);

			// Span form is length-sensitive (empty included) and deterministic.
			KeySet spanKey = Vault.DeriveProcessKeys(0x0123456789ABCDEFUL);
			ulong[] words = new ulong[]
			{
				0x1111111111111111UL,
				0x2222222222222222UL,
				0x3333333333333333UL,
			};
			uint empty = Mac.ComputeTag(new ReadOnlySpan<ulong>(new ulong[0]), spanKey);
			uint one = Mac.ComputeTag(new ReadOnlySpan<ulong>(words, 0, 1), spanKey);
			uint three = Mac.ComputeTag(new ReadOnlySpan<ulong>(words, 0, 3), spanKey);
			Assert.AreNotEqual(empty, one);
			Assert.AreNotEqual(one, three);
			Assert.AreEqual(three, Mac.ComputeTag(new ReadOnlySpan<ulong>(words, 0, 3), spanKey));
		}
	}
}
#endif
