#nullable enable
using System.Globalization;
using System.Numerics;
using System.Text;
using SecureValue;
using SecureValue.Numerics;

namespace SecureValue.Tests
{
	/// <summary>
	/// Parse/format interface contract (SecureValue.CodeGen Parsable emitter):
	/// IParsable/ISpanParsable (NET7+), IUtf8SpanFormattable/IUtf8SpanParsable
	/// (NET8+), IEnumerable&lt;char&gt; on SecureString (all TFMs). Static
	/// interface dispatch goes through constrained type parameters (CS8926
	/// forbids IParsable&lt;S&gt;.Parse direct calls) — the helpers below are
	/// the consumer pattern these tests pin down.
	/// </summary>
	public class ParsableTests
	{
		private static T ParseIt<T>(string? s, IFormatProvider? provider)
			where T : IParsable<T> => T.Parse(s!, provider);

		private static bool TryParseIt<T>(string? s, IFormatProvider? provider, out T result)
			where T : IParsable<T>
		{
			bool ok = T.TryParse(s, provider, out T? tmp);
			result = tmp!;
			return ok;
		}

		private static T SpanParseIt<T>(ReadOnlySpan<char> s, IFormatProvider? provider)
			where T : ISpanParsable<T> => T.Parse(s, provider);

		private static bool SpanTryParseIt<T>(
			ReadOnlySpan<char> s,
			IFormatProvider? provider,
			out T result
		)
			where T : ISpanParsable<T>
		{
			bool ok = T.TryParse(s, provider, out T? tmp);
			result = tmp!;
			return ok;
		}

		private static T Utf8ParseIt<T>(ReadOnlySpan<byte> s, IFormatProvider? provider)
			where T : IUtf8SpanParsable<T> => T.Parse(s, provider);

		private static bool Utf8TryParseIt<T>(
			ReadOnlySpan<byte> s,
			IFormatProvider? provider,
			out T result
		)
			where T : IUtf8SpanParsable<T>
		{
			bool ok = T.TryParse(s, provider, out T? tmp);
			result = tmp!;
			return ok;
		}

		[Fact]
		public void Parsable_RoundTripsNumerics()
		{
			Assert.Equal(-12345, (int)ParseIt<SecureInt>("-12345", null));
			Assert.Equal(9876543210L, (long)ParseIt<SecureLong>("9876543210", null));
			Assert.Equal(2.5, (double)ParseIt<SecureDouble>("2.5", CultureInfo.InvariantCulture));
			Assert.Equal(
				123.45m,
				(decimal)ParseIt<SecureDecimal>("123.45", CultureInfo.InvariantCulture)
			);
			Assert.True(TryParseIt<SecureInt>("42", null, out SecureInt n));
			Assert.Equal(42, (int)n);
			Assert.False(TryParseIt<SecureInt>("nope", null, out _));
			Assert.Throws<FormatException>(() => ParseIt<SecureInt>("nope", null));
			SecureBigInteger big = ParseIt<SecureBigInteger>(
				"123456789012345678901234567890",
				null
			);
			Assert.Equal(BigInteger.Parse("123456789012345678901234567890"), (BigInteger)big);
			Complex expectedComplex = new(3, 4);
			string complexText = expectedComplex.ToString("G", CultureInfo.InvariantCulture);
			SecureComplex c = ParseIt<SecureComplex>(complexText, CultureInfo.InvariantCulture);
			Assert.Equal(expectedComplex, (Complex)c);
		}

		[Fact]
		public void Parsable_RoundTripsCharBoolDatesGuidString()
		{
			Assert.Equal('Z', (char)ParseIt<SecureChar>("Z", null));
			Assert.True((bool)ParseIt<SecureBool>("True", null));
			Assert.False(TryParseIt<SecureBool>("maybe", null, out _));
			DateTime dt = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
			Assert.Equal(
				dt,
				(DateTime)
					ParseIt<SecureDateTime>("01/02/2026 03:04:05", CultureInfo.InvariantCulture)
			);
			Guid g = Guid.NewGuid();
			Assert.Equal(g, (Guid)ParseIt<SecureGuid>(g.ToString("D"), null));
			Assert.False(TryParseIt<SecureGuid>("not-a-guid", null, out _));
			Assert.Equal("hello", (string?)ParseIt<SecureString>("hello", null));
			Assert.Equal(string.Empty, (string?)ParseIt<SecureString>(null, null));
			Assert.True(TryParseIt<SecureString>(null, null, out SecureString empty));
			Assert.Equal(string.Empty, (string?)empty);
		}

