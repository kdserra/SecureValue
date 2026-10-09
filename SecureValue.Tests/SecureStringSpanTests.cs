#nullable enable
using SecureValue;

namespace SecureValue.Tests
{
	/// <summary>
	/// SecureString span API: Length + CopyTo(Span&lt;char&gt;) decrypt words
	/// straight into the caller's buffer — no intermediate string, no
	/// collection allocations. Tamper/unset semantics mirror Decrypted.
	/// </summary>
	[Collection("TamperNotifier")]
	public class SecureStringSpanTests
	{
		public SecureStringSpanTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Fact]
		public void Length_MatchesPlainLength()
		{
			Assert.Equal(0, ((SecureString)"").Length);
			Assert.Equal(1, ((SecureString)"a").Length);
			Assert.Equal(4, ((SecureString)"abcd").Length);
			Assert.Equal(5, ((SecureString)"abcde").Length);
			Assert.Equal(100, ((SecureString)new string('x', 100)).Length);
			Assert.Equal(4, ((SecureString)"a🌍b").Length);
		}

		[Fact]
		public void Length_DefaultIsZero()
		{
			Assert.Equal(0, default(SecureString).Length);
		}

		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("abc")]
		[InlineData("abcd")]
		[InlineData("abcde")]
		[InlineData("abcdefg")]
		[InlineData("abcdefgh")]
		[InlineData("abcdefghijklmnop")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		[InlineData("héllo wörld🌍")]
		public void CopyTo_RoundTripsWithoutAllocatingString(string plain)
		{
			SecureString s = plain;
			Span<char> destination = new char[s.Length];
			s.CopyTo(destination);
			Assert.Equal(plain, destination.ToString());
		}

		[Fact]
		public void CopyTo_OversizedLeavesTailUntouched()
		{
			SecureString s = "hey";
			Span<char> destination = new char[8];
			destination.Fill('X');
			s.CopyTo(destination);
			Assert.Equal("hey", destination.Slice(0, 3).ToString());
			Assert.Equal("XXXXX", destination.Slice(3).ToString());
		}

		[Fact]
		public void CopyTo_TooSmallThrows()
		{
			SecureString s = "hello";
			Assert.Throws<ArgumentException>(() => s.CopyTo(Span<char>.Empty));
			Assert.Throws<ArgumentException>(() => s.CopyTo(new char[s.Length - 1]));
		}

		[Fact]
		public void CopyTo_DefaultWritesNothing()
		{
			Span<char> destination = stackalloc char[4];
			destination.Fill('X');
			default(SecureString).CopyTo(destination);
			Assert.Equal("XXXX", destination.ToString());
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void CopyTo_SinglyTamperedRestoresAndRaises(string plain)
		{
			SecureString s = plain;
			object box = s;
			CorruptFirstWord(box, backup: false, heap: plain.Length > 16);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Span<char> destination = new char[plain.Length];
			((SecureString)box).CopyTo(destination);
			Assert.Equal(plain, destination.ToString());
			Assert.True(fired > 0);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void CopyTo_DoublyTamperedThrows(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			Assert.Throws<TamperedException>(() =>
			{
				((SecureString)box).CopyTo(new char[plain.Length]);
			});
		}

		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("hunter2")]
		[InlineData("abcdefghijklmnop")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		[InlineData("héllo wörld🌍")]
		public void SequenceEqual_MatchesPlainComparison(string plain)
		{
			SecureString s = plain;
			Assert.True(s.SequenceEqual(plain.AsSpan()));
			Assert.True(s.SequenceEqual(plain));
			Assert.False(s.SequenceEqual((plain + "x").AsSpan()));
			Assert.False(s.SequenceEqual("something else entirely"));
		}

		[Fact]
		public void SequenceEqual_EmptyAgainstNonEmptyIsFalse()
		{
			SecureString s = string.Empty;
			Assert.True(s.SequenceEqual(ReadOnlySpan<char>.Empty));
			Assert.False(s.SequenceEqual("x".AsSpan()));
		}

		[Fact]
		public void SequenceEqual_DefaultMatchesEmpty()
		{
			Assert.False(default(SecureString).SequenceEqual("x".AsSpan()));
			Assert.True(default(SecureString).SequenceEqual("".AsSpan()));
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void SequenceEqual_SinglyTamperedRestoresAndRaises(string plain)
		{
			SecureString s = plain;
			object box = s;
			CorruptFirstWord(box, backup: false, heap: plain.Length > 16);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.True(((SecureString)box).SequenceEqual(plain.AsSpan()));
			Assert.True(fired > 0);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void SequenceEqual_DoublyTamperedThrows(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			Assert.Throws<TamperedException>(() =>
				((SecureString)box).SequenceEqual(plain.AsSpan())
			);
		}

		[Theory]
		[InlineData("")]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryCopyTo_RoundTripsWithoutThrowing(string plain)
		{
			SecureString s = plain;
			Span<char> destination = new char[s.Length];
			Assert.True(s.TryCopyTo(destination, out int written));
			Assert.Equal(plain.Length, written);
			Assert.Equal(plain, destination.ToString());
		}

		[Fact]
		public void TryCopyTo_TooSmallReturnsFalse()
		{
			SecureString s = "hello";
			Assert.False(s.TryCopyTo(new char[s.Length - 1], out int written));
			Assert.Equal(0, written);
			Assert.False(s.TryCopyTo(Span<char>.Empty, out written));
			Assert.Equal(0, written);
		}

		[Fact]
		public void TryCopyTo_DefaultReturnsFalse()
		{
			Assert.False(default(SecureString).TryCopyTo(stackalloc char[4], out int written));
			Assert.Equal(0, written);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryCopyTo_SinglyTamperedRecoversHealsAndRaises(string plain)
		{
			SecureString s = plain;
			object box = s;
			CorruptFirstWord(box, backup: false, heap: plain.Length > 16);
			var p = (SecureString)box;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Span<char> destination = new char[plain.Length];
			Assert.True(p.TryCopyTo(destination, out int written));
			Assert.Equal(plain.Length, written);
			Assert.Equal(plain, destination.ToString());
			Assert.Equal(1, fired);
			// Healed by the first call: the second is silent.
			Assert.True(p.TryCopyTo(destination, out written));
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryCopyTo_DoublyTamperedReturnsFalse(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Span<char> destination = new char[plain.Length];
			Assert.False(((SecureString)box).TryCopyTo(destination, out int written));
			Assert.Equal(0, written);
			Assert.True(fired > 0);
		}

		[Fact]
		public void CopyTo_AllocationFree()
		{
			SecureString s = "allocation probe string for span copies";
			Consume(s);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			Consume(s);
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(before, after);
		}

		private static void Consume(SecureString s)
		{
			Span<char> destination = stackalloc char[41];
			for (int i = 0; i < 1000; i++)
			{
				s.CopyTo(destination);
				if (destination[0] != 'a')
				{
					throw new InvalidOperationException("Corrupted span copy.");
				}
			}
		}

		private static void CorruptFirstWord(object box, bool backup, bool heap)
		{
			const System.Reflection.BindingFlags flags =
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
			if (heap)
			{
				CipherArray(box, backup ? "_ciphersB" : "_ciphers")[0] ^= 1UL;
				return;
			}
			string field = backup ? "_w0B" : "_w0";
			var f = box.GetType().GetField(field, flags)!;
			f.SetValue(box, (ulong)f.GetValue(box)! ^ 1UL);
		}

		private static ulong[] CipherArray(object box, string field)
		{
			var f = box.GetType()
				.GetField(
					field,
					System.Reflection.BindingFlags.NonPublic
						| System.Reflection.BindingFlags.Instance
				)!;
			return (ulong[])f.GetValue(box)!;
		}
	}
}
