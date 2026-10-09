#nullable enable
using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// BCL-parity surface: every wrapper supports what its wrapped
	/// type supports — formatting, non-generic comparison, ordering, bitwise,
	/// date arithmetic, vector/matrix products, and parsing.
	/// </summary>
	public class ParityTests
	{
		[Fact]
		public void Int_BitwiseAndShifts()
		{
			SecureInt a = 12;
			SecureInt b = 10;
			Assert.Equal(12 & 10, (int)(a & b));
			Assert.Equal(12 | 10, (int)(a | b));
			Assert.Equal(12 ^ 10, (int)(a ^ b));
			Assert.Equal(~12, (int)(~a));
			Assert.Equal(+12, (int)(+a));
		}

		[Fact]
		public void Int_MixedOperandsStillResolve()
		{
			SecureInt a = 12;
			Assert.Equal(12 & 10, (int)(a & 10));
			Assert.Equal(12 + 1, (int)(a + 1));
		}

		[Fact]
		public void Numerics_FormatParseCompare()
		{
			SecureInt i = 255;
			Assert.Equal("FF", i.ToString("X"));
			Assert.Equal("255", i.ToString(null, CultureInfo.InvariantCulture));
			Assert.Equal(255, (int)SecureInt.Parse("255"));
			Assert.True(
				SecureInt.TryParse(
					"FF",
					NumberStyles.HexNumber,
					CultureInfo.InvariantCulture,
					out SecureInt hex
				)
			);
			Assert.Equal(255, (int)hex);
			Assert.False(SecureInt.TryParse("nope", out _));
			Assert.Equal(0, i.CompareTo((object)new SecureInt(255)));
			Assert.Throws<ArgumentException>(() => i.CompareTo(new object()));
		}

		[Fact]
		public void ULong_Bitwise()
		{
			SecureULong a = 12UL;
			SecureULong b = 10UL;
			Assert.Equal(12UL & 10UL, (ulong)(a & b));
			Assert.Equal(~12UL, (ulong)(~a));
			Assert.Equal(12UL, (ulong)(+a));
		}

		[Fact]
		public void Double_UnaryPlusFormatParse()
		{
			SecureDouble d = 1.5;
			Assert.Equal(1.5, (double)(+d));
			Assert.Equal(
				1.5.ToString("F2", CultureInfo.InvariantCulture),
				d.ToString("F2", CultureInfo.InvariantCulture)
			);
			Assert.Equal(1.5, (double)SecureDouble.Parse("1.5", CultureInfo.InvariantCulture));
		}

		[Fact]
		public void Decimal_UnaryPlus()
		{
			SecureDecimal d = 1.5m;
			Assert.Equal(1.5m, (decimal)(+d));
			Assert.Equal(1.5m, (decimal)SecureDecimal.Parse("1.5", CultureInfo.InvariantCulture));
		}

		[Fact]
		public void Char_OrderingIncrementParse()
		{
			SecureChar a = 'a';
			SecureChar b = 'b';
			SecureChar a2 = 'a';
			Assert.True(a < b);
			Assert.True(b > a);
			Assert.True(a <= a2);
			Assert.True(b >= a2);
			Assert.True(a.CompareTo(b) < 0);
			Assert.Equal(0, a.CompareTo((object)new SecureChar('a')));
			Assert.Equal('b', (char)(++a));
			Assert.Equal('a', (char)(--a));
			Assert.Equal('z', (char)SecureChar.Parse("z"));
		}

		[Fact]
		public void Bool_CompareParse()
		{
			SecureBool f = false;
			SecureBool t = true;
			Assert.True(f.CompareTo(t) < 0);
			Assert.Equal(0, t.CompareTo((object)new SecureBool(true)));
			Assert.True((bool)SecureBool.Parse("True"));
		}

		[Fact]
		public void String_ConcatCompare()
		{
			SecureString a = "foo";
			SecureString b = "bar";
			Assert.Equal("foobar", (string?)(a + b));
			Assert.True(a.CompareTo(b) > 0);
			Assert.Equal(0, a.CompareTo((object)new SecureString("foo")));
			Assert.True(((IComparable)new SecureString("a")).CompareTo(new SecureString("b")) < 0);
		}

		[Fact]
		public void DateTime_OrderArithmeticParse()
		{
			var d0 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			SecureDateTime d = d0;
			SecureTimeSpan day = TimeSpan.FromDays(1);
			Assert.Equal(d0.AddDays(1), (DateTime)(d + day));
			Assert.Equal(d0, (DateTime)((d + day) - day));
			Assert.Equal(TimeSpan.FromDays(1), (TimeSpan)((d + day) - d));
			Assert.True(d < (d + day));
			Assert.True((d + day) > d);
			Assert.Equal(d0.ToString("O"), d.ToString("O"));
			Assert.Equal(
				d0,
				(DateTime)
					SecureDateTime.Parse(
						"2024-01-01T00:00:00Z",
						CultureInfo.InvariantCulture,
						DateTimeStyles.RoundtripKind
					)
			);
		}

		[Fact]
		public void DateTime_MixedOperandsResolve()
		{
			SecureDateTime d = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			Assert.Equal(
				new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
				(DateTime)(d + TimeSpan.FromDays(1))
			);
			Assert.Equal(
				new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
				(DateTime)(
					new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
					+ (SecureTimeSpan)TimeSpan.FromDays(1)
				)
			);
		}

		[Fact]
		public void DateTimeOffset_OrderArithmeticConvert()
		{
			var d0 = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
			SecureDateTimeOffset d = d0;
			SecureTimeSpan h = TimeSpan.FromHours(1);
			Assert.Equal(d0.AddHours(1), (DateTimeOffset)(d + h));
			Assert.Equal(TimeSpan.FromHours(1), (TimeSpan)((d + h) - d));
			Assert.True(d < (d + h));
			SecureDateTime sd = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			SecureDateTimeOffset converted = sd;
			Assert.Equal(((DateTime)sd).ToUniversalTime(), converted.Decrypted.UtcDateTime);
		}

		[Fact]
		public void TimeSpan_FullArithmetic()
		{
			SecureTimeSpan a = TimeSpan.FromHours(2);
			SecureTimeSpan b = TimeSpan.FromHours(1);
			Assert.Equal(TimeSpan.FromHours(3), (TimeSpan)(a + b));
			Assert.Equal(TimeSpan.FromHours(1), (TimeSpan)(a - b));
			Assert.Equal(TimeSpan.FromHours(-2), (TimeSpan)(-a));
			Assert.Equal(TimeSpan.FromHours(2), (TimeSpan)(+a));
			Assert.True(b < a);
			Assert.Equal(TimeSpan.FromHours(4), (TimeSpan)(a * (SecureDouble)2.0));
			Assert.Equal(TimeSpan.FromHours(4), (TimeSpan)((SecureDouble)2.0 * a));
			Assert.Equal(TimeSpan.FromHours(1), (TimeSpan)(a / (SecureDouble)2.0));
			Assert.Equal(2.0, (double)(a / b));
			Assert.Equal(
				TimeSpan.FromHours(2),
				(TimeSpan)SecureTimeSpan.Parse("02:00:00", CultureInfo.InvariantCulture)
			);
		}

		[Fact]
		public void DateOnly_OrderParse()
		{
			SecureDateOnly a = new DateOnly(2024, 1, 1);
			SecureDateOnly b = new DateOnly(2024, 1, 2);
			Assert.True(a < b);
			Assert.True(b > a);
			Assert.True(a.CompareTo(b) < 0);
			Assert.Equal(
				new DateOnly(2024, 1, 1),
				(DateOnly)SecureDateOnly.Parse("2024-01-01", CultureInfo.InvariantCulture)
			);
		}

		[Fact]
		public void TimeOnly_OrderSubtractParse()
		{
			SecureTimeOnly a = new TimeOnly(11, 0);
			SecureTimeOnly b = new TimeOnly(12, 0);
			Assert.True(a < b);
			Assert.Equal(TimeSpan.FromHours(1), (TimeSpan)(b - a));
			Assert.Equal(
				new TimeOnly(11, 0),
				(TimeOnly)SecureTimeOnly.Parse("11:00", CultureInfo.InvariantCulture)
			);
		}

		[Fact]
		public void Guid_OrderParseExact()
		{
			Guid g1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
			Guid g2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
			SecureGuid a = g1;
			SecureGuid b = g2;
			Assert.True(a < b);
			Assert.True(b > a);
			Assert.Equal(g1, (Guid)SecureGuid.Parse(g1.ToString("D")));
			Assert.Equal(g1, (Guid)SecureGuid.ParseExact(g1.ToString("N"), "N"));
			Assert.True(SecureGuid.TryParseExact(g1.ToString("B"), "B", out SecureGuid rt));
			Assert.Equal(g1, (Guid)rt);
			Assert.Equal(g1.ToString("x"), a.ToString("x"));
		}

		[Fact]
		public void Rune_OrderFormat()
		{
			SecureRune a = new Rune('a');
			SecureRune b = new Rune('b');
			Assert.True(a < b);
			Assert.True(a.CompareTo(b) < 0);
			Assert.Equal(
				((IFormattable)new Rune('a')).ToString("X", CultureInfo.InvariantCulture),
				((IFormattable)a).ToString("X", CultureInfo.InvariantCulture)
			);
		}

		[Fact]
		public void BigInteger_BitwiseShiftParse()
		{
			SecureBigInteger a = new SecureBigInteger(12);
			SecureBigInteger b = new SecureBigInteger(10);
			Assert.Equal((BigInteger)12 & 10, (BigInteger)(a & b));
			Assert.Equal((BigInteger)12 | 10, (BigInteger)(a | b));
			Assert.Equal((BigInteger)12 ^ 10, (BigInteger)(a ^ b));
			Assert.Equal(~(BigInteger)12, (BigInteger)(~a));
			Assert.Equal((BigInteger)12, (BigInteger)(+a));
			Assert.Equal(
				BigInteger.Pow(2, 200),
				(BigInteger)
					SecureBigInteger.Parse(
						"1606938044258990275541962092341162602522202993782792835301376"
					)
			);
		}

		[Fact]
		public void Complex_UnaryIncDecFormat()
		{
			SecureComplex c = new Complex(1, 2);
			Assert.Equal(new Complex(-1, -2), (Complex)(-c));
			Assert.Equal(new Complex(2, 2), (Complex)(++c));
			Assert.Equal(new Complex(1, 2), (Complex)(--c));
			Assert.Equal(
				new Complex(1, 2).ToString("F1", CultureInfo.InvariantCulture),
				c.ToString("F1", CultureInfo.InvariantCulture)
			);
		}

		[Fact]
		public void Vectors_MulDivNegateFormat()
		{
			SecureVector2 v2 = new Vector2(2, 4);
			SecureVector2 w2 = new Vector2(1, 2);
			Assert.Equal(new Vector2(2, 8), (Vector2)(v2 * w2));
			Assert.Equal(new Vector2(2, 2), (Vector2)(v2 / w2));
			Assert.Equal(new Vector2(-2, -4), (Vector2)(-v2));
			Assert.Equal(
				new Vector2(2, 4).ToString("F0", CultureInfo.InvariantCulture),
				v2.ToString("F0", CultureInfo.InvariantCulture)
			);
			SecureVector3 v3 = new Vector3(2, 4, 6);
			Assert.Equal(new Vector3(2, 4, 6) * new Vector3(2, 4, 6), (Vector3)(v3 * v3));
			SecureVector4 v4 = new Vector4(1, 2, 3, 4);
			Assert.Equal(new Vector4(1, 1, 1, 1), (Vector4)(v4 / v4));
		}

		[Fact]
		public void Matrices_MulNegate()
		{
			var m = new Matrix3x2(1, 0, 0, 1, 5, 6);
			SecureMatrix3x2 a = m;
			Assert.Equal(Matrix3x2.Multiply(m, m), (Matrix3x2)(a * a));
			Assert.Equal(Matrix3x2.Negate(m), (Matrix3x2)(-a));
			var m4 = Matrix4x4.Identity;
			SecureMatrix4x4 b = m4;
			Assert.Equal(m4, (Matrix4x4)(b * b));
			Assert.Equal(Matrix4x4.Negate(m4), (Matrix4x4)(-b));
		}

		[Fact]
		public void Quaternion_MulDivNegate()
		{
			var q = new Quaternion(0, 0, 0, 1);
			SecureQuaternion a = q;
			Assert.Equal(q * q, (Quaternion)(a * a));
			Assert.Equal(q / q, (Quaternion)(a / a));
			Assert.Equal(Quaternion.Negate(q), (Quaternion)(-a));
		}

		[Fact]
		public void Byte_FormatParseCompare()
		{
			SecureByte v = 255;
			Assert.Equal("FF", v.ToString("X"));
			Assert.Equal(255, (byte)SecureByte.Parse("255"));
			Assert.Equal(0, v.CompareTo((object)new SecureByte(255)));
		}
	}
}
