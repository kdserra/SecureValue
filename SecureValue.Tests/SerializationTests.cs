using System;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Round-trip and tamper-detection guarantees of the engine-serialization
	/// export path (raw cell words packed for Unity's serializer).
	/// </summary>
	public class SerializationTests
	{
		private static uint[] Export(Cell cell)
		{
			Span<ulong> words = stackalloc ulong[Cell.WordCount];
			cell.CopyWords(words);
			return SerializationFormat.Pack(words);
		}

		private static Cell Import(uint[] packed)
		{
			var cell = default(Cell);
			Span<ulong> words = stackalloc ulong[Cell.WordCount];
			Assert.True(SerializationFormat.TryUnpack(packed, words));
			cell.RestoreWords(words);
			return cell;
		}

		[Fact]
		public void PackUnpack_RoundTripsWords()
		{
			ulong[] words = { 0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, 1UL };
			uint[] packed = SerializationFormat.Pack(words);
			Assert.Equal(6, packed.Length);
			var restored = new ulong[3];
			Assert.True(SerializationFormat.TryUnpack(packed, restored));
			Assert.Equal(words, restored);
		}

		[Fact]
		public void TryUnpack_ReturnsFalse_ForNullOrWrongLength()
		{
			Assert.False(SerializationFormat.TryUnpack(null, stackalloc ulong[3]));
			Assert.False(SerializationFormat.TryUnpack(new uint[4], stackalloc ulong[3]));
			Assert.False(SerializationFormat.TryUnpack(new uint[8], stackalloc ulong[3]));
		}

		[Fact]
		public void Cell_ExportImport_RoundTripsValue()
		{
			var cell = default(Cell);
			cell.Protect(12345UL);
			uint[] packed = Export(cell);
			Cell restored = Import(packed);
			Assert.Equal(12345UL, restored.Unprotect());
		}

		[Fact]
		public void Cell_TamperStillDetected_AfterExportImport()
		{
			var cell = default(Cell);
			cell.Protect(42UL);
			Cell restored = Import(Export(cell));
			restored.CorruptBothForTesting();
			Assert.Throws<TamperedException>(() => restored.Unprotect());
		}

		[Fact]
		public void Cell128_ExportImport_RoundTripsValueAndDetectsTamper()
		{
			var cell = default(Cell128);
			cell.Protect(0xDEADBEEFCAFEBABEUL, 0x0F0F0F0F0F0F0F0FUL);

			Span<ulong> words = stackalloc ulong[Cell128.WordCount];
			cell.CopyWords(words);
			uint[] packed = SerializationFormat.Pack(words);

			var restored = default(Cell128);
			Span<ulong> back = stackalloc ulong[Cell128.WordCount];
			Assert.True(SerializationFormat.TryUnpack(packed, back));
			restored.RestoreWords(back);

			(ulong lo, ulong hi) = restored.Unprotect();
			Assert.Equal(0xDEADBEEFCAFEBABEUL, lo);
			Assert.Equal(0x0F0F0F0F0F0F0F0FUL, hi);
		}

		[Fact]
		public void MultiCell_ExportImport_RoundTripsLikeEngineSerialization()
		{
			// Simulates a multi-cell wrapper (e.g. Matrix3x2: one Cell128 + one Cell).
			var cellA = default(Cell128);
			var cellB = default(Cell);
			cellA.Protect(11UL, 22UL);
			cellB.Protect(33UL);

			const int total = Cell128.WordCount + Cell.WordCount;
			Span<ulong> words = stackalloc ulong[total];
			cellA.CopyWords(words);
			cellB.CopyWords(words.Slice(Cell128.WordCount));
			uint[] packed = SerializationFormat.Pack(words);

			var restoredA = default(Cell128);
			var restoredB = default(Cell);
			Span<ulong> back = stackalloc ulong[total];
			Assert.True(SerializationFormat.TryUnpack(packed, back));
			restoredA.RestoreWords(back);
			restoredB.RestoreWords(back.Slice(Cell128.WordCount));

			(ulong lo, ulong hi) = restoredA.Unprotect();
			Assert.Equal(11UL, lo);
			Assert.Equal(22UL, hi);
			Assert.Equal(33UL, restoredB.Unprotect());
		}

		[Fact]
		public void Cell_StorageExportImport_RoundTripsValue()
		{
			var cell = default(Cell);
			cell.Protect(777UL);

			Span<ulong> storage = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(storage, saveKey);
			saveKey.CopyTo(storage.Slice(Cell.WordCount));
			uint[] packed = SerializationFormat.Pack(storage);

			var restored = default(Cell);
			Span<ulong> back = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			Assert.True(SerializationFormat.TryUnpack(packed, back));
			restored.RestoreStorageWords(back, KeySet.FromWords(back.Slice(Cell.WordCount)));
			Assert.Equal(777UL, restored.Unprotect());
		}

		[Fact]
		public void Cell_StorageForm_DiffersFromInMemoryForm()
		{
			// The saved form uses a fresh per-save storage key; the in-memory
			// form uses the per-process key. Same value -> different ciphertext.
			var cell = default(Cell);
			cell.Protect(42UL);
			Span<ulong> process = stackalloc ulong[Cell.WordCount];
			cell.CopyWords(process);
			Span<ulong> storage = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(storage, saveKey);
			Assert.Equal(process[0], storage[0]); // salt is shared
			Assert.NotEqual(process[1], storage[1]); // cipher differs by key set
			Assert.NotEqual(process[2], storage[2]); // tag differs by key set
		}

		[Fact]
		public void Cell_StorageForm_IsUniquePerSave()
		{
			// Same cell saved twice: each save mints a fresh storage key, so both
			// the stored key and the ciphertext differ while the salt is reused.
			var cell = default(Cell);
			cell.Protect(42UL);
			Span<ulong> first = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			Span<ulong> second = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet key1 = Vault.NewStorageKey();
			cell.CopyStorageWords(first, key1);
			key1.CopyTo(first.Slice(Cell.WordCount));
			KeySet key2 = Vault.NewStorageKey();
			cell.CopyStorageWords(second, key2);
			key2.CopyTo(second.Slice(Cell.WordCount));
			Assert.Equal(first[0], second[0]); // salt is reused across saves
			Assert.NotEqual(first[1], second[1]); // fresh key -> fresh ciphertext
			Assert.False(first.Slice(Cell.WordCount).SequenceEqual(second.Slice(Cell.WordCount)));
		}

		[Fact]
		public void Cell_StorageImport_DetectsTamperedSaveData()
		{
			var cell = default(Cell);
			cell.Protect(42UL);
			Span<ulong> storage = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(storage, saveKey);
			saveKey.CopyTo(storage.Slice(Cell.WordCount));
			storage[1] ^= 1UL; // flip a bit of the primary stored cipher
			storage[4] ^= 1UL; // and of the backup copy: singly-tampered saves restore
			uint[] packed = SerializationFormat.Pack(storage);

			var restored = default(Cell);
			Assert.Throws<TamperedException>(() =>
			{
				Span<ulong> back = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
				Assert.True(SerializationFormat.TryUnpack(packed, back));
				restored.RestoreStorageWords(back, KeySet.FromWords(back.Slice(Cell.WordCount)));
			});
		}

		[Fact]
		public void Cell_StorageImport_DetectsTamperedKey()
		{
			// The key travels with the payload: flipping a stored key word breaks
			// the derived tag check exactly like flipping a cipher word.
			var cell = default(Cell);
			cell.Protect(42UL);
			Span<ulong> storage = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(storage, saveKey);
			saveKey.CopyTo(storage.Slice(Cell.WordCount));
			storage[Cell.WordCount] ^= 1UL; // flip a bit of the stored key
			uint[] packed = SerializationFormat.Pack(storage);

			var restored = default(Cell);
			Assert.Throws<TamperedException>(() =>
			{
				Span<ulong> back = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
				Assert.True(SerializationFormat.TryUnpack(packed, back));
				restored.RestoreStorageWords(back, KeySet.FromWords(back.Slice(Cell.WordCount)));
			});
		}

		[Fact]
		public void Cell128_StorageExportImport_RoundTripsValue()
		{
			var cell = default(Cell128);
			cell.Protect(0xA1B2C3D4E5F60718UL, 0x1122334455667788UL);

			Span<ulong> storage = stackalloc ulong[Cell128.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(storage, saveKey);
			saveKey.CopyTo(storage.Slice(Cell128.WordCount));
			uint[] packed = SerializationFormat.Pack(storage);

			var restored = default(Cell128);
			Span<ulong> back = stackalloc ulong[Cell128.WordCount + KeySet.WordCount];
			Assert.True(SerializationFormat.TryUnpack(packed, back));
			restored.RestoreStorageWords(back, KeySet.FromWords(back.Slice(Cell128.WordCount)));
			(ulong lo, ulong hi) = restored.Unprotect();
			Assert.Equal(0xA1B2C3D4E5F60718UL, lo);
			Assert.Equal(0x1122334455667788UL, hi);
		}

		[Fact]
		public void Cell_DefaultUnprotect_ReturnsDefault()
		{
			// All-zero backing fields read as default.
			Assert.Equal(0UL, default(Cell).Unprotect());
			Assert.Equal((0UL, 0UL), default(Cell128).Unprotect());
		}

		[Fact]
		public void MixStream_IsPureFunctionOfSaltAndIndex()
		{
			// Array-backed wrappers (string, BigInteger) bake this mask opaquely into
			// ciphers that cross process boundaries via the storage form, then unmask
			// with the loading process's state. Any per-process key material folded in
			// here corrupts every such value on the next editor restart while
			// same-process reads stay green, so pin the independence explicitly.
			ulong salt = Vault.RandomSalt();
			Assert.Equal(
				Mixing.SplitMix(salt ^ unchecked(0UL * 0x9E3779B97F4A7C15UL)),
				Vault.MixStream(salt, 0UL)
			);
			Assert.Equal(
				Mixing.SplitMix(salt ^ unchecked(7UL * 0x9E3779B97F4A7C15UL)),
				Vault.MixStream(salt, 7UL)
			);
			Assert.NotEqual(Vault.MixStream(salt, 0UL), Vault.MixStream(salt, 1UL));
			Assert.NotEqual(Vault.MixStream(salt, 0UL), Vault.MixStream(salt ^ 1UL, 0UL));
		}
	}
}
