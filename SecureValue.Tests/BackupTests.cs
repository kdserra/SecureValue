#nullable enable
using System;
using System.Numerics;
using System.Reflection;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Backup-restore semantics: a singly-tampered copy heals from the good one
	/// (raising exactly once, never throwing); only doubly-tampered reads fail.
	/// Shares the isolated TamperNotifier collection so exact event counts hold.
	/// </summary>
	[Collection("TamperNotifier")]
	public class BackupTests
	{
		public BackupTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Fact]
		public void SinglePrimaryCorrupt_RestoresValueWithoutThrowing()
		{
			object boxed = new SecureInt(42);
			CorruptCipher(boxed, "_cell", "_cipher");
			var p = (SecureInt)boxed;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Equal(42, p.Decrypted);
			Assert.Equal(1, fired);
			// The first direct read healed the damaged copy: the next read is
			// silent. (Reads through implicit conversions heal a discarded copy
			// instead, so they keep reporting; healing persists on direct reads.)
			Assert.Equal(42, p.Decrypted);
			Assert.Equal(1, fired);
			// A plain reassignment still heals both copies with fresh salts.
			p = 42;
			Assert.Equal(42, p.Decrypted);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void SingleBackupCorrupt_RestoresValueWithoutThrowing()
		{
			object boxed = new SecureInt(42);
			CorruptCipher(boxed, "_cell", "_cipherB");
			var p = (SecureInt)boxed;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Equal(42, (int)p);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void BothCorrupt_ThrowsOnce()
		{
			var cell = default(Cell);
			cell.Protect(7UL);
			cell.CorruptBothForTesting();

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Throws<TamperedException>(() => cell.Unprotect());
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Copies_HaveIndependentCiphertext()
		{
			// The copies must never share encryption material: different salts and
			// different ciphertext for the same plaintext (no reusable pattern).
			var cell = default(Cell);
			cell.Protect(100UL);
			(ulong salt, ulong cipher) = (GetWord(cell, "_salt"), GetWord(cell, "_cipher"));
			(ulong saltB, ulong cipherB) = (GetWord(cell, "_saltB"), GetWord(cell, "_cipherB"));
			Assert.NotEqual(salt, saltB);
			Assert.NotEqual(cipher, cipherB);
		}

		[Fact]
		public void HealOnRead_SecondReadSilent_KillingOtherCopyStillRecovers()
		{
			object boxed = new SecureInt(99);
			CorruptCipher(boxed, "_cell", "_cipher");
			var p = (SecureInt)boxed;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(99, p.Decrypted); // heals the primary copy
			Assert.Equal(1, fired);
			Assert.Equal(99, p.Decrypted); // healed: silent
			Assert.Equal(1, fired);

			// Killing the surviving copy afterwards still recovers from the
			// healed one.
			object reboxed = p;
			CorruptCipher(reboxed, "_cell", "_cipherB");
			var q = (SecureInt)reboxed;
			Assert.Equal(99, q.Decrypted);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void HealOnRead_KillingBothCopiesAfterHeal_StillFailsClosed()
		{
			// Recovery lasts exactly as long as one copy verifies: the first
			// read heals, so both copies must be killed post-heal to fail closed.
			object boxed = new SecureInt(99);
			CorruptCipher(boxed, "_cell", "_cipher");
			var damaged = (SecureInt)boxed;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(99, damaged.Decrypted); // heals the primary copy
			Assert.Equal(1, fired);

			object reboxed = damaged;
			CorruptCipher(reboxed, "_cell", "_cipher");
			CorruptCipher(reboxed, "_cell", "_cipherB");
			var twice = (SecureInt)reboxed;

			Assert.Throws<TamperedException>(() => _ = twice.Decrypted);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell128_SingleCorrupt_RestoresPair()
		{
			Guid expected = Guid.NewGuid();
			object boxed = new SecureGuid(expected);
			CorruptCipher(boxed, "_cell", "_cipherLo");
			var p = (SecureGuid)boxed;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(expected, p.Decrypted);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Cell128_SingleCorrupt_HealsOnRead()
		{
			Guid expected = Guid.NewGuid();
			object boxed = new SecureGuid(expected);
			CorruptCipher(boxed, "_cell", "_cipherLo");
			var p = (SecureGuid)boxed;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(expected, p.Decrypted); // heals the damaged copy
			Assert.Equal(1, fired);
			Assert.Equal(expected, p.Decrypted); // healed: silent
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData("backup me")]
		[InlineData("backup me, this string is long enough for the heap")]
		public void String_SingleArrayCorrupt_Recovers(string plain)
		{
			SecureString p = CorruptStringWord(new SecureString(plain), backup: false);

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(plain, (string)p);
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData("backup me")]
		[InlineData("backup me, this string is long enough for the heap")]
		public void String_SingleCorrupt_HealsOnRead(string plain)
		{
			// Direct reads persist the heal, so the second read is silent.
			SecureString p = CorruptStringWord(new SecureString(plain), backup: false);

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(plain, p.Decrypted); // heals the damaged copy
			Assert.Equal(1, fired);
			Assert.Equal(plain, p.Decrypted); // healed: silent
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData("backup me")]
		[InlineData("backup me, this string is long enough for the heap")]
		public void String_BothArraysCorrupt_ThrowsOnce(string plain)
		{
			SecureString p = CorruptStringWord(new SecureString(plain), backup: false);
			p = CorruptStringWord(p, backup: true);

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Throws<TamperedException>(() => _ = (string)p);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void BigInteger_SingleArrayCorrupt_Recovers()
		{
			BigInteger expected = BigInteger.Pow(2, 128) + 7;
			SecureBigInteger p = CorruptBigIntegerArray(new SecureBigInteger(expected), "_ciphers");

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(expected, (BigInteger)p);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void BigInteger_SingleArrayCorrupt_HealsOnRead()
		{
			BigInteger expected = BigInteger.Pow(2, 128) + 7;
			SecureBigInteger p = CorruptBigIntegerArray(new SecureBigInteger(expected), "_ciphers");

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.Equal(expected, p.Decrypted); // heals the damaged copy
			Assert.Equal(1, fired);
			Assert.Equal(expected, p.Decrypted); // healed: silent
			Assert.Equal(1, fired);
		}

		[Fact]
		public void TryDecrypt_SingleCorrupt_ReturnsTrueWithEvent()
		{
			// Approved spec change: a recoverable read succeeds; false now means
			// both copies failed (or never assigned).
			var cell = default(Cell);
			cell.Protect(5UL);
			cell.CorruptForTesting();

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.True(cell.TryUnprotect(out ulong plain));
			Assert.Equal(5UL, plain);
			Assert.Equal(1, fired);
			// Healed by the first try-read: the second is silent.
			Assert.True(cell.TryUnprotect(out ulong plain2));
			Assert.Equal(5UL, plain2);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void TryDecrypt_BothCorrupt_ReturnsFalseWithOneEvent()
		{
			var cell = default(Cell);
			cell.Protect(5UL);
			cell.CorruptBothForTesting();

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(cell.TryUnprotect(out ulong plain));
			Assert.Equal(0UL, plain);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void MultiCell_AllCellsBothBad_RaisesOnceWithCapOne()
		{
			// The value still checks every cell and fails closed; the global notifier
			// dispatches only once, regardless of how many cells are damaged.
			SecureMatrix4x4 p = CorruptAllCellsBothCopies(new SecureMatrix4x4(Matrix4x4.Identity));

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out Matrix4x4 value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Storage_SingleCopyTampered_RestoresOnLoad()
		{
			var cell = default(Cell);
			cell.Protect(4242UL);
			KeySet saveKey = Vault.NewStorageKey();
			Span<ulong> storage = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			cell.CopyStorageWords(storage, saveKey);
			saveKey.CopyTo(storage.Slice(Cell.WordCount));
			storage[1] ^= 1UL; // tamper the primary stored cipher only

			uint[] packed = SerializationFormat.Pack(storage);
			var restored = default(Cell);
			Span<ulong> back = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			Assert.True(SerializationFormat.TryUnpack(packed, back));

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			restored.RestoreStorageWords(back, KeySet.FromWords(back.Slice(Cell.WordCount)));
			Assert.Equal(4242UL, restored.Unprotect());
			Assert.Equal(1, fired);
		}

		// ---------- helpers ----------

		private static ulong GetWord(Cell cell, string name)
		{
			return (ulong)
				typeof(Cell)
					.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
					.GetValue(cell)!;
		}

		private static void CorruptCipher(object boxedWrapper, string cellField, string cipherField)
		{
			Type t = boxedWrapper.GetType();
			FieldInfo cellF = t.GetField(
				cellField,
				BindingFlags.NonPublic | BindingFlags.Instance
			)!;
			object cell = cellF.GetValue(boxedWrapper)!;
			FieldInfo cipher = cell.GetType()
				.GetField(cipherField, BindingFlags.NonPublic | BindingFlags.Instance)!;
			cipher.SetValue(cell, (ulong)cipher.GetValue(cell)! ^ 1UL);
			cellF.SetValue(boxedWrapper, cell);
		}

		internal static T CorruptOneCellBothCopies<T>(T wrapper, string cellField)
			where T : struct
		{
			// Fully damage exactly one cell (both copies): the wrapper fails closed
			// with a single event while sibling cells stay silent.
			object boxed = wrapper;
			Type t = boxed.GetType();
			FieldInfo cellF = t.GetField(
				cellField,
				BindingFlags.NonPublic | BindingFlags.Instance
			)!;
			object cell = cellF.GetValue(boxed)!;
			Type cellType = cell.GetType();
			string[] ciphers =
				cellType == typeof(Cell)
					? new[] { "_cipher", "_cipherB" }
					: new[] { "_cipherLo", "_cipherLoB" };
			foreach (string name in ciphers)
			{
				FieldInfo cipher = cellType.GetField(
					name,
					BindingFlags.NonPublic | BindingFlags.Instance
				)!;
				cipher.SetValue(cell, (ulong)cipher.GetValue(cell)! ^ 1UL);
			}
			cellF.SetValue(boxed, cell);
			return (T)boxed;
		}

		private static SecureString CorruptStringWord(SecureString value, bool backup)
		{
			// First word of the active store: heap array, else the inline words.
			object boxed = value;
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var arr = (ulong[]?)
				typeof(SecureString)
					.GetField(backup ? "_ciphersB" : "_ciphers", flags)!
					.GetValue(boxed);
			if (arr is { Length: > 0 })
			{
				arr[0] ^= 1UL;
				return (SecureString)boxed;
			}
			string word = backup ? "_w0B" : "_w0";
			var f = typeof(SecureString).GetField(word, flags)!;
			f.SetValue(boxed, (ulong)f.GetValue(boxed)! ^ 1UL);
			return (SecureString)boxed;
		}

		private static SecureBigInteger CorruptBigIntegerArray(SecureBigInteger value, string field)
		{
			object boxed = value;
			var f = typeof(SecureBigInteger).GetField(
				field,
				BindingFlags.NonPublic | BindingFlags.Instance
			)!;
			((ulong[])f.GetValue(boxed)!)[0] ^= 1UL;
			return (SecureBigInteger)boxed;
		}

		private static SecureMatrix4x4 CorruptAllCellsBothCopies(SecureMatrix4x4 value)
		{
			object boxed = value;
			Type t = boxed.GetType();
			foreach (FieldInfo cellF in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (cellF.FieldType != typeof(Cell128))
				{
					continue;
				}
				object cell = cellF.GetValue(boxed)!;
				Type cellType = cell.GetType();
				foreach (string name in new[] { "_cipherLo", "_cipherLoB" })
				{
					FieldInfo cipher = cellType.GetField(
						name,
						BindingFlags.NonPublic | BindingFlags.Instance
					)!;
					cipher.SetValue(cell, (ulong)cipher.GetValue(cell)! ^ 1UL);
				}
				cellF.SetValue(boxed, cell);
			}
			return (SecureMatrix4x4)boxed;
		}
	}
}
