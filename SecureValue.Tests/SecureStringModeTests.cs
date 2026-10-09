#nullable enable
using SecureValue;

namespace SecureValue.Tests
{
	/// <summary>
	/// SecureString dual-store contract (Cell256 inline at or under 16 chars,
	/// heap above): capacity routing, behavior parity, zero-alloc proof for
	/// small writes, tamper matrix (mode/length flips fail closed), and
	/// serialization incl. old-heap-short-save migration.
	/// </summary>
	public class SecureStringModeTests
	{
		private const System.Reflection.BindingFlags Flags =
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

		private static string ModeOf(object box) =>
			box.GetType().GetField("_mode", Flags)!.GetValue(box)!.ToString()!;

		private static void SetMode(object box, string mode)
		{
			System.Type modeType = box.GetType().GetNestedType("StorageMode", Flags)!;
			box.GetType()
				.GetField("_mode", Flags)!
				.SetValue(box, System.Enum.Parse(modeType, mode));
		}

		private static void SetLength(object box, int length) =>
			box.GetType().GetField("_length", Flags)!.SetValue(box, length);

		private static void CorruptInlineWord(object box, string field)
		{
			var f = box.GetType().GetField(field, Flags)!;
			f.SetValue(box, (ulong)f.GetValue(box)! ^ 1UL);
		}

		[Theory]
		[InlineData("", "Inline")]
		[InlineData("a", "Inline")]
		[InlineData("123456789012345", "Inline")]
		[InlineData("1234567890123456", "Inline")]
		[InlineData("12345678901234567", "Heap")]
		[InlineData("a much longer string that definitely lives on the heap for sure", "Heap")]
		public void Capacity_RoutesByLength(string plain, string expectedMode)
		{
			object box = (SecureString)plain;
			Assert.Equal(expectedMode, ModeOf(box));
			Assert.Equal(plain.Length, ((SecureString)box).Length);
		}

		[Fact]
		public void Capacity_DefaultIsUnassigned()
		{
			object box = default(SecureString);
			Assert.Equal("Unassigned", ModeOf(box));
			Assert.True(((SecureString)box).IsUnset);
		}

		[Theory]
		[InlineData("", true)]
		[InlineData("a", true)]
		[InlineData("1234567890123456", true)]
		[InlineData("12345678901234567", false)]
		[InlineData("a much longer string that definitely lives on the heap for sure", false)]
		public void IsInline_MatchesStore(string plain, bool expected)
		{
			SecureString s = plain;
			Assert.Equal(expected, s.IsInline);
		}

		[Fact]
		public void IsInline_DefaultIsFalseWithoutThrowing()
		{
			Assert.False(default(SecureString).IsInline);
		}

		[Theory]
		[InlineData("1234567890123456")]
		[InlineData("12345678901234567")]
		public void Parity_ModesBehaveIdentically(string plain)
		{
			SecureString a = plain;
			SecureString b = plain;
			Assert.Equal((string?)a, (string?)b);
			Assert.Equal(a.ToString(), b.ToString());
			Assert.True(a.Equals(b));
			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
			Span<char> destination = new char[plain.Length];
			a.CopyTo(destination);
			Assert.Equal(plain, destination.ToString());
		}

		[Fact]
		public void Alloc_SmallWritesAreZero()
		{
			ConsumeSmallWrites();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			ConsumeSmallWrites();
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(before, after);
		}

		[Fact]
		public void Alloc_LargeWritesAllocate()
		{
			ConsumeLargeWrite();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			ConsumeLargeWrite();
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.True(after > before);
		}

		private static void ConsumeSmallWrites()
		{
			for (int i = 0; i < 1000; i++)
			{
				SecureString a = "hunter2";
				if (a.Length != 7)
				{
					throw new InvalidOperationException("Corrupted small write.");
				}
				if (
					!SecureString.TryParse("hunter2".AsSpan(), null, out SecureString b)
					|| b.Length != 7
				)
				{
					throw new InvalidOperationException("Corrupted small span write.");
				}
			}
		}

		private static void ConsumeLargeWrite()
		{
			SecureString s = "a much longer string that definitely lives on the heap for sure";
			if (s.Length == 0)
			{
				throw new InvalidOperationException("Corrupted large write.");
			}
		}

		[Fact]
		public void Tamper_ModeFlipToHeap_ReadsDefault()
		{
			object box = (SecureString)"small";
			SetMode(box, "Heap");
			Assert.Null(((SecureString)box).Decrypted);
			Assert.Throws<TamperedException>(() => ((SecureString)box).Length);
		}

		[Fact]
		public void Tamper_ModeFlipToInlineLong_ReadsDefault()
		{
			object box = (SecureString)new string('x', 100);
			SetMode(box, "Inline");
			Assert.Null(((SecureString)box).Decrypted);
		}