		[Fact]
		public void SpanParsable_SpanRoundTrips()
		{
			Assert.Equal(-12345, (int)SpanParseIt<SecureInt>("-12345".AsSpan(), null));
			Assert.True(
				SpanTryParseIt<SecureDouble>(
					"2.5".AsSpan(),
					CultureInfo.InvariantCulture,
					out SecureDouble d
				)
			);
			Assert.Equal(2.5, (double)d);
			Assert.False(SpanTryParseIt<SecureInt>("nope".AsSpan(), null, out _));
			Guid g = Guid.NewGuid();
			Assert.Equal(g, (Guid)SpanParseIt<SecureGuid>(g.ToString("D").AsSpan(), null));
			Assert.Equal('Q', (char)SpanParseIt<SecureChar>("Q".AsSpan(), null));
		}

		[Fact]
		public void Utf8Parsable_Utf8RoundTrips()
		{
			Assert.Equal(12345, (int)Utf8ParseIt<SecureInt>(Encoding.UTF8.GetBytes("12345"), null));
			Assert.True(
				Utf8TryParseIt<SecureDouble>(
					Encoding.UTF8.GetBytes("2.5"),
					CultureInfo.InvariantCulture,
					out SecureDouble d
				)
			);
			Assert.Equal(2.5, (double)d);
			Assert.False(Utf8TryParseIt<SecureInt>(Encoding.UTF8.GetBytes("nope"), null, out _));
			Guid g = Guid.NewGuid();
			Assert.Equal(
				g,
				(Guid)Utf8ParseIt<SecureGuid>(Encoding.UTF8.GetBytes(g.ToString("D")), null)
			);
			SecureRune rune = Utf8ParseIt<SecureRune>(Encoding.UTF8.GetBytes("é"), null);
			Assert.Equal(new Rune('é'), (Rune)rune);
			Assert.False(Utf8TryParseIt<SecureRune>(Encoding.UTF8.GetBytes("ab"), null, out _));
			Assert.False(Utf8TryParseIt<SecureRune>([], null, out _));
			Assert.Throws<FormatException>(() =>
				Utf8ParseIt<SecureRune>(Encoding.UTF8.GetBytes("ab"), null)
			);
		}

		[Fact]
		public void SpanParsable_DirectKinds()
		{
			Assert.Equal(9876543210L, (long)SpanParseIt<SecureLong>("9876543210".AsSpan(), null));
			Assert.Equal(
				123.45m,
				(decimal)SpanParseIt<SecureDecimal>("123.45".AsSpan(), CultureInfo.InvariantCulture)
			);
			Assert.Equal(
				new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Unspecified),
				(DateTime)
					SpanParseIt<SecureDateTime>("01/02/2026".AsSpan(), CultureInfo.InvariantCulture)
			);
			Assert.Equal(
				TimeSpan.FromMinutes(90),
				(TimeSpan)
					SpanParseIt<SecureTimeSpan>("01:30:00".AsSpan(), CultureInfo.InvariantCulture)
			);
			Assert.Equal(
				BigInteger.Parse("12345678901234567890"),
				(BigInteger)SpanParseIt<SecureBigInteger>("12345678901234567890".AsSpan(), null)
			);
			Complex expected = new(1.5, -2.5);
			string text = expected.ToString("G", CultureInfo.InvariantCulture);
			Assert.Equal(
				expected,
				(Complex)SpanParseIt<SecureComplex>(text.AsSpan(), CultureInfo.InvariantCulture)
			);
			Assert.False(SpanTryParseIt<SecureLong>("nope".AsSpan(), null, out _));
			Assert.Equal(
				BigInteger.Parse("99"),
				(BigInteger)Utf8ParseIt<SecureBigInteger>(Encoding.UTF8.GetBytes("99"), null)
			);
		}

