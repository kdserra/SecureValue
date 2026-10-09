using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	// Shares the isolated TamperNotifier collection (see ThreadSafetyTests):
	// exact event counts below would break if another collection triggered
	// tamper concurrently.
	[Collection("TamperNotifier")]
	public class MacTests
	{
		public MacTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

		// Cell words: [salt, cipher, tag, saltB, cipherB, tagB].
		// Cell128 words: [salt, cipherLo, cipherHi, tag, saltB, cipherLoB, cipherHiB, tagB].
		// Tag words pack a uint; every other word is a full ulong.
		private static bool IsCellTagWord(int index) => index == 2 || index == 5;

		private static bool IsCell128TagWord(int index) => index == 3 || index == 7;

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
			(words[a], words[b]) = (words[b], words[a]);
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
			(words[a], words[b]) = (words[b], words[a]);
			cell.RestoreWords(words);
		}

		private static object ReadCell(object boxed)
		{
			return boxed.GetType().GetField("_cell", Flags)!.GetValue(boxed)!;
		}

		private static void WriteCell(object boxed, object cell)
		{
			boxed.GetType().GetField("_cell", Flags)!.SetValue(boxed, cell);
		}

		private static void FlipWord(object cell, string name, ulong mask)
		{
			FieldInfo f = cell.GetType().GetField(name, Flags)!;
			if (f.FieldType == typeof(uint))
			{
				f.SetValue(cell, (uint)f.GetValue(cell)! ^ (uint)mask);
				return;
			}
			f.SetValue(cell, (ulong)f.GetValue(cell)! ^ mask);
		}

		private static object GetField(object boxed, string name)
		{
			return boxed.GetType().GetField(name, Flags)!.GetValue(boxed)!;
		}

		private static void SetField(object boxed, string name, object value)
		{
			boxed.GetType().GetField(name, Flags)!.SetValue(boxed, value);
		}

		private static ulong[] StringArray(object boxed, string name)
		{
			return (ulong[])GetField(boxed, name)!;
		}

		private static SecureString HeapString(string plain)
		{
			// Lengths over 16 characters take the heap store by construction.
			SecureString s = new SecureString(plain);
			Assert.True(s.Length > 16);
			Assert.NotNull(StringArray(s, "_ciphers"));
			return s;
		}

		// ---------- 1. salt-only and tag-only faults ----------

		[Theory]
		[InlineData(0)] // primary salt
		[InlineData(3)] // backup salt
		public void Cell_SaltFault_SingleCopy_HealsWithEvent(int word)
		{
			const ulong plain = 0x0BADF00DDEADBEEFUL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell);
			cell.Protect(plain);
			FlipCellWord(ref cell, word, 1UL);

			Assert.Equal(plain, cell.Unprotect());
			Assert.Equal(1, fired);
			Assert.Equal(plain, cell.Unprotect()); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell_SaltFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell);
			cell.Protect(7UL);
			FlipCellWord(ref cell, 0, 1UL);
			FlipCellWord(ref cell, 3, 1UL);

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData(2)] // primary tag
		[InlineData(5)] // backup tag
		public void Cell_TagFault_SingleCopy_HealsWithEvent(int word)
		{
			const ulong plain = 999UL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell);
			cell.Protect(plain);
			FlipCellWord(ref cell, word, 1UL);

			Assert.Equal(plain, cell.Unprotect());
			Assert.Equal(1, fired);
			Assert.Equal(plain, cell.Unprotect()); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell_TagFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell);
			cell.Protect(7UL);
			FlipCellWord(ref cell, 2, 1UL);
			FlipCellWord(ref cell, 5, 1UL);

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData(0)] // primary salt
		[InlineData(4)] // backup salt
		public void Cell128_SaltFault_SingleCopy_HealsWithEvent(int word)
		{
			const ulong lo = 0x1111111111111111UL;
			const ulong hi = 0x2222222222222222UL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(lo, hi);
			FlipCell128Word(ref cell, word, 1UL);

			Assert.Equal((lo, hi), cell.Unprotect());
			Assert.Equal(1, fired);
			Assert.Equal((lo, hi), cell.Unprotect()); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell128_SaltFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(1UL, 2UL);
			FlipCell128Word(ref cell, 0, 1UL);
			FlipCell128Word(ref cell, 4, 1UL);

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData(3)] // primary tag
		[InlineData(7)] // backup tag
		public void Cell128_TagFault_SingleCopy_HealsWithEvent(int word)
		{
			const ulong lo = 0x3333333333333333UL;
			const ulong hi = 0x4444444444444444UL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(lo, hi);
			FlipCell128Word(ref cell, word, 1UL);

			Assert.Equal((lo, hi), cell.Unprotect());
			Assert.Equal(1, fired);
			Assert.Equal((lo, hi), cell.Unprotect()); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell128_TagFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(1UL, 2UL);
			FlipCell128Word(ref cell, 3, 1UL);
			FlipCell128Word(ref cell, 7, 1UL);

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		// ---------- 2. Cell128 hi-word-only faults ----------

		[Theory]
		[InlineData(2)] // primary cipherHi
		[InlineData(6)] // backup cipherHi
		public void Cell128_HiWordFault_SingleCopy_HealsWithEvent(int word)
		{
			// The pair tag covers both halves, so a hi-only flip is detected
			// even though the lo word is untouched.
			const ulong lo = 0x5555555555555555UL;
			const ulong hi = 0x6666666666666666UL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(lo, hi);
			FlipCell128Word(ref cell, word, 1UL);

			Assert.Equal((lo, hi), cell.Unprotect());
			Assert.Equal(1, fired);
			Assert.Equal((lo, hi), cell.Unprotect()); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell128_HiWordFault_BothCopies_Throws()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cell = default(Cell128);
			cell.Protect(1UL, 2UL);
			FlipCell128Word(ref cell, 2, 1UL);
			FlipCell128Word(ref cell, 6, 1UL);

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		// ---------- 3. word-index sweep, single-bit and multi-bit ----------

		[Fact]
		public void Cell_WordSweep_SingleCopy_Heals()
		{
			const ulong plain = 123456789UL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			int heals = 0;
			for (int word = 0; word < Cell.WordCount; word++)
			{
				// Tag words pack a uint: keep masks in the low 32 bits so the
				// flip survives the (uint) truncation in RestoreWords.
				ulong[] masks = IsCellTagWord(word) ? Masks32 : Masks64;
				foreach (ulong mask in masks)
				{
					TamperingNotifier.ResetThrottleForTesting();
					int baseline = fired;
					var cell = default(Cell);
					cell.Protect(plain);
					FlipCellWord(ref cell, word, mask);
					Assert.Equal(plain, cell.Unprotect());
					Assert.Equal(baseline + 1, fired);
					heals++;
				}
			}
			Assert.Equal(Cell.WordCount * 4, heals); // 6 words x 4 masks
		}

		[Fact]
		public void Cell128_WordSweep_SingleCopy_Heals()
		{
			const ulong lo = 0x7777777777777777UL;
			const ulong hi = 0x8888888888888888UL;
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
					var cell = default(Cell128);
					cell.Protect(lo, hi);
					FlipCell128Word(ref cell, word, mask);
					Assert.Equal((lo, hi), cell.Unprotect());
					Assert.Equal(baseline + 1, fired);
					heals++;
				}
			}
			Assert.Equal(Cell128.WordCount * 4, heals); // 8 words x 4 masks
		}

		// ---------- 4. identical fault in both copies ----------

		[Fact]
		public void Cell_SameFaultBothCopies_Throws()
		{
			// No copy survives to heal from: even a symmetric fault fails closed.
			var cases = new (int A, int B, ulong Mask)[]
			{
				(1, 4, 1UL),
				(1, 4, 0xFFFFFFFFFFFFFFFFUL),
				(0, 3, 1UL),
				(0, 3, 0xFFUL),
				(2, 5, 1UL),
				(2, 5, 0xFFFFFFFFUL),
			};
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach ((int a, int b, ulong mask) in cases)
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				var cell = default(Cell);
				cell.Protect(42UL);
				FlipCellWord(ref cell, a, mask);
				FlipCellWord(ref cell, b, mask);
				Assert.Throws<TamperedException>(() => cell.Unprotect());
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Fact]
		public void Cell128_SameFaultBothCopies_Throws()
		{
			var cases = new (int A, int B, ulong Mask)[]
			{
				(1, 5, 1UL),
				(2, 6, 0xFFFFFFFFFFFFFFFFUL),
				(0, 4, 1UL),
				(3, 7, 1UL),
			};
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach ((int a, int b, ulong mask) in cases)
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				var cell = default(Cell128);
				cell.Protect(42UL, 43UL);
				FlipCell128Word(ref cell, a, mask);
				FlipCell128Word(ref cell, b, mask);
				Assert.Throws<TamperedException>(() => cell.Unprotect());
				Assert.Equal(baseline + 1, fired);
			}
		}

		// ---------- 5. swap / reorder ----------

		[Fact]
		public void Cell_SwappedCopies_Detected()
		{
			// Moving a word into the other copy breaks both tags: neither copy
			// verifies under the foreign salt/cipher, so reads fail closed.
			var swaps = new (int A, int B)[]
			{
				(1, 4), // ciphers A<->B
				(0, 3), // salts A<->B
			};
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach ((int a, int b) in swaps)
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				var cell = default(Cell);
				cell.Protect(77UL);
				SwapCellWords(ref cell, a, b);
				Assert.Throws<TamperedException>(() => cell.Unprotect());
				Assert.Equal(baseline + 1, fired);
			}

			// Tags A<->B: same shape, but guard the 2^-32 tie (identical tags
			// would make the swap a no-op) by resealing until they differ.
			TamperingNotifier.ResetThrottleForTesting();
			int tagBaseline = fired;
			var tagCell = default(Cell);
			tagCell.Protect(77UL);
			ulong[] snapshot = new ulong[Cell.WordCount];
			tagCell.CopyWords(snapshot);
			int guard = 0;
			while (snapshot[2] == snapshot[5] && guard++ < 10)
			{
				tagCell.Protect(77UL);
				tagCell.CopyWords(snapshot);
			}
			Assert.NotEqual(snapshot[2], snapshot[5]);
			SwapCellWords(ref tagCell, 2, 5);
			Assert.Throws<TamperedException>(() => tagCell.Unprotect());
			Assert.Equal(tagBaseline + 1, fired);
		}

		[Fact]
		public void Cell128_ReorderedWords_Detected()
		{
			const ulong lo = 0x9999999999999999UL;
			const ulong hi = 0xAAAAAAAAAAAAAAAAUL;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// cLo/cHi swapped inside copy A: the pair tag is order-sensitive,
			// so copy A dies while copy B heals the read.
			var cell = default(Cell128);
			cell.Protect(lo, hi);
			SwapCell128Words(ref cell, 1, 2);
			Assert.Equal((lo, hi), cell.Unprotect());
			Assert.Equal(1, fired);

			// cLo swapped across copies: both tags break, the read throws.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			var cross = default(Cell128);
			cross.Protect(lo, hi);
			SwapCell128Words(ref cross, 1, 5);
			Assert.Throws<TamperedException>(() => cross.Unprotect());
			Assert.Equal(baseline + 1, fired);

			// Salts swapped across copies: both tags break, the read throws.
			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			var salts = default(Cell128);
			salts.Protect(lo, hi);
			SwapCell128Words(ref salts, 0, 4);
			Assert.Throws<TamperedException>(() => salts.Unprotect());
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_InlineWordReorder_Detected()
		{
			const string plain = "abcdefgh"; // 8 chars: inline store
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// Reorder within copy A: backup copy B heals the read.
			object boxed = new SecureString(plain);
			Assert.True(((SecureString)boxed).IsInline);
			ulong w0 = (ulong)GetField(boxed, "_w0");
			SetField(boxed, "_w0", GetField(boxed, "_w1"));
			SetField(boxed, "_w1", w0);
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			// Swap a word across copies: both inline tags break, throws.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = new SecureString(plain);
			ulong v0 = (ulong)GetField(boxed2, "_w0");
			SetField(boxed2, "_w0", GetField(boxed2, "_w0B"));
			SetField(boxed2, "_w0B", v0);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		// ---------- 6. truncate / extend / splice ----------

		[Fact]
		public void String_HeapTruncate_HealsSingle_ThrowsBoth()
		{
			const string plain = "backup me, this string is long enough for the heap";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// One copy truncated: the tag no longer matches, the good copy heals.
			object boxed = HeapString(plain);
			ulong[] arr = StringArray(boxed, "_ciphers");
			SetField(boxed, "_ciphers", arr.Take(arr.Length / 2).ToArray());
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			// Both copies truncated: nothing verifies, the read fails closed
			// (and never silently accepts the shortened array).
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(plain);
			ulong[] a2 = StringArray(boxed2, "_ciphers");
			ulong[] b2 = StringArray(boxed2, "_ciphersB");
			SetField(boxed2, "_ciphers", a2.Take(a2.Length / 2).ToArray());
			SetField(boxed2, "_ciphersB", b2.Take(b2.Length / 2).ToArray());
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_HeapExtend_HealsSingle_ThrowsBoth()
		{
			const string plain = "backup me, this string is long enough for the heap";
			ulong[] extra = new ulong[] { 1UL, 2UL, 3UL };
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// One copy extended: the tag no longer matches, the good copy heals.
			object boxed = HeapString(plain);
			ulong[] arr = StringArray(boxed, "_ciphers");
			SetField(boxed, "_ciphers", arr.Concat(extra).ToArray());
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			// Both copies extended: nothing verifies, the read fails closed.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(plain);
			SetField(boxed2, "_ciphers", StringArray(boxed2, "_ciphers").Concat(extra).ToArray());
			SetField(boxed2, "_ciphersB", StringArray(boxed2, "_ciphersB").Concat(extra).ToArray());
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_InlineLengthSmuggle_Throws()
		{
			// The inline tag covers the length plus the live words: a smuggled
			// length breaks both copies' tags (the length is shared state).
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = new SecureString("hi");
			SetField(boxed, "_length", 3);
			var p = (SecureString)boxed;
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = new SecureString("hi");
			SetField(boxed2, "_length", 1);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_HeapSaltFaults_HealSingle_ThrowBoth()
		{
			const string plain = "salty heap string, long enough to live off-inline!!";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// The tag key derives from the salt: flipping salt bits kills the tag.
			object boxed = HeapString(plain);
			SetField(boxed, "_salt", (ulong)GetField(boxed, "_salt") ^ 1UL);
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(plain);
			SetField(boxed2, "_salt", (ulong)GetField(boxed2, "_salt") ^ 1UL);
			SetField(boxed2, "_saltB", (ulong)GetField(boxed2, "_saltB") ^ 1UL);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_TagFaults_HealSingle_ThrowBoth()
		{
			const string plain = "tagged heap string, long enough to live off-inline!";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = HeapString(plain);
			SetField(boxed, "_tag", (uint)GetField(boxed, "_tag") ^ 1U);
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(plain);
			SetField(boxed2, "_tag", (uint)GetField(boxed2, "_tag") ^ 1U);
			SetField(boxed2, "_tagB", (uint)GetField(boxed2, "_tagB") ^ 1U);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_InlineCipherFaults_HealSingle_ThrowBoth()
		{
			const string plain = "inline12"; // 8 chars: inline store
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = new SecureString(plain);
			SetField(boxed, "_w0", (ulong)GetField(boxed, "_w0") ^ 1UL);
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = new SecureString(plain);
			SetField(boxed2, "_w0", (ulong)GetField(boxed2, "_w0") ^ 1UL);
			SetField(boxed2, "_w0B", (ulong)GetField(boxed2, "_w0B") ^ 1UL);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_CrossValueSplice_Heals()
		{
			// Cipher words smuggled from one sealed value into another no longer
			// match the target's tag: the surviving copy heals the read.
			const string keeper = "KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK";
			const string donor = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";
			Assert.Equal(keeper.Length, donor.Length);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = HeapString(keeper);
			object donorBox = HeapString(donor);
			SetField(boxed, "_ciphers", ((ulong[])GetField(donorBox, "_ciphers")).ToArray());
			var p = (SecureString)boxed;
			Assert.Equal(keeper, p.Decrypted);
			Assert.Equal(1, fired);

			// Same for a smuggled salt: the tag key changes, the tag dies.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(keeper);
			object donor2 = HeapString(donor);
			SetField(boxed2, "_salt", GetField(donor2, "_salt"));
			var q = (SecureString)boxed2;
			Assert.Equal(keeper, q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void String_WithinValueCrossSalt_HealsThenSwappedThrows()
		{
			const string plain = "cross salt string, long enough for the heap!!!!";
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// Backup salt overwritten with the primary salt: copy B's tag (bound
			// to the old salt) dies, copy A heals the read.
			object boxed = HeapString(plain);
			SetField(boxed, "_saltB", GetField(boxed, "_salt"));
			var p = (SecureString)boxed;
			Assert.Equal(plain, p.Decrypted);
			Assert.Equal(1, fired);

			// Salts exchanged: both tags die, the read fails closed.
			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = HeapString(plain);
			object saltA = GetField(boxed2, "_salt");
			SetField(boxed2, "_salt", GetField(boxed2, "_saltB"));
			SetField(boxed2, "_saltB", saltA);
			var q = (SecureString)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void BigInteger_Truncate_HealsSingle_ThrowsBoth()
		{
			BigInteger expected = BigInteger.Pow(2, 128) + 7;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = new SecureBigInteger(expected);
			ulong[] arr = (ulong[])GetField(boxed, "_ciphers");
			SetField(boxed, "_ciphers", arr.Take(arr.Length - 1).ToArray());
			var p = (SecureBigInteger)boxed;
			Assert.Equal(expected, p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = new SecureBigInteger(expected);
			ulong[] a2 = (ulong[])GetField(boxed2, "_ciphers");
			ulong[] b2 = (ulong[])GetField(boxed2, "_ciphersB");
			SetField(boxed2, "_ciphers", a2.Take(a2.Length - 1).ToArray());
			SetField(boxed2, "_ciphersB", b2.Take(b2.Length - 1).ToArray());
			var q = (SecureBigInteger)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void BigInteger_Extend_HealsSingle_ThrowsBoth()
		{
			BigInteger expected = BigInteger.Pow(2, 128) + 7;
			ulong[] extra = new ulong[] { 0xDEADBEEFUL };
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = new SecureBigInteger(expected);
			SetField(
				boxed,
				"_ciphers",
				((ulong[])GetField(boxed, "_ciphers")).Concat(extra).ToArray()
			);
			var p = (SecureBigInteger)boxed;
			Assert.Equal(expected, p.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object boxed2 = new SecureBigInteger(expected);
			SetField(
				boxed2,
				"_ciphers",
				((ulong[])GetField(boxed2, "_ciphers")).Concat(extra).ToArray()
			);
			SetField(
				boxed2,
				"_ciphersB",
				((ulong[])GetField(boxed2, "_ciphersB")).Concat(extra).ToArray()
			);
			var q = (SecureBigInteger)boxed2;
			Assert.Throws<TamperedException>(() => _ = q.Decrypted);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void BigInteger_CrossValueSplice_Heals()
		{
			// Same word count on both sides so only the tag can tell them apart.
			BigInteger keeper = BigInteger.Pow(2, 128) + 7;
			BigInteger donor = BigInteger.Pow(2, 128) + 9;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object boxed = new SecureBigInteger(keeper);
			object donorBox = new SecureBigInteger(donor);
			Assert.Equal(
				((ulong[])GetField(boxed, "_ciphers")).Length,
				((ulong[])GetField(donorBox, "_ciphers")).Length
			);
			SetField(boxed, "_ciphers", ((ulong[])GetField(donorBox, "_ciphers")).ToArray());
			var p = (SecureBigInteger)boxed;
			Assert.Equal(keeper, p.Decrypted);
			Assert.Equal(1, fired);
		}

		// ---------- 7. full-range inputs ----------

		[Fact]
		public void ComputeTag_FullRangeInputs_DeterministicAndDistinct()
		{
			KeySet rk = Vault.DeriveProcessKeys(0x123456789ABCDEF0UL);
			long intMin = int.MinValue;
			long intMax = int.MaxValue;
			uint uintMin = unchecked((uint)int.MinValue);
			uint uintMax = (uint)int.MaxValue;
			var inputs = new List<ulong>
			{
				0UL,
				ulong.MaxValue,
				(ulong)intMin,
				(ulong)intMax,
				uintMin,
				uintMax,
			};
			for (int i = 0; i < 64; i++)
			{
				inputs.Add(1UL << i);
			}
			uint tagOfZero = Mac.ComputeTag(0UL, rk);
			foreach (ulong cipher in inputs)
			{
				Assert.Equal(Mac.ComputeTag(cipher, rk), Mac.ComputeTag(cipher, rk));
				if (cipher != 0UL)
				{
					Assert.NotEqual(tagOfZero, Mac.ComputeTag(cipher, rk));
				}
			}
		}

		[Fact]
		public void Cell_PrimitiveExtremes_RoundTripAndFault()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (ulong plain in new ulong[] { 0UL, ulong.MaxValue })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				var cell = default(Cell);
				cell.Protect(plain);
				Assert.Equal(plain, cell.Unprotect());
				Assert.Equal(baseline, fired); // clean read: silent
				FlipCellWord(ref cell, 1, 1UL);
				Assert.Equal(plain, cell.Unprotect());
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Fact]
		public void Cell128_PrimitiveExtremes_RoundTripAndFault()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var cases = new (ulong Lo, ulong Hi)[] { (0UL, 0UL), (ulong.MaxValue, ulong.MaxValue) };
			foreach ((ulong lo, ulong hi) in cases)
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				var cell = default(Cell128);
				cell.Protect(lo, hi);
				Assert.Equal((lo, hi), cell.Unprotect());
				Assert.Equal(baseline, fired); // clean read: silent
				FlipCell128Word(ref cell, 2, 1UL);
				Assert.Equal((lo, hi), cell.Unprotect());
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Theory]
		[InlineData(sbyte.MinValue)]
		[InlineData((sbyte)0)]
		[InlineData(sbyte.MaxValue)]
		public void SByte_Extremes_RoundTrip(sbyte value)
		{
			SecureSByte p = value;
			Assert.Equal(value, (sbyte)p);
		}

		[Theory]
		[InlineData(byte.MinValue)]
		[InlineData(byte.MaxValue)]
		public void Byte_Extremes_RoundTrip(byte value)
		{
			SecureByte p = value;
			Assert.Equal(value, (byte)p);
		}

		[Theory]
		[InlineData(int.MinValue)]
		[InlineData(0)]
		[InlineData(int.MaxValue)]
		public void Int_Extremes_RoundTrip(int value)
		{
			SecureInt p = value;
			Assert.Equal(value, (int)p);
		}

		[Theory]
		[InlineData(long.MinValue)]
		[InlineData(0L)]
		[InlineData(long.MaxValue)]
		public void Long_Extremes_RoundTrip(long value)
		{
			SecureLong p = value;
			Assert.Equal(value, (long)p);
		}

		[Fact]
		public void Decimal_Extremes_RoundTrip()
		{
			foreach (
				decimal value in new decimal[] { decimal.MinValue, -1m, 0m, 1m, decimal.MaxValue }
			)
			{
				SecureDecimal p = value;
				Assert.Equal(value, (decimal)p);
			}
		}

		[Fact]
		public void String_Extremes_RoundTrip()
		{
			SecureString empty = new SecureString(string.Empty);
			Assert.Equal(string.Empty, empty.Decrypted);
			SecureString boundary = new SecureString(new string('M', 16));
			Assert.True(boundary.IsInline);
			Assert.Equal(new string('M', 16), boundary.Decrypted);
			SecureString heap = HeapString(new string('L', 100));
			Assert.Equal(new string('L', 100), heap.Decrypted);
		}

		[Fact]
		public void IntExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (int value in new int[] { int.MinValue, int.MaxValue })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				object boxed = new SecureInt(value);
				object cell = ReadCell(boxed);
				FlipWord(cell, "_cipher", 1UL);
				WriteCell(boxed, cell);
				var p = (SecureInt)boxed;
				Assert.Equal(value, p.Decrypted);
				Assert.Equal(baseline + 1, fired);

				object boxed2 = new SecureInt(value);
				object cell2 = ReadCell(boxed2);
				FlipWord(cell2, "_cipher", 1UL);
				FlipWord(cell2, "_cipherB", 1UL);
				WriteCell(boxed2, cell2);
				var q = (SecureInt)boxed2;
				Assert.Throws<TamperedException>(() => _ = q.Decrypted);
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Fact]
		public void LongExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (long value in new long[] { long.MinValue, long.MaxValue })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				object boxed = new SecureLong(value);
				object cell = ReadCell(boxed);
				FlipWord(cell, "_cipher", 1UL);
				WriteCell(boxed, cell);
				var p = (SecureLong)boxed;
				Assert.Equal(value, p.Decrypted);
				Assert.Equal(baseline + 1, fired);

				object boxed2 = new SecureLong(value);
				object cell2 = ReadCell(boxed2);
				FlipWord(cell2, "_cipher", 1UL);
				FlipWord(cell2, "_cipherB", 1UL);
				WriteCell(boxed2, cell2);
				var q = (SecureLong)boxed2;
				Assert.Throws<TamperedException>(() => _ = q.Decrypted);
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Fact]
		public void SByteByteExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object sboxed = new SecureSByte(sbyte.MinValue);
			object scell = ReadCell(sboxed);
			FlipWord(scell, "_cipher", 1UL);
			WriteCell(sboxed, scell);
			var sp = (SecureSByte)sboxed;
			Assert.Equal(sbyte.MinValue, (sbyte)sp);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object bboxed = new SecureByte(byte.MaxValue);
			object bcell = ReadCell(bboxed);
			FlipWord(bcell, "_cipherB", 1UL);
			WriteCell(bboxed, bcell);
			var bp = (SecureByte)bboxed;
			Assert.Equal(byte.MaxValue, (byte)bp);
			Assert.Equal(baseline + 1, fired);
		}

		[Fact]
		public void DecimalExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			foreach (decimal value in new decimal[] { decimal.MinValue, decimal.MaxValue })
			{
				TamperingNotifier.ResetThrottleForTesting();
				int baseline = fired;
				object boxed = new SecureDecimal(value);
				object cell = ReadCell(boxed);
				FlipWord(cell, "_cipherLo", 1UL);
				WriteCell(boxed, cell);
				var p = (SecureDecimal)boxed;
				Assert.Equal(value, p.Decrypted);
				Assert.Equal(baseline + 1, fired);

				object boxed2 = new SecureDecimal(value);
				object cell2 = ReadCell(boxed2);
				FlipWord(cell2, "_cipherLo", 1UL);
				FlipWord(cell2, "_cipherLoB", 1UL);
				WriteCell(boxed2, cell2);
				var q = (SecureDecimal)boxed2;
				Assert.Throws<TamperedException>(() => _ = q.Decrypted);
				Assert.Equal(baseline + 1, fired);
			}
		}

		[Fact]
		public void StringExtremes_FaultDetection()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			// Inline boundary (16 chars): one damaged word heals, both throw.
			object iboxed = new SecureString(new string('M', 16));
			SetField(iboxed, "_w1", (ulong)GetField(iboxed, "_w1") ^ 1UL);
			var ip = (SecureString)iboxed;
			Assert.Equal(new string('M', 16), ip.Decrypted);
			Assert.Equal(1, fired);

			TamperingNotifier.ResetThrottleForTesting();
			int baseline = fired;
			object iboxed2 = new SecureString(new string('M', 16));
			SetField(iboxed2, "_w1", (ulong)GetField(iboxed2, "_w1") ^ 1UL);
			SetField(iboxed2, "_w1B", (ulong)GetField(iboxed2, "_w1B") ^ 1UL);
			var iq = (SecureString)iboxed2;
			Assert.Throws<TamperedException>(() => _ = iq.Decrypted);
			Assert.Equal(baseline + 1, fired);

			// Long heap string: same shape through the span seal path.
			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			object hboxed = HeapString(new string('L', 100));
			ulong[] arr = StringArray(hboxed, "_ciphers");
			arr[arr.Length - 1] ^= 1UL;
			var hp = (SecureString)hboxed;
			Assert.Equal(new string('L', 100), hp.Decrypted);
			Assert.Equal(baseline + 1, fired);

			// Empty has no sealed words: faults are vacuous, reads stay empty
			// without dispatching (length zero skips verification by design).
			TamperingNotifier.ResetThrottleForTesting();
			baseline = fired;
			object eboxed = new SecureString(string.Empty);
			SetField(eboxed, "_salt", (ulong)GetField(eboxed, "_salt") ^ 1UL);
			var ep = (SecureString)eboxed;
			Assert.Equal(string.Empty, ep.Decrypted);
			Assert.Equal(baseline, fired);
		}

		// ---------- 8. tag statistics ----------

		[Fact]
		public void Tag_Deterministic()
		{
			KeySet rk = Vault.DeriveProcessKeys(0x0FEDCBA987654321UL);
			const ulong cipher = 0x0123456789ABCDEFUL;
			Assert.Equal(Mac.ComputeTag(cipher, rk), Mac.ComputeTag(cipher, rk));
			Assert.Equal(Mac.ComputeTag(cipher, 1UL, rk), Mac.ComputeTag(cipher, 1UL, rk));
			ulong[] words = new ulong[] { cipher, 2UL, 3UL };
			Assert.Equal(Mac.ComputeTag(words.AsSpan(), rk), Mac.ComputeTag(words.AsSpan(), rk));
		}

		[Fact]
		public void Tag_Uniformity()
		{
			// Fixed seed: the same 2000 inputs every run. Buckets split the tag
			// on its top 3 bits (expectation 250 each); bounds are ~7 sigma out.
			const int samples = 2000;
			KeySet rk = Vault.DeriveProcessKeys(0x0FEDCBA987654321UL);
			var rng = new Random(42);
			int[] buckets = new int[8];
			for (int i = 0; i < samples; i++)
			{
				ulong cipher = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
				buckets[Mac.ComputeTag(cipher, rk) >> 29]++;
			}
			foreach (int count in buckets)
			{
				Assert.InRange(count, 150, 350);
			}
		}

		[Fact]
		public void Tag_SingleBitAvalanche()
		{
			// A 1-bit cipher flip must diffuse across the 32-bit tag (~16 bits
			// on average); the pinned range is ~80 standard errors wide.
			const int samples = 2000;
			ulong k3 = Vault.DeriveProcessTagKey(0x123456789ABCDEF0UL);
			var rng = new Random(777);
			long totalFlips = 0;
			for (int i = 0; i < samples; i++)
			{
				ulong cipher = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
				uint before = Mac.ComputeTag(cipher, k3);
				uint after = Mac.ComputeTag(cipher ^ (1UL << (i % 64)), k3);
				totalFlips += System.Numerics.BitOperations.PopCount(before ^ after);
			}
			double average = totalFlips / (double)samples;
			Assert.InRange(average, 11.0, 21.0); // ideal is 16
		}

		[Fact]
		public void Tag_DistinctSalts_DistinctTags()
		{
			// The tag key derives from the salt: distinct salts tag the same
			// cipher distinctly (32 samples: ~1e-7 accidental-tie chance).
			const ulong cipher = 0xDEADBEEFCAFEBABEUL;
			var tags = new HashSet<uint>();
			for (int i = 0; i < 32; i++)
			{
				ulong salt = ((ulong)(i + 1) * 0x9E3779B97F4A7C15UL) ^ 0x123456789ABCDEF0UL;
				tags.Add(Mac.ComputeTag(cipher, Vault.DeriveProcessTagKey(salt)));
			}
			Assert.Equal(32, tags.Count);
		}

		// ---------- 9. direct ComputeTag overloads ----------

		[Fact]
		public void ComputeTag_KeySetAndRawK3_Equivalent()
		{
			KeySet rk = Vault.DeriveProcessKeys(0x9E3779B97F4A7C15UL);
			ulong[] ciphers = new ulong[] { 0UL, 1UL, 0xFFFFFFFFFFFFFFFFUL, 0x123456789ABCDEF0UL };
			foreach (ulong cipher in ciphers)
			{
				Assert.Equal(Mac.ComputeTag(cipher, rk.K3), Mac.ComputeTag(cipher, rk));
			}
			foreach (ulong c0 in ciphers)
			{
				foreach (ulong c1 in ciphers)
				{
					Assert.Equal(Mac.ComputeTag(c0, c1, rk.K3), Mac.ComputeTag(c0, c1, rk));
				}
			}
		}

		[Fact]
		public void ComputeTag_SpanLengths_ZeroToFive()
		{
			KeySet rk = Vault.DeriveProcessKeys(0x0123456789ABCDEFUL);
			ulong[] words = new ulong[]
			{
				0x1111111111111111UL,
				0x2222222222222222UL,
				0x3333333333333333UL,
				0x4444444444444444UL,
				0x5555555555555555UL,
			};
			uint[] tags = new uint[6];
			for (int length = 0; length <= 5; length++)
			{
				tags[length] = Mac.ComputeTag(new ReadOnlySpan<ulong>(words, 0, length), rk);
				Assert.Equal(
					tags[length],
					Mac.ComputeTag(new ReadOnlySpan<ulong>(words, 0, length), rk)
				);
			}
			// The span tag is length-sensitive (empty included).
			Assert.Distinct(tags);

			// ... and order-sensitive, in both pair and span form.
			Assert.NotEqual(
				Mac.ComputeTag(words[0], words[1], rk),
				Mac.ComputeTag(words[1], words[0], rk)
			);
			ulong[] flipped = new ulong[] { words[1], words[0] };
			Assert.NotEqual(
				Mac.ComputeTag(words.AsSpan(0, 2), rk),
				Mac.ComputeTag(flipped.AsSpan(), rk)
			);
		}
	}
}
