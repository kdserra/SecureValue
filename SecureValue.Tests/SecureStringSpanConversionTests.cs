#nullable enable
using SecureValue;

namespace SecureValue.Tests
{
	/// <summary>
	/// Span-to-SecureString implicit conversions: both <c>Span&lt;char&gt;</c> and
	/// <c>ReadOnlySpan&lt;char&gt;</c> sources must bind (operator lookup does not chain
	/// the Span-to-ReadOnlySpan step, hence the twin overloads), while string and
	/// null sources must keep binding the pre-existing string conversion.
	/// </summary>
	public class SecureStringSpanConversionTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("crown_01")]
		[InlineData("abcdefghijklmnop")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		[InlineData("héllo wörld🌍")]
		public void ReadOnlySpanAssignment_RoundTrips(string plain)
		{
			ReadOnlySpan<char> span = plain.AsSpan();
			SecureString s = span;
			Assert.Equal(plain.Length, s.Length);
			Assert.Equal(plain, s.Decrypted);
			Assert.True(s.SequenceEqual(plain.AsSpan()));
		}

		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("crown_01")]
		[InlineData("abcdefghijklmnop")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		public void SpanAssignment_RoundTrips(string plain)
		{
			Span<char> span = plain.ToCharArray().AsSpan();
			SecureString s = span;
			Assert.Equal(plain.Length, s.Length);
			Assert.Equal(plain, s.Decrypted);
		}

		[Fact]
		public void InlineStackallocAssignment_RoundTrips()
		{
			Span<char> buffer = stackalloc char[16];
			"wpn_excalibur_01".CopyTo(buffer);
			SecureString s = buffer;
			Assert.Equal("wpn_excalibur_01", s.Decrypted);
		}

		[Fact]
		public void EmptySpanAssignment_IsEmpty()
		{
			SecureString s = ReadOnlySpan<char>.Empty;
			Assert.Equal(0, s.Length);
			Assert.Equal(string.Empty, s.Decrypted);
		}

		[Fact]
		public void StringSources_StillBindStringConversion()
		{
			SecureString fromLiteral = "crown_01";
			Assert.Equal("crown_01", fromLiteral.Decrypted);

			string value = "crown_01";
			SecureString fromVar = value;
			Assert.Equal("crown_01", fromVar.Decrypted);

			SecureString fromNull = (string)null!;
			Assert.Equal(string.Empty, fromNull.Decrypted);
		}

		[Fact]
		public void SpanAssignment_InlineLengthIsAllocationFree()
		{
			ReadOnlySpan<char> span = "allocation probe".AsSpan();
			Consume(span);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			Consume(span);
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(before, after);
		}

		private static void Consume(ReadOnlySpan<char> span)
		{
			for (int i = 0; i < 1000; i++)
			{
				SecureString s = span;
				if (s.Length != span.Length)
				{
					throw new InvalidOperationException("Corrupted span conversion.");
				}
			}
		}
	}
}