		[Fact]
		public void SpanFormattable_CharRuneDateTimeOffset()
		{
			SecureChar c = 'Q';
			Span<char> destination = stackalloc char[8];
			Assert.True(
				((ISpanFormattable)c).TryFormat(destination, out int written, default, null)
			);
			Assert.Equal("Q", destination.Slice(0, written).ToString());
			SecureRune rune = new(new Rune('é'));
			Assert.True(
				((ISpanFormattable)rune).TryFormat(destination, out written, default, null)
			);
			Assert.Equal("é", destination.Slice(0, written).ToString());
			SecureDateTimeOffset dto = new(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
			Span<char> wide = stackalloc char[64];
			Assert.True(((ISpanFormattable)dto).TryFormat(wide, out written, default, null));
			Assert.True(written > 0);
		}

		[Fact]
		public void PublicParse_StringAndSpan()
		{
			Assert.Equal("hello", (string?)SecureString.Parse("hello", null));
			Assert.Equal(string.Empty, (string?)SecureString.Parse(null, null));
			Assert.True(SecureString.TryParse("hey", null, out SecureString s));
			Assert.Equal("hey", (string?)s);
			Assert.Equal("span", (string?)SecureString.Parse("span".AsSpan(), null));
			Assert.True(SecureString.TryParse("span".AsSpan(), null, out SecureString parsed));
			Assert.Equal("span", (string?)parsed);
		}

		[Fact]
		public void SpanParsable_StringSealsDirectly()
		{
			string plain = "span-native sealing, no intermediate string object";
			SecureString viaSpan = SpanParseIt<SecureString>(plain.AsSpan(), null);
			Assert.Equal(plain, (string?)viaSpan);
			Assert.True(
				SpanTryParseIt<SecureString>(plain.AsSpan(), null, out SecureString parsed)
			);
			Assert.Equal(plain, (string?)parsed);
		}

		[Fact]
		public void SpanFormattable_ComplexAndBigInteger()
		{
			SecureBigInteger big = new(BigInteger.Parse("12345678901234567890"));
			Span<char> destination = stackalloc char[32];
			Assert.True(
				((ISpanFormattable)big).TryFormat(destination, out int written, default, null)
			);
			Assert.Equal("12345678901234567890", destination.Slice(0, written).ToString());
			SecureComplex c = new(new Complex(1.5, -2.5));
			Assert.True(((ISpanFormattable)c).TryFormat(destination, out written, default, null));
			Assert.Equal(
				((ISpanFormattable)new Complex(1.5, -2.5)).ToString(),
				destination.Slice(0, written).ToString()
			);
		}

		[Fact]
		public void Parsable_RoundTripsRemainingDates()
		{
			Assert.Equal(
				new DateOnly(2026, 1, 2),
				(DateOnly)ParseIt<SecureDateOnly>("01/02/2026", CultureInfo.InvariantCulture)
			);
			Assert.Equal(
				new TimeOnly(3, 4, 5),
				(TimeOnly)ParseIt<SecureTimeOnly>("03:04:05", CultureInfo.InvariantCulture)
			);
			Assert.Equal(
				TimeSpan.FromMinutes(90),
				(TimeSpan)ParseIt<SecureTimeSpan>("01:30:00", CultureInfo.InvariantCulture)
			);
			DateTimeOffset dto = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
			Assert.Equal(
				dto,
				(DateTimeOffset)
					ParseIt<SecureDateTimeOffset>(
						"01/02/2026 03:04:05 +00:00",
						CultureInfo.InvariantCulture
					)
			);
			Assert.False(TryParseIt<SecureTimeSpan>("not-a-span", null, out _));
		}

		[Fact]
		public void SpanParsable_CoversBoolComplexString()
		{
			Assert.True(SpanParseIt<SecureBool>("True".AsSpan(), null));
			Assert.False(SpanTryParseIt<SecureBool>("maybe".AsSpan(), null, out _));
			Complex expected = new(1.5, -2.5);
			string text = expected.ToString("G", CultureInfo.InvariantCulture);
			Assert.Equal(
				expected,
				(Complex)SpanParseIt<SecureComplex>(text.AsSpan(), CultureInfo.InvariantCulture)
			);
			Assert.False(SpanTryParseIt<SecureComplex>("nope".AsSpan(), null, out _));
			Assert.Equal("hey", (string?)SpanParseIt<SecureString>("hey".AsSpan(), null));
		}

		[Fact]
		public void Utf8Formattable_ReportsTooSmall()
		{
			SecureInt p = 12345;
			Span<byte> tiny = stackalloc byte[2];
			Assert.False(((IUtf8SpanFormattable)p).TryFormat(tiny, out _, default, null));
		}

		[Fact]
		public void Enumerable_EmptyStringEnumeratesNothing()
		{
			SecureString s = new(string.Empty);
			StringBuilder seen = new();
			foreach (char c in s)
			{
				seen.Append(c);
			}

			Assert.Equal(string.Empty, seen.ToString());
		}

		[Fact]
		public void Formattable_PresentWhereBclHasIt()
		{
			Assert.Contains(typeof(IFormattable), typeof(SecureInt).GetInterfaces());
			Assert.Contains(typeof(IFormattable), typeof(SecureVector3).GetInterfaces());
			Assert.DoesNotContain(typeof(IFormattable), typeof(SecureBool).GetInterfaces());
			Assert.DoesNotContain(typeof(IFormattable), typeof(SecureMatrix4x4).GetInterfaces());
		}

		[Fact]
		public void Utf8Formattable_FormatsToUtf8Bytes()
		{
			SecureInt p = 12345;
			Span<byte> destination = stackalloc byte[16];
			Assert.True(
				((IUtf8SpanFormattable)p).TryFormat(destination, out int written, default, null)
			);
			Assert.Equal("12345", Encoding.UTF8.GetString(destination.Slice(0, written)));
			SecureRune rune = new(new Rune('é'));
			Assert.True(
				((IUtf8SpanFormattable)rune).TryFormat(destination, out written, default, null)
			);
			Assert.Equal("é", Encoding.UTF8.GetString(destination.Slice(0, written)));
		}

		[Fact]
		public void Enumerable_EnumeratesDecryptedChars()
		{
			SecureString s = "hello";
			StringBuilder seen = new();
			foreach (char c in s)
			{
				seen.Append(c);
			}

			Assert.Equal("hello", seen.ToString());
			Assert.Equal("hello", string.Concat(s));
			Assert.Equal(5, ((System.Collections.Generic.IEnumerable<char>)s).Count());
			Assert.Empty(((string?)new SecureString(string.Empty))!);
		}

		[Fact]
		public void InterfacePresence_MatchesBclSupport()
		{
			Assert.Contains(typeof(IParsable<SecureInt>), typeof(SecureInt).GetInterfaces());
			Assert.Contains(typeof(ISpanParsable<SecureInt>), typeof(SecureInt).GetInterfaces());
			Assert.Contains(typeof(IUtf8SpanFormattable), typeof(SecureInt).GetInterfaces());
			Assert.Contains(
				typeof(IUtf8SpanParsable<SecureInt>),
				typeof(SecureInt).GetInterfaces()
			);
			Assert.Contains(typeof(IParsable<SecureBool>), typeof(SecureBool).GetInterfaces());
			Assert.DoesNotContain(
				typeof(SecureBool).GetInterfaces(),
				i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUtf8SpanParsable<>)
			);
			Assert.Contains(
				typeof(IUtf8SpanParsable<SecureRune>),
				typeof(SecureRune).GetInterfaces()
			);
			Assert.DoesNotContain(
				typeof(SecureRune).GetInterfaces(),
				i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>)
			);
			Assert.Contains(
				typeof(System.Collections.Generic.IEnumerable<char>),
				typeof(SecureString).GetInterfaces()
			);
			Assert.DoesNotContain(
				typeof(SecureVector3).GetInterfaces(),
				i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>)
			);
		}
	}
}
