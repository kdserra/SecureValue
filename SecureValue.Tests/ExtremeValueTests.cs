using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Dedicated lowest/highest/zero round-trip coverage for every wrapper type.
	/// A decrypt failure that resolves to 0 is invisible against a 0 constant, so the
	/// demo and sample values deliberately avoid these ranges — this suite covers them
	/// explicitly instead. Bit-exactness is asserted, not approximate equality.
	/// </summary>
	public class ExtremeValueTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Bool_Extremes_RoundTrip(bool value)
		{
			SecureBool p = value;
			Assert.Equal(value, (bool)p);
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
		[InlineData(sbyte.MinValue)]
		[InlineData((sbyte)0)]
		[InlineData(sbyte.MaxValue)]
		public void SByte_Extremes_RoundTrip(sbyte value)
		{
			SecureSByte p = value;
			Assert.Equal(value, (sbyte)p);
		}

		[Theory]
		[InlineData(char.MinValue)]
		[InlineData(char.MaxValue)]
		public void Char_Extremes_RoundTrip(char value)
		{
			SecureChar p = value;
			Assert.Equal(value, (char)p);
		}

		[Theory]
		[InlineData(short.MinValue)]
		[InlineData((short)0)]
		[InlineData(short.MaxValue)]
		public void Short_Extremes_RoundTrip(short value)
		{
			SecureShort p = value;
			Assert.Equal(value, (short)p);
		}

		[Theory]
		[InlineData(ushort.MinValue)]
		[InlineData(ushort.MaxValue)]
		public void UShort_Extremes_RoundTrip(ushort value)
		{
			SecureUShort p = value;
			Assert.Equal(value, (ushort)p);
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
		[InlineData(uint.MinValue)]
		[InlineData(uint.MaxValue)]
		public void UInt_Extremes_RoundTrip(uint value)
		{
			SecureUInt p = value;
			Assert.Equal(value, (uint)p);
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

		[Theory]
		[InlineData(ulong.MinValue)]
		[InlineData(ulong.MaxValue)]
		public void ULong_Extremes_RoundTrip(ulong value)
		{
			SecureULong p = value;
			Assert.Equal(value, (ulong)p);
		}

		[Theory]
		[InlineData(float.MinValue)]
		[InlineData(0.0f)]
		[InlineData(float.MaxValue)]
		[InlineData(float.PositiveInfinity)]
		[InlineData(float.NegativeInfinity)]
		public void Float_Extremes_RoundTrip(float value)
		{
			SecureFloat p = value;
			Assert.Equal(
				BitConverter.SingleToInt32Bits(value),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[Fact]
		public void Float_NegativeZero_Extremes_RoundTrip()
		{
			// -0.0f == 0.0f by value but differs in the sign bit: assert bitwise.
			// (InlineData cannot hold both: the analyzer flags them as duplicates.)
			SecureFloat p = -0.0f;
			Assert.Equal(
				BitConverter.SingleToInt32Bits(-0.0f),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[Fact]
		public void Float_NaN_Extremes_RoundTrip()
		{
			SecureFloat p = float.NaN;
			Assert.Equal(
				BitConverter.SingleToInt32Bits(float.NaN),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[Theory]
		[InlineData(double.MinValue)]
		[InlineData(0.0)]
		[InlineData(double.MaxValue)]
		[InlineData(double.PositiveInfinity)]
		[InlineData(double.NegativeInfinity)]
		public void Double_Extremes_RoundTrip(double value)
		{
			SecureDouble p = value;
			Assert.Equal(
				BitConverter.DoubleToInt64Bits(value),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Fact]
		public void Double_NegativeZero_Extremes_RoundTrip()
		{
			// -0.0 == 0.0 by value but differs in the sign bit: assert bitwise.
			// (InlineData cannot hold both: the analyzer flags them as duplicates.)
			SecureDouble p = -0.0;
			Assert.Equal(
				BitConverter.DoubleToInt64Bits(-0.0),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Fact]
		public void Double_NaN_Extremes_RoundTrip()
		{
			SecureDouble p = double.NaN;
			Assert.Equal(
				BitConverter.DoubleToInt64Bits(double.NaN),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Fact]
		public void Decimal_Extremes_RoundTrip()
		{
			SecureDecimal lo = decimal.MinValue;
			Assert.Equal(decimal.MinValue, (decimal)lo);
			SecureDecimal zero = decimal.Zero;
			Assert.Equal(decimal.Zero, (decimal)zero);
			SecureDecimal hi = decimal.MaxValue;
			Assert.Equal(decimal.MaxValue, (decimal)hi);
		}

		[Fact]
		public void String_Extremes_RoundTrip()
		{
			SecureString empty = "";
			Assert.Equal("", (string?)empty);
			SecureString nil = new SecureString(null!);
			Assert.Equal("", (string?)nil);
			string longValue = new string('z', 1000);
			SecureString p = longValue;
			Assert.Equal(longValue, (string?)p);
		}

		[Fact]
		public void Guid_Extremes_RoundTrip()
		{
			SecureGuid empty = Guid.Empty;
			Assert.Equal(Guid.Empty, (Guid)empty);
			var max = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
			SecureGuid hi = max;
			Assert.Equal(max, (Guid)hi);
		}

		[Fact]
		public void DateTime_Extremes_RoundTrip()
		{
			SecureDateTime lo = DateTime.MinValue;
			Assert.Equal(DateTime.MinValue, (DateTime)lo);
			SecureDateTime hi = DateTime.MaxValue;
			Assert.Equal(DateTime.MaxValue, (DateTime)hi);
		}

		[Fact]
		public void DateTimeOffset_Extremes_RoundTrip()
		{
			SecureDateTimeOffset lo = DateTimeOffset.MinValue;
			Assert.Equal(DateTimeOffset.MinValue, (DateTimeOffset)lo);
			SecureDateTimeOffset hi = DateTimeOffset.MaxValue;
			Assert.Equal(DateTimeOffset.MaxValue, (DateTimeOffset)hi);
		}

		[Fact]
		public void TimeSpan_Extremes_RoundTrip()
		{
			SecureTimeSpan zero = TimeSpan.Zero;
			Assert.Equal(TimeSpan.Zero, (TimeSpan)zero);
			SecureTimeSpan lo = TimeSpan.MinValue;
			Assert.Equal(TimeSpan.MinValue, (TimeSpan)lo);
			SecureTimeSpan hi = TimeSpan.MaxValue;
			Assert.Equal(TimeSpan.MaxValue, (TimeSpan)hi);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_Extremes_RoundTrip()
		{
			SecureDateOnly lo = DateOnly.MinValue;
			Assert.Equal(DateOnly.MinValue, (DateOnly)lo);
			SecureDateOnly hi = DateOnly.MaxValue;
			Assert.Equal(DateOnly.MaxValue, (DateOnly)hi);
		}

		[Fact]
		public void TimeOnly_Extremes_RoundTrip()
		{
			SecureTimeOnly lo = TimeOnly.MinValue;
			Assert.Equal(TimeOnly.MinValue, (TimeOnly)lo);
			SecureTimeOnly hi = TimeOnly.MaxValue;
			Assert.Equal(TimeOnly.MaxValue, (TimeOnly)hi);
		}
#endif

#if NET
		[Theory]
		[InlineData(0x0)]
		[InlineData(0x10FFFF)]
		public void Rune_Extremes_RoundTrip(int codePoint)
		{
			var value = new System.Text.Rune(codePoint);
			SecureRune p = value;
			Assert.Equal(value, (System.Text.Rune)p);
		}
#endif

		[Fact]
		public void BigInteger_Extremes_RoundTrip()
		{
			SecureBigInteger zero = BigInteger.Zero;
			Assert.Equal(BigInteger.Zero, (BigInteger)zero);
			BigInteger huge = BigInteger.Pow(2, 256);
			SecureBigInteger hi = huge;
			Assert.Equal(huge, (BigInteger)hi);
			SecureBigInteger lo = -huge;
			Assert.Equal(-huge, (BigInteger)lo);
		}

		[Fact]
		public void Complex_Extremes_RoundTrip()
		{
			SecureComplex zero = Complex.Zero;
			Assert.Equal(Complex.Zero, (Complex)zero);
			var hi = new Complex(double.MaxValue, double.MaxValue);
			SecureComplex phi = hi;
			Assert.Equal(hi, (Complex)phi);
			var lo = new Complex(double.MinValue, double.MinValue);
			SecureComplex plo = lo;
			Assert.Equal(lo, (Complex)plo);
		}

		[Fact]
		public void Vector2_Extremes_RoundTrip()
		{
			Assert.Equal(Vector2.Zero, (Vector2)(SecureVector2)Vector2.Zero);
			var lo = new Vector2(float.MinValue, float.MinValue);
			Assert.Equal(lo, (Vector2)(SecureVector2)lo);
			var hi = new Vector2(float.MaxValue, float.MaxValue);
			Assert.Equal(hi, (Vector2)(SecureVector2)hi);
		}

		[Fact]
		public void Vector3_Extremes_RoundTrip()
		{
			Assert.Equal(Vector3.Zero, (Vector3)(SecureVector3)Vector3.Zero);
			var lo = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Assert.Equal(lo, (Vector3)(SecureVector3)lo);
			var hi = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Assert.Equal(hi, (Vector3)(SecureVector3)hi);
		}

		[Fact]
		public void Vector4_Extremes_RoundTrip()
		{
			Assert.Equal(Vector4.Zero, (Vector4)(SecureVector4)Vector4.Zero);
			var lo = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
			Assert.Equal(lo, (Vector4)(SecureVector4)lo);
			var hi = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
			Assert.Equal(hi, (Vector4)(SecureVector4)hi);
		}

		[Fact]
		public void Quaternion_Extremes_RoundTrip()
		{
			var zero = new Quaternion(0f, 0f, 0f, 0f);
			Assert.Equal(zero, (Quaternion)(SecureQuaternion)zero);
			var lo = new Quaternion(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
			Assert.Equal(lo, (Quaternion)(SecureQuaternion)lo);
			var hi = new Quaternion(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
			Assert.Equal(hi, (Quaternion)(SecureQuaternion)hi);
		}

		[Fact]
		public void Plane_Extremes_RoundTrip()
		{
			var zero = new Plane(Vector3.Zero, 0f);
			Assert.Equal(zero, (Plane)(SecurePlane)zero);
			var lo = new Plane(
				new Vector3(float.MinValue, float.MinValue, float.MinValue),
				float.MinValue
			);
			Assert.Equal(lo, (Plane)(SecurePlane)lo);
			var hi = new Plane(
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				float.MaxValue
			);
			Assert.Equal(hi, (Plane)(SecurePlane)hi);
		}

		[Fact]
		public void Matrix3x2_Extremes_RoundTrip()
		{
			var zero = default(Matrix3x2);
			Assert.Equal(zero, (Matrix3x2)(SecureMatrix3x2)zero);
			Matrix3x2 lo = Fill3x2(float.MinValue);
			Assert.Equal(lo, (Matrix3x2)(SecureMatrix3x2)lo);
			Matrix3x2 hi = Fill3x2(float.MaxValue);
			Assert.Equal(hi, (Matrix3x2)(SecureMatrix3x2)hi);
		}

		[Fact]
		public void Matrix4x4_Extremes_RoundTrip()
		{
			var zero = default(Matrix4x4);
			Assert.Equal(zero, (Matrix4x4)(SecureMatrix4x4)zero);
			Matrix4x4 lo = Fill4x4(float.MinValue);
			Assert.Equal(lo, (Matrix4x4)(SecureMatrix4x4)lo);
			Matrix4x4 hi = Fill4x4(float.MaxValue);
			Assert.Equal(hi, (Matrix4x4)(SecureMatrix4x4)hi);
		}

		private static Matrix3x2 Fill3x2(float v) => new Matrix3x2(v, v, v, v, v, v);

		private static Matrix4x4 Fill4x4(float v) =>
			new Matrix4x4(v, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v);
	}
}
