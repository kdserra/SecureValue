using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	public class NumericsTests
	{
		[Fact]
		public void Vector2_RoundTrip()
		{
			SecureVector2 p = new Vector2(1.5f, -2.5f);
			Vector2 v = p;
			Assert.Equal(new Vector2(1.5f, -2.5f), v);
		}

		[Fact]
		public void Vector3_RoundTrip()
		{
			SecureVector3 p = new Vector3(1f, 2f, 3f);
			Assert.Equal(new Vector3(1f, 2f, 3f), (Vector3)p);
		}

		[Fact]
		public void Vector4_RoundTrip()
		{
			SecureVector4 p = new Vector4(1f, 2f, 3f, 4f);
			Assert.Equal(new Vector4(1f, 2f, 3f, 4f), (Vector4)p);
		}

		[Fact]
		public void Quaternion_RoundTrip()
		{
			var q = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f);
			SecureQuaternion p = q;
			Assert.Equal(q, (Quaternion)p);
		}

		[Fact]
		public void Plane_RoundTrip()
		{
			var pl = new Plane(Vector3.UnitY, 3.5f);
			SecurePlane p = pl;
			Assert.Equal(pl, (Plane)p);
		}

		[Fact]
		public void Matrix3x2_RoundTrip()
		{
			var m = Matrix3x2.CreateRotation(1.23f) * Matrix3x2.CreateTranslation(4f, 5f);
			SecureMatrix3x2 p = m;
			Assert.Equal(m, (Matrix3x2)p);
		}

		[Fact]
		public void Matrix4x4_RoundTrip()
		{
			var m = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 1.77f, 0.1f, 100f);
			SecureMatrix4x4 p = m;
			Assert.Equal(m, (Matrix4x4)p);
		}

		[Fact]
		public void Complex_RoundTrip()
		{
			SecureComplex p = new Complex(3.5, -2.25);
			Assert.Equal(new Complex(3.5, -2.25), (Complex)p);
		}

		[Fact]
		public void BigInteger_RoundTrip()
		{
			SecureBigInteger p = BigInteger.Parse("-170141183460469231731687303715884105728");
			Assert.Equal<object>(
				BigInteger.Parse("-170141183460469231731687303715884105728"),
				(BigInteger)p
			);

			SecureBigInteger zero = BigInteger.Zero;
			Assert.Equal<object>(BigInteger.Zero, (BigInteger)zero);
		}

		[Fact]
		public void BigInteger_Huge_RoundTrip()
		{
			BigInteger huge = BigInteger.Pow(2, 100);
			SecureBigInteger p = huge;
			Assert.Equal(huge, (BigInteger)p);
		}

		[Fact]
		public void BigInteger_Operators()
		{
			SecureBigInteger a = BigInteger.Pow(10, 30);
			SecureBigInteger b = BigInteger.One;
			Assert.Equal(BigInteger.Pow(10, 30) + 1, (BigInteger)(a + b));
			Assert.True(a > b);
		}

		[Fact]
		public void BigInteger_PlainConversions_AssignFromInt_UInt_Long_ULong()
		{
			SecureBigInteger fromInt = 100;
			SecureBigInteger fromUInt = 100u;
			SecureBigInteger fromLong = 100L;
			SecureBigInteger fromULong = 100UL;
			Assert.Equal((BigInteger)100, (BigInteger)fromInt);
			Assert.Equal((BigInteger)100, (BigInteger)fromUInt);
			Assert.Equal((BigInteger)100, (BigInteger)fromLong);
			Assert.Equal((BigInteger)100, (BigInteger)fromULong);

			int iv = 7;
			uint uv = 7u;
			long lv = 7L;
			ulong ulv = 7UL;
			SecureBigInteger vInt = iv;
			SecureBigInteger vUInt = uv;
			SecureBigInteger vLong = lv;
			SecureBigInteger vULong = ulv;
			Assert.Equal((BigInteger)7, (BigInteger)vInt);
			Assert.Equal((BigInteger)7, (BigInteger)vUInt);
			Assert.Equal((BigInteger)7, (BigInteger)vLong);
			Assert.Equal((BigInteger)7, (BigInteger)vULong);

			SecureBigInteger neg = -100;
			Assert.Equal((BigInteger)(-100), (BigInteger)neg);
		}

		[Fact]
		public void BigInteger_Arithmetic_AllOperandKinds()
		{
			SecureBigInteger a = 100;
			SecureBigInteger b = 7;
			BigInteger big = new BigInteger(7);

			// (P,P): 100+7=107, 100-7=93, 100*7=700, 100/7=14, 100%7=2.
			Assert.Equal((BigInteger)107, (BigInteger)(a + b));
			Assert.Equal((BigInteger)93, (BigInteger)(a - b));
			Assert.Equal((BigInteger)700, (BigInteger)(a * b));
			Assert.Equal((BigInteger)14, (BigInteger)(a / b));
			Assert.Equal((BigInteger)2, (BigInteger)(a % b));

			// (P,BigInteger) and (BigInteger,P).
			Assert.Equal((BigInteger)107, (BigInteger)(a + big));
			Assert.Equal((BigInteger)107, (BigInteger)(big + a));
			Assert.Equal((BigInteger)93, (BigInteger)(a - big));
			Assert.Equal((BigInteger)(-93), (BigInteger)(big - a));
			Assert.Equal((BigInteger)700, (BigInteger)(a * big));
			Assert.Equal((BigInteger)700, (BigInteger)(big * a));
			Assert.Equal((BigInteger)14, (BigInteger)(a / big));
			Assert.Equal((BigInteger)0, (BigInteger)(big / a));
			Assert.Equal((BigInteger)2, (BigInteger)(a % big));
			Assert.Equal((BigInteger)7, (BigInteger)(big % a));

			// (P,int) and (int,P).
			Assert.Equal((BigInteger)107, (BigInteger)(a + 7));
			Assert.Equal((BigInteger)107, (BigInteger)(7 + a));
			Assert.Equal((BigInteger)93, (BigInteger)(a - 7));
			Assert.Equal((BigInteger)(-93), (BigInteger)(7 - a));
			Assert.Equal((BigInteger)700, (BigInteger)(a * 7));
			Assert.Equal((BigInteger)700, (BigInteger)(7 * a));
			Assert.Equal((BigInteger)14, (BigInteger)(a / 7));
			Assert.Equal((BigInteger)0, (BigInteger)(7 / a));
			Assert.Equal((BigInteger)2, (BigInteger)(a % 7));
			Assert.Equal((BigInteger)7, (BigInteger)(7 % a));

			// (P,uint) and (uint,P).
			Assert.Equal((BigInteger)107, (BigInteger)(a + 7u));
			Assert.Equal((BigInteger)107, (BigInteger)(7u + a));
			Assert.Equal((BigInteger)93, (BigInteger)(a - 7u));
			Assert.Equal((BigInteger)(-93), (BigInteger)(7u - a));
			Assert.Equal((BigInteger)700, (BigInteger)(a * 7u));
			Assert.Equal((BigInteger)700, (BigInteger)(7u * a));
			Assert.Equal((BigInteger)14, (BigInteger)(a / 7u));
			Assert.Equal((BigInteger)0, (BigInteger)(7u / a));
			Assert.Equal((BigInteger)2, (BigInteger)(a % 7u));
			Assert.Equal((BigInteger)7, (BigInteger)(7u % a));

			// (P,long) and (long,P).
			Assert.Equal((BigInteger)107, (BigInteger)(a + 7L));
			Assert.Equal((BigInteger)107, (BigInteger)(7L + a));
			Assert.Equal((BigInteger)93, (BigInteger)(a - 7L));
			Assert.Equal((BigInteger)(-93), (BigInteger)(7L - a));
			Assert.Equal((BigInteger)700, (BigInteger)(a * 7L));
			Assert.Equal((BigInteger)700, (BigInteger)(7L * a));
			Assert.Equal((BigInteger)14, (BigInteger)(a / 7L));
			Assert.Equal((BigInteger)0, (BigInteger)(7L / a));
			Assert.Equal((BigInteger)2, (BigInteger)(a % 7L));
			Assert.Equal((BigInteger)7, (BigInteger)(7L % a));

			// (P,ulong) and (ulong,P).
			Assert.Equal((BigInteger)107, (BigInteger)(a + 7UL));
			Assert.Equal((BigInteger)107, (BigInteger)(7UL + a));
			Assert.Equal((BigInteger)93, (BigInteger)(a - 7UL));
			Assert.Equal((BigInteger)(-93), (BigInteger)(7UL - a));
			Assert.Equal((BigInteger)700, (BigInteger)(a * 7UL));
			Assert.Equal((BigInteger)700, (BigInteger)(7UL * a));
			Assert.Equal((BigInteger)14, (BigInteger)(a / 7UL));
			Assert.Equal((BigInteger)0, (BigInteger)(7UL / a));
			Assert.Equal((BigInteger)2, (BigInteger)(a % 7UL));
			Assert.Equal((BigInteger)7, (BigInteger)(7UL % a));

			// Beyond 64 bits: operators scale past long range.
			SecureBigInteger huge = BigInteger.Pow(10, 30);
			Assert.Equal(BigInteger.Pow(10, 30) + 1, (BigInteger)(huge + 1));
			Assert.Equal(BigInteger.Pow(10, 30) * 2, (BigInteger)(huge + huge));
		}

		[Fact]
		public void BigInteger_Comparisons_AllOperandKinds()
		{
			SecureBigInteger a = 100;
			SecureBigInteger b = 100;
			SecureBigInteger c = 7;
			BigInteger bigEq = new BigInteger(100);
			BigInteger bigLo = new BigInteger(7);

			// (P,P).
			Assert.True(a == b);
			Assert.False(a != b);
			Assert.False(a < b);
			Assert.True(a <= b);
			Assert.False(a > b);
			Assert.True(a >= b);
			Assert.True(c < a);
			Assert.True(c <= a);
			Assert.True(a > c);
			Assert.True(a >= c);
			Assert.True(a != c);

			// (P,BigInteger) and (BigInteger,P).
			Assert.True(a == bigEq);
			Assert.True(bigEq == a);
			Assert.True(a != bigLo);
			Assert.True(bigLo != a);
			Assert.True(a > bigLo);
			Assert.True(bigLo < a);
			Assert.True(a >= bigEq);
			Assert.True(bigEq <= a);

			// int, uint, long, ulong, both orders.
			Assert.True(a == 100);
			Assert.True(100 == a);
			Assert.True(a != 7);
			Assert.True(7 != a);
			Assert.True(a > 7);
			Assert.True(7 < a);
			Assert.True(a >= 100);
			Assert.True(100 <= a);
			Assert.True(a == 100u);
			Assert.True(100u == a);
			Assert.True(a > 7u);
			Assert.True(7u < a);
			Assert.True(a == 100L);
			Assert.True(100L == a);
			Assert.True(a > 7L);
			Assert.True(7L < a);
			Assert.True(a == 100UL);
			Assert.True(100UL == a);
			Assert.True(a > 7UL);
			Assert.True(7UL < a);
		}

		[Fact]
		public void BigInteger_Bitwise_Unary_Shifts()
		{
			// 12 = 0b1100, 10 = 0b1010: &=8, |=14, ^=6.
			SecureBigInteger a = 12;
			SecureBigInteger b = 10;

			// (P,P).
			Assert.Equal((BigInteger)8, (BigInteger)(a & b));
			Assert.Equal((BigInteger)14, (BigInteger)(a | b));
			Assert.Equal((BigInteger)6, (BigInteger)(a ^ b));

			// Mixed with BigInteger, int, uint, long, ulong, both orders.
			Assert.Equal((BigInteger)8, (BigInteger)(a & new BigInteger(10)));
			Assert.Equal((BigInteger)8, (BigInteger)(new BigInteger(12) & b));
			Assert.Equal((BigInteger)8, (BigInteger)(a & 10));
			Assert.Equal((BigInteger)8, (BigInteger)(10 & a));
			Assert.Equal((BigInteger)14, (BigInteger)(a | 10));
			Assert.Equal((BigInteger)14, (BigInteger)(10 | a));
			Assert.Equal((BigInteger)6, (BigInteger)(a ^ 10));
			Assert.Equal((BigInteger)6, (BigInteger)(10 ^ a));
			Assert.Equal((BigInteger)8, (BigInteger)(a & 10u));
			Assert.Equal((BigInteger)8, (BigInteger)(10u & a));
			Assert.Equal((BigInteger)14, (BigInteger)(a | 10u));
			Assert.Equal((BigInteger)6, (BigInteger)(a ^ 10u));
			Assert.Equal((BigInteger)8, (BigInteger)(a & 10L));
			Assert.Equal((BigInteger)8, (BigInteger)(10L & a));
			Assert.Equal((BigInteger)14, (BigInteger)(a | 10L));
			Assert.Equal((BigInteger)6, (BigInteger)(a ^ 10L));
			Assert.Equal((BigInteger)8, (BigInteger)(a & 10UL));
			Assert.Equal((BigInteger)8, (BigInteger)(10UL & a));
			Assert.Equal((BigInteger)14, (BigInteger)(a | 10UL));
			Assert.Equal((BigInteger)6, (BigInteger)(a ^ 10UL));

			// Unary: -12, +12, ~12 = -13.
			Assert.Equal((BigInteger)(-12), (BigInteger)(-a));
			Assert.Equal((BigInteger)12, (BigInteger)(+a));
			Assert.Equal(~(BigInteger)12, (BigInteger)(~a));

			SecureBigInteger inc = 12;
			inc++;
			Assert.Equal((BigInteger)13, (BigInteger)inc);
			inc--;
			Assert.Equal((BigInteger)12, (BigInteger)inc);

			// Shifts take a plain int count: 100 << 2 = 400, 100 >> 2 = 25.
			SecureBigInteger s = 100;
			Assert.Equal((BigInteger)400, (BigInteger)(s << 2));
			Assert.Equal((BigInteger)25, (BigInteger)(s >> 2));
			int n = 3;
			Assert.Equal((BigInteger)800, (BigInteger)(s << n));
			Assert.Equal((BigInteger)12, (BigInteger)(s >> n));
		}

		[Fact]
		public void SameBigInteger_DifferentCiphertext()
		{
			SecureBigInteger a = BigInteger.Pow(10, 30);
			SecureBigInteger b = BigInteger.Pow(10, 30);
			Assert.Equal((BigInteger)a, (BigInteger)b);
			Assert.NotEqual(FirstCipher(a), FirstCipher(b));
		}

		private static ulong FirstCipher(SecureBigInteger p)
		{
			var f = typeof(SecureBigInteger).GetField(
				"_ciphers",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			return ((ulong[])f.GetValue(p)!)[0];
		}

		[Fact]
		public void Tamper_Detected_BigInteger()
		{
			SecureBigInteger p = BigInteger.Pow(10, 30);
			object boxed = p;
			Type t = boxed.GetType();
			var salt = t.GetField(
				"_salt",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			salt.SetValue(boxed, (ulong)salt.GetValue(boxed)! ^ 0xABCDEF0123456789UL);
			var saltB = t.GetField(
				"_saltB",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			saltB.SetValue(boxed, (ulong)saltB.GetValue(boxed)! ^ 0x123456789ABCDEF0UL);
			var ciphers = t.GetField(
				"_ciphers",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			((ulong[])ciphers.GetValue(boxed)!)[0] ^= 1UL;
			var ciphersB = t.GetField(
				"_ciphersB",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			((ulong[])ciphersB.GetValue(boxed)!)[0] ^= 1UL;
			var tag = t.GetField(
				"_tag",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			tag.SetValue(boxed, (uint)tag.GetValue(boxed)! ^ 0xDEADBEEFu);
			var tagB = t.GetField(
				"_tagB",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)!;
			tagB.SetValue(boxed, (uint)tagB.GetValue(boxed)! ^ 0xBEEFDEADu);
			Assert.Throws<TamperedException>(() => boxed.ToString());
		}

		[Fact]
		public void Vector_Operators()
		{
			SecureVector2 a2 = new Vector2(1f, 2f);
			SecureVector2 b2 = new Vector2(3f, 4f);
			Assert.Equal(new Vector2(4f, 6f), (Vector2)(a2 + b2));
			Assert.Equal(new Vector2(-2f, -2f), (Vector2)(a2 - b2));
			Assert.True(a2 != b2);

			SecureVector3 a3 = new Vector3(1f, 2f, 3f);
			SecureVector3 b3 = new Vector3(4f, 5f, 6f);
			Assert.Equal(new Vector3(5f, 7f, 9f), (Vector3)(a3 + b3));
			Assert.Equal(new Vector3(-3f, -3f, -3f), (Vector3)(a3 - b3));
			Assert.True(a3 == new SecureVector3(new Vector3(1f, 2f, 3f)));

			SecureVector4 a4 = new Vector4(1f, 2f, 3f, 4f);
			SecureVector4 b4 = new Vector4(4f, 3f, 2f, 1f);
			Assert.Equal(new Vector4(5f, 5f, 5f, 5f), (Vector4)(a4 + b4));
			Assert.True(a4 != b4);
		}

		[Fact]
		public void Quaternion_Matrix_Plane_Complex_Operators()
		{
			var q = new Quaternion(1f, 2f, 3f, 4f);
			SecureQuaternion a = q,
				b = q;
			Assert.Equal(q, (Quaternion)(a + b - b));
			Assert.True(a == b);

			var m = new Matrix4x4(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 5f, 6f, 7f, 1f);
			SecureMatrix4x4 x = m,
				y = m;
			Assert.Equal(m, (Matrix4x4)(x + y - y));
			Assert.True(x == y);
			Assert.True(x != (x + y)); // default reads as default; compare live values here

			var m2 = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
			SecureMatrix3x2 u = m2,
				v = m2;
			Assert.Equal(m2, (Matrix3x2)(u + v - v));
			Assert.True(u == v);

			var pl = new Plane(new Vector3(0f, 1f, 0f), 3.5f);
			SecurePlane p1 = pl,
				p2 = pl;
			SecurePlane p3 = new Plane(new Vector3(0f, 1f, 0f), 4.5f);
			Assert.True(p1 == p2);
			Assert.True(p1 != p3); // default reads as default; compare live values here

			SecureComplex c1 = new Complex(1.5, -2.5);
			SecureComplex c2 = new Complex(0.5, 0.5);
			Assert.Equal(new Complex(2.0, -2.0), (Complex)(c1 + c2));
			Assert.Equal(new Complex(1.0, -3.0), (Complex)(c1 - c2));
			Assert.Equal(new Complex(2.0, -0.5), (Complex)(c1 * c2));
			Assert.True(c1 != c2);
		}

		[Fact]
		public void Default_Read_ReturnsDefault()
		{
			// All-default backing fields read as default (see WrapperTests).
			Assert.Equal(default(BigInteger), (BigInteger)default(SecureBigInteger));
			Assert.Equal(default(Vector2), (Vector2)default(SecureVector2));
			Assert.Equal(default(Matrix4x4), (Matrix4x4)default(SecureMatrix4x4));
		}

		[Fact]
		public void Unset_ReadsDefaultWithoutMaterialization()
		{
			Assert.Equal(BigInteger.Zero, (BigInteger)default(SecureBigInteger));
			Assert.Equal(default(Vector2), (Vector2)default(SecureVector2));
			Assert.Equal(default(Matrix4x4), (Matrix4x4)default(SecureMatrix4x4));
		}
	}
}