		[Fact]
		public void Tamper_InlineLengthInRange_ThrowsTampered()
		{
			// Length is MAC-bound on the inline store: 5 -> 7 changes the tag input.
			object box = (SecureString)"hello";
			SetLength(box, 7);
			Assert.Throws<TamperedException>(() => ((SecureString)box).Decrypted);
		}

		[Fact]
		public void Tamper_InlineLengthOutOfRange_ReadsDefault()
		{
			object box = (SecureString)"hello";
			SetLength(box, 99);
			Assert.Null(((SecureString)box).Decrypted);
		}

		[Fact]
		public void Tamper_HeapNullArrays_ReadsDefault()
		{
			object box = (SecureString)new string('x', 100);
			box.GetType().GetField("_ciphers", Flags)!.SetValue(box, null);
			Assert.Null(((SecureString)box).Decrypted);
		}

		[Theory]
		[InlineData("short")]
		[InlineData("this string is long enough to live on the heap happily")]
		public void Tamper_DoubleWord_ThrowsBothModes(string plain)
		{
			object box = (SecureString)plain;
			bool heap = plain.Length > 16;
			if (heap)
			{
				HeapWords(box, "_ciphers")[0] ^= 1UL;
				HeapWords(box, "_ciphersB")[0] ^= 1UL;
			}
			else
			{
				CorruptInlineWord(box, "_w0");
				CorruptInlineWord(box, "_w0B");
			}
			Assert.Throws<TamperedException>(() => ((SecureString)box).Decrypted);
		}

		[Theory]
		[InlineData("short")]
		[InlineData("this string is long enough to live on the heap happily")]
		public void Serialization_RoundTripsPerMode(string plain)
		{
			object box = (SecureString)plain;
			string mode = ModeOf(box);
			uint[] packed = ((ISecureSerialization)box).SaveToSerialized();
			object fresh = default(SecureString);
			((ISecureSerialization)fresh).LoadFromSerialized(packed);
			Assert.Equal(plain, (string?)(SecureString)fresh);
			Assert.Equal(mode, ModeOf(fresh));
		}

		[Fact]
		public void Serialization_EmptyRoundTrips()
		{
			object box = (SecureString)"";
			uint[] packed = ((ISecureSerialization)box).SaveToSerialized();
			object fresh = default(SecureString);
			((ISecureSerialization)fresh).LoadFromSerialized(packed);
			Assert.Equal(string.Empty, (string?)(SecureString)fresh);
			Assert.Equal("Inline", ModeOf(fresh));
		}

		[Fact]
		public void Serialization_TamperedPack_Throws()
		{
			object box = (SecureString)"short";
			uint[] packed = ((ISecureSerialization)box).SaveToSerialized();
			int halfUints = (packed.Length - 1) / 2;
			packed[3] ^= 0xFFFFFFFFU;
			packed[3 + halfUints] ^= 0xFFFFFFFFU;
			object fresh = default(SecureString);
			Assert.Throws<TamperedException>(() =>
			{
				((ISecureSerialization)fresh).LoadFromSerialized(packed);
			});
		}

		[Fact]
		public void Serialization_OldHeapShortSaveMigratesToInline()
		{
			// Forge exactly what the pre-inline code emitted for a short string:
			// heap arrays with legacy (ciphers-only) tags, then load and confirm
			// the value is equal and the store normalized to inline.
			SecureString src = "hello";
			object box = src;
			ulong[] words = new ulong[2];
			words[0] = InlineWord(box, "_w0");
			words[1] = InlineWord(box, "_w1");
			ulong salt = (ulong)box.GetType().GetField("_salt", Flags)!.GetValue(box)!;
			ulong saltB = (ulong)box.GetType().GetField("_saltB", Flags)!.GetValue(box)!;
			SetMode(box, "Heap");
			box.GetType().GetField("_ciphers", Flags)!.SetValue(box, (ulong[])words.Clone());
			box.GetType()
				.GetField("_tag", Flags)!
				.SetValue(box, Vault.ComputeTag(words, Vault.DeriveProcessKeys(salt)));
			box.GetType().GetField("_ciphersB", Flags)!.SetValue(box, (ulong[])words.Clone());
			box.GetType()
				.GetField("_tagB", Flags)!
				.SetValue(box, Vault.ComputeTag(words, Vault.DeriveProcessKeys(saltB)));
			uint[] packed = ((ISecureSerialization)box).SaveToSerialized();
			object fresh = default(SecureString);
			((ISecureSerialization)fresh).LoadFromSerialized(packed);
			Assert.Equal("hello", (string?)(SecureString)fresh);
			Assert.Equal("Inline", ModeOf(fresh));
		}

		private static ulong InlineWord(object box, string field) =>
			(ulong)box.GetType().GetField(field, Flags)!.GetValue(box)!;

		private static ulong[] HeapWords(object box, string field) =>
			(ulong[])box.GetType().GetField(field, Flags)!.GetValue(box)!;
	}
}
