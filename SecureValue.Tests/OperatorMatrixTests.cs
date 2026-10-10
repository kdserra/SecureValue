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
	/// Minimal-operator contract (SecureValue.CodeGen): every wrapper owns
	/// O(P,P) + O(P,T_own) + O(T_own,P) plus int-literal interop twins
	/// O(P,int) + O(int,P). Cross-width mixes beyond int are compile errors
	/// (CS0019) or mirror ties (CS9342) — both intentional; use an explicit
	/// cast or .Decrypted. If any assert here fails to COMPILE unexpectedly,
	/// the generator's operator set has regressed — fix the generator.
	/// </summary>
	public class OperatorMatrixTests
	{
		[Fact]
		public void Int_OwnAndIntTwinsBindLiterals()
		{
			SecureInt p = 1000;
			Assert.Equal(1007, (int)(p + 7));
			Assert.Equal(993, (int)(p - 7));
			Assert.Equal(7000, (int)(p * 7));
			Assert.Equal(142, (int)(p / 7));
			Assert.Equal(6, (int)(p % 7));
			Assert.True(p == 1000);
			Assert.True(p != 7);
			Assert.True(p > 7);
			Assert.True(p >= 1000);
			Assert.True(p < 1001);
			Assert.True(p <= 1000);
			Assert.Equal(1000 & 7, (int)(p & 7));
			Assert.Equal(1000 | 7, (int)(p | 7));
			Assert.Equal(1000 ^ 7, (int)(p ^ 7));
			Assert.Equal(1000 << 2, (int)(p << 2));
			Assert.Equal(1000 >> 2, (int)(p >> 2));
			p += 7;
			Assert.Equal(1007, (int)p);
			p -= 7;
			Assert.Equal(1000, (int)p);
			p *= 2;
			Assert.Equal(2000, (int)p);
			p /= 2;
			Assert.Equal(1000, (int)p);
			p %= 7;
			Assert.Equal(6, (int)p);
			p = 1000;
			p &= 7;
			Assert.Equal(1000 & 7, (int)p);
			p = 1000;
			p |= 7;
			Assert.Equal(1000 | 7, (int)p);
			p = 1000;
			p ^= 7;
			Assert.Equal(1000 ^ 7, (int)p);
		}

		[Fact]
		public void ReverseOrder_BindsExactMirror()
		{
			SecureInt p = 1000;
			SecureLong big = 1000L;
			Assert.Equal(1007, (int)(7 + p));
			Assert.Equal(1007L, (long)(7L + big));
			Assert.True(7 < p);
			Assert.True(1000 == (int)p);
			SecureULong u = 100UL;
			Assert.True((decimal)u > 2.5m);
			Assert.True(2.5m < (decimal)u);
			Assert.Equal(102.5m, 2.5m + (decimal)u);
		}

		[Fact]
		public void CrossSecure_NeedsExplicitCast()
		{
			// No (P1,P2) pairs: cross-Secure mixes do not resolve implicitly.
			// Spell the widening explicitly; the value math is unchanged.
			SecureLong bossHP = 1000L;
			SecureInt heal = 7;
			SecureLong healed = bossHP + new SecureLong(heal.Decrypted);
			Assert.Equal(1007L, (long)healed);
			bossHP += new SecureLong(heal.Decrypted);
			Assert.Equal(1007L, (long)bossHP);
			Assert.True(bossHP > new SecureLong(heal.Decrypted));
		}

		[Fact]
		public void SmallTypes_IntLiteralsBindWithoutSuffix()
		{
			// sbyte/byte/short/ushort have no literal suffix; every literal
			// is int, so the (P,int) twin is what makes these compile.
			SecureSByte a = 100;
			Assert.IsType<SecureInt>(a + 5);
			Assert.Equal(105, (int)(a + 5));
			SecureShort s = 100;
			Assert.Equal(95, (int)(s - 5));
			SecureByte b = 100;
			Assert.Equal(107, (int)(b + 7));
			SecureUShort us = 100;
			Assert.Equal(93, (int)(us - 7));
			// Typed sub-int variables still need a visible cast (no silent
			// bridge): short -> int is explicit at the call site.
			short sv = 7;
			Assert.Equal(107, (int)(a + (int)sv));
		}

		[Fact]
		public void Widening_IntInteropReturnsWidenedSecure()
		{
			// Returns follow the BCL: never narrow silently.
			SecureLong a = 100L;
			Assert.IsType<SecureLong>(a + 7);
			Assert.Equal(107L, (long)(a + 7));
			SecureUInt u = 4000000000U;
			Assert.IsType<SecureLong>(u + 7);
			Assert.Equal(4000000007L, (long)(u + 7));
			SecureInt n = 100;
			Assert.IsType<SecureInt>(n + 7);
			SecureSByte tiny = 100;
			Assert.IsType<SecureInt>(tiny + 5);
		}

		[Fact]
		public void ULong_SuffixedLiteralsBindOwnTwins()
		{
			SecureULong v = ulong.MaxValue;
			// BCL rejects ulong+int variables, so there is no (P,int) twin
			// here; the UL-suffixed literal binds (P,ulong) exactly.
			Assert.Equal(ulong.MaxValue - 1, (ulong)(v - 1UL));
			Assert.True(v == ulong.MaxValue);
			Assert.True(v != 1UL);
			Assert.True(v > 1UL);
			SecureUInt u = 4000000000U;
			Assert.Equal(4000000001UL, (ulong)(u + 1U));
		}

		[Fact]
		public void CharTwin_BindsExplicitInt()
		{
			SecureInt n = 100;
			Assert.Equal(100 + 'A', (int)(n + (int)'A'));
		}

		[Fact]
		public void Float_MixesWithIntBothOrders()
		{
			SecureFloat f = 100f;
			Assert.IsType<SecureFloat>(f + 2);
			Assert.Equal(102f, (float)(f + 2));
			Assert.Equal(102f, (float)(2 + f));
			SecureDouble d = 100.0;
			Assert.IsType<SecureDouble>(d + 2);
			Assert.Equal(102.0, (double)(d + 2));
			SecureDecimal m = 100m;
			Assert.IsType<SecureDecimal>(m + 2);
			Assert.Equal(102m, (decimal)(m + 2));
		}

		[Fact]
		public void Char_MixesWithIntLiterals()
		{
			SecureChar c = 'A';
			Assert.IsType<SecureInt>(c + 1);
			Assert.Equal('A' + 1, (int)(c + 1));
			Assert.Equal('A' + 1, (int)(1 + c));
			Assert.True(c == 'A');
			Assert.True('A' == c);
			// Cross-Secure (char + SecureInt) is a mirror tie by design;
			// cast one side to the plain type.
			SecureInt n = 7;
			Assert.Equal('A' + 7, (int)(c + (int)n));
			Assert.Equal('A' + 7, (int)((int)n + c));
		}

		[Fact]
		public void Bool_EqualityWithPrimitive()
		{
			SecureBool p = true;
			Assert.True(p == true);
			Assert.True(true == p);
			Assert.True(p != false);
			Assert.True(false != p);
		}

		[Fact]
		public void String_ConcatAndEqualityWithPrimitive()
		{
			SecureString s = "foo";
			Assert.Equal("foobar", (string?)(s + "bar"));
			Assert.Equal("barfoo", (string?)("bar" + s));
			Assert.True(s == "foo");
			Assert.True("foo" == s);
			Assert.True(s != "bar");
			Assert.True(s != null);
		}

		[Fact]
		public void DateTime_ArithmeticWithTimeSpan()
		{
			SecureDateTime dt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan ts = TimeSpan.FromDays(1);
			Assert.Equal(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), (DateTime)(dt + ts));
			Assert.Equal(
				new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
				(DateTime)(dt - ts)
			);
			Assert.Equal(TimeSpan.FromDays(1), (TimeSpan)(dt + ts - dt));
			SecureTimeSpan span = TimeSpan.FromMinutes(90);
			Assert.Equal(TimeSpan.FromMinutes(180), (TimeSpan)(span * 2.0));
			Assert.Equal(TimeSpan.FromMinutes(180), (TimeSpan)(2.0 * span));
			Assert.Equal(TimeSpan.FromMinutes(45), (TimeSpan)(span / 2.0));
			Assert.Equal(2.0, (span * 2.0 / span));
		}

		[Fact]
		public void PrimitiveInterfaces_DispatchThroughInterface()
		{
			SecureInt p = 100;
			Assert.True(((IEquatable<int>)p).Equals(100));
			Assert.True(((IComparable<int>)p).CompareTo(99) > 0);
			SecureString s = "foo";
			Assert.True(((IEquatable<string>)s).Equals("foo"));
			SecureGuid g = Guid.Empty;
			Assert.True(((IEquatable<Guid>)g).Equals(Guid.Empty));
		}

		[Fact]
		public void ConvertibleAndSpanFormat_ForwardToDecrypted()
		{
			SecureInt p = 100;
			Assert.Equal(100, ((IConvertible)p).ToInt32(null));
			Assert.Equal(TypeCode.Int32, ((IConvertible)p).GetTypeCode());
			Span<char> destination = stackalloc char[16];
			Assert.True(p.TryFormat(destination, out int written, default, null));
			Assert.Equal("100", destination.Slice(0, written).ToString());
		}

		[Fact]
		public void SecureConversions_AreExplicitAndMirrorBcl()
		{
			SecureInt n = 100;
			Assert.Equal(100L, (long)(SecureLong)n);
			Assert.Equal(100.0, (double)(SecureDouble)n);
			SecureSByte small = 100;
			Assert.Equal(100, (int)(SecureInt)small);
			SecureChar ch = 'A';
			Assert.Equal((int)'A', (int)(SecureInt)ch);
			SecureLong big = 100L;
			Assert.Equal(100, (int)(SecureInt)big);
			SecureDouble dbl = 100.0;
			Assert.Equal(100, (int)(SecureInt)dbl);
		}

		[Fact]
		public void BigInteger_ExplicitTwinsMirrorBcl()
		{
			BigInteger plain = new BigInteger(100);
			SecureBigInteger secured = plain;
			Assert.Equal((sbyte)plain, (sbyte)(SecureSByte)secured);
			Assert.Equal((byte)plain, (byte)(SecureByte)secured);
			Assert.Equal((short)plain, (short)(SecureShort)secured);
			Assert.Equal((ushort)plain, (ushort)(SecureUShort)secured);
			Assert.Equal((int)plain, (int)(SecureInt)secured);
			Assert.Equal((uint)plain, (uint)(SecureUInt)secured);
			Assert.Equal((long)plain, (long)(SecureLong)secured);
			Assert.Equal((ulong)plain, (ulong)(SecureULong)secured);
			Assert.Equal((char)100, (char)(SecureChar)secured);
			Assert.Equal((float)plain, (float)(SecureFloat)secured);
			Assert.Equal((double)plain, (double)(SecureDouble)secured);
			Assert.Equal((decimal)plain, (decimal)(SecureDecimal)secured);
		}

		[Fact]
		public void BigInteger_ExplicitTwinsThrowOnOverflowLikeBcl()
		{
			SecureBigInteger huge = BigInteger.Pow(2, 100);
			Assert.Throws<OverflowException>(() => (SecureSByte)huge);
			Assert.Throws<OverflowException>(() => (SecureByte)huge);
			Assert.Throws<OverflowException>(() => (SecureShort)huge);
			Assert.Throws<OverflowException>(() => (SecureUShort)huge);
			Assert.Throws<OverflowException>(() => (SecureInt)huge);
			Assert.Throws<OverflowException>(() => (SecureUInt)huge);
			Assert.Throws<OverflowException>(() => (SecureLong)huge);
			Assert.Throws<OverflowException>(() => (SecureULong)huge);
			Assert.Throws<OverflowException>(() => (SecureDecimal)huge);
			SecureBigInteger negative = new BigInteger(-1);
			Assert.Throws<OverflowException>(() => (SecureUInt)negative);
			Assert.Throws<OverflowException>(() => (SecureULong)negative);
		}

		[Fact]
		public void ExplicitCasts_PlainAndSecuredAreEqual()
		{
			sbyte plainSbyte = 100;
			SecureSByte securedSbyte = plainSbyte;
			Assert.Equal(plainSbyte, (sbyte)securedSbyte);
			Assert.Equal((byte)plainSbyte, (byte)securedSbyte);
			Assert.Equal((short)plainSbyte, (short)securedSbyte);
			Assert.Equal((ushort)plainSbyte, (ushort)securedSbyte);
			Assert.Equal((int)plainSbyte, (int)securedSbyte);
			Assert.Equal((uint)plainSbyte, (uint)securedSbyte);
			Assert.Equal((long)plainSbyte, (long)securedSbyte);
			Assert.Equal((ulong)plainSbyte, (ulong)securedSbyte);
			Assert.Equal((char)plainSbyte, (char)securedSbyte);
			Assert.Equal((float)plainSbyte, (float)securedSbyte);
			Assert.Equal((double)plainSbyte, (double)securedSbyte);
			Assert.Equal((decimal)plainSbyte, (decimal)securedSbyte);

			byte plainByte = 100;
			SecureByte securedByte = plainByte;
			Assert.Equal((sbyte)plainByte, (sbyte)securedByte);
			Assert.Equal(plainByte, (byte)securedByte);
			Assert.Equal((short)plainByte, (short)securedByte);
			Assert.Equal((ushort)plainByte, (ushort)securedByte);
			Assert.Equal((int)plainByte, (int)securedByte);
			Assert.Equal((uint)plainByte, (uint)securedByte);
			Assert.Equal((long)plainByte, (long)securedByte);
			Assert.Equal((ulong)plainByte, (ulong)securedByte);
			Assert.Equal((char)plainByte, (char)securedByte);
			Assert.Equal((float)plainByte, (float)securedByte);
			Assert.Equal((double)plainByte, (double)securedByte);
			Assert.Equal((decimal)plainByte, (decimal)securedByte);

			short plainShort = 100;
			SecureShort securedShort = plainShort;
			Assert.Equal((sbyte)plainShort, (sbyte)securedShort);
			Assert.Equal((byte)plainShort, (byte)securedShort);
			Assert.Equal(plainShort, (short)securedShort);
			Assert.Equal((ushort)plainShort, (ushort)securedShort);
			Assert.Equal((int)plainShort, (int)securedShort);
			Assert.Equal((uint)plainShort, (uint)securedShort);
			Assert.Equal((long)plainShort, (long)securedShort);
			Assert.Equal((ulong)plainShort, (ulong)securedShort);
			Assert.Equal((char)plainShort, (char)securedShort);
			Assert.Equal((float)plainShort, (float)securedShort);
			Assert.Equal((double)plainShort, (double)securedShort);
			Assert.Equal((decimal)plainShort, (decimal)securedShort);

			ushort plainUShort = 100;
			SecureUShort securedUShort = plainUShort;
			Assert.Equal((sbyte)plainUShort, (sbyte)securedUShort);
			Assert.Equal((byte)plainUShort, (byte)securedUShort);
			Assert.Equal((short)plainUShort, (short)securedUShort);
			Assert.Equal(plainUShort, (ushort)securedUShort);
			Assert.Equal((int)plainUShort, (int)securedUShort);
			Assert.Equal((uint)plainUShort, (uint)securedUShort);
			Assert.Equal((long)plainUShort, (long)securedUShort);
			Assert.Equal((ulong)plainUShort, (ulong)securedUShort);
			Assert.Equal((char)plainUShort, (char)securedUShort);
			Assert.Equal((float)plainUShort, (float)securedUShort);
			Assert.Equal((double)plainUShort, (double)securedUShort);
			Assert.Equal((decimal)plainUShort, (decimal)securedUShort);

			int plainInt = 100;
			SecureInt securedInt = plainInt;
			Assert.Equal((sbyte)plainInt, (sbyte)securedInt);
			Assert.Equal((byte)plainInt, (byte)securedInt);
			Assert.Equal((short)plainInt, (short)securedInt);
			Assert.Equal((ushort)plainInt, (ushort)securedInt);
			Assert.Equal(plainInt, (int)securedInt);
			Assert.Equal((uint)plainInt, (uint)securedInt);
			Assert.Equal((long)plainInt, (long)securedInt);
			Assert.Equal((ulong)plainInt, (ulong)securedInt);
			Assert.Equal((char)plainInt, (char)securedInt);
			Assert.Equal((float)plainInt, (float)securedInt);
			Assert.Equal((double)plainInt, (double)securedInt);
			Assert.Equal((decimal)plainInt, (decimal)securedInt);

			uint plainUInt = 100;
			SecureUInt securedUInt = plainUInt;
			Assert.Equal((sbyte)plainUInt, (sbyte)securedUInt);
			Assert.Equal((byte)plainUInt, (byte)securedUInt);
			Assert.Equal((short)plainUInt, (short)securedUInt);
			Assert.Equal((ushort)plainUInt, (ushort)securedUInt);
			Assert.Equal((int)plainUInt, (int)securedUInt);
			Assert.Equal(plainUInt, (uint)securedUInt);
			Assert.Equal((long)plainUInt, (long)securedUInt);
			Assert.Equal((ulong)plainUInt, (ulong)securedUInt);
			Assert.Equal((char)plainUInt, (char)securedUInt);
			Assert.Equal((float)plainUInt, (float)securedUInt);
			Assert.Equal((double)plainUInt, (double)securedUInt);
			Assert.Equal((decimal)plainUInt, (decimal)securedUInt);

			long plainLong = 100;
			SecureLong securedLong = plainLong;
			Assert.Equal((sbyte)plainLong, (sbyte)securedLong);
			Assert.Equal((byte)plainLong, (byte)securedLong);
			Assert.Equal((short)plainLong, (short)securedLong);
			Assert.Equal((ushort)plainLong, (ushort)securedLong);
			Assert.Equal((int)plainLong, (int)securedLong);
			Assert.Equal((uint)plainLong, (uint)securedLong);
			Assert.Equal(plainLong, (long)securedLong);
			Assert.Equal((ulong)plainLong, (ulong)securedLong);
			Assert.Equal((char)plainLong, (char)securedLong);
			Assert.Equal((float)plainLong, (float)securedLong);
			Assert.Equal((double)plainLong, (double)securedLong);
			Assert.Equal((decimal)plainLong, (decimal)securedLong);

			ulong plainULong = 100;
			SecureULong securedULong = plainULong;
			Assert.Equal((sbyte)plainULong, (sbyte)securedULong);
			Assert.Equal((byte)plainULong, (byte)securedULong);
			Assert.Equal((short)plainULong, (short)securedULong);
			Assert.Equal((ushort)plainULong, (ushort)securedULong);
			Assert.Equal((int)plainULong, (int)securedULong);
			Assert.Equal((uint)plainULong, (uint)securedULong);
			Assert.Equal((long)plainULong, (long)securedULong);
			Assert.Equal(plainULong, (ulong)securedULong);
			Assert.Equal((char)plainULong, (char)securedULong);
			Assert.Equal((float)plainULong, (float)securedULong);
			Assert.Equal((double)plainULong, (double)securedULong);
			Assert.Equal((decimal)plainULong, (decimal)securedULong);

			char plainChar = 'd';
			SecureChar securedChar = plainChar;
			Assert.Equal((sbyte)plainChar, (sbyte)securedChar);
			Assert.Equal((byte)plainChar, (byte)securedChar);
			Assert.Equal((short)plainChar, (short)securedChar);
			Assert.Equal((ushort)plainChar, (ushort)securedChar);
			Assert.Equal((int)plainChar, (int)securedChar);
			Assert.Equal((uint)plainChar, (uint)securedChar);
			Assert.Equal((long)plainChar, (long)securedChar);
			Assert.Equal((ulong)plainChar, (ulong)securedChar);
			Assert.Equal(plainChar, (char)securedChar);
			Assert.Equal((float)plainChar, (float)securedChar);
			Assert.Equal((double)plainChar, (double)securedChar);
			Assert.Equal((decimal)plainChar, (decimal)securedChar);

			float plainFloat = 100f;
			SecureFloat securedFloat = plainFloat;
			Assert.Equal((sbyte)plainFloat, (sbyte)securedFloat);
			Assert.Equal((byte)plainFloat, (byte)securedFloat);
			Assert.Equal((short)plainFloat, (short)securedFloat);
			Assert.Equal((ushort)plainFloat, (ushort)securedFloat);
			Assert.Equal((int)plainFloat, (int)securedFloat);
			Assert.Equal((uint)plainFloat, (uint)securedFloat);
			Assert.Equal((long)plainFloat, (long)securedFloat);
			Assert.Equal((ulong)plainFloat, (ulong)securedFloat);
			Assert.Equal((char)plainFloat, (char)securedFloat);
			Assert.Equal(plainFloat, (float)securedFloat);
			Assert.Equal((double)plainFloat, (double)securedFloat);
			Assert.Equal((decimal)plainFloat, (decimal)securedFloat);

			double plainDouble = 100.0;
			SecureDouble securedDouble = plainDouble;
			Assert.Equal((sbyte)plainDouble, (sbyte)securedDouble);
			Assert.Equal((byte)plainDouble, (byte)securedDouble);
			Assert.Equal((short)plainDouble, (short)securedDouble);
			Assert.Equal((ushort)plainDouble, (ushort)securedDouble);
			Assert.Equal((int)plainDouble, (int)securedDouble);
			Assert.Equal((uint)plainDouble, (uint)securedDouble);
			Assert.Equal((long)plainDouble, (long)securedDouble);
			Assert.Equal((ulong)plainDouble, (ulong)securedDouble);
			Assert.Equal((char)plainDouble, (char)securedDouble);
			Assert.Equal((float)plainDouble, (float)securedDouble);
			Assert.Equal(plainDouble, (double)securedDouble);
			Assert.Equal((decimal)plainDouble, (decimal)securedDouble);

			decimal plainDecimal = 100m;
			SecureDecimal securedDecimal = plainDecimal;
			Assert.Equal((sbyte)plainDecimal, (sbyte)securedDecimal);
			Assert.Equal((byte)plainDecimal, (byte)securedDecimal);
			Assert.Equal((short)plainDecimal, (short)securedDecimal);
			Assert.Equal((ushort)plainDecimal, (ushort)securedDecimal);
			Assert.Equal((int)plainDecimal, (int)securedDecimal);
			Assert.Equal((uint)plainDecimal, (uint)securedDecimal);
			Assert.Equal((long)plainDecimal, (long)securedDecimal);
			Assert.Equal((ulong)plainDecimal, (ulong)securedDecimal);
			Assert.Equal((char)plainDecimal, (char)securedDecimal);
			Assert.Equal((float)plainDecimal, (float)securedDecimal);
			Assert.Equal((double)plainDecimal, (double)securedDecimal);
			Assert.Equal(plainDecimal, (decimal)securedDecimal);

			BigInteger plainBigInteger = new BigInteger(100);
			SecureBigInteger securedBigInteger = plainBigInteger;
			Assert.Equal((sbyte)plainBigInteger, (sbyte)securedBigInteger);
			Assert.Equal((byte)plainBigInteger, (byte)securedBigInteger);
			Assert.Equal((short)plainBigInteger, (short)securedBigInteger);
			Assert.Equal((ushort)plainBigInteger, (ushort)securedBigInteger);
			Assert.Equal((int)plainBigInteger, (int)securedBigInteger);
			Assert.Equal((uint)plainBigInteger, (uint)securedBigInteger);
			Assert.Equal((long)plainBigInteger, (long)securedBigInteger);
			Assert.Equal((ulong)plainBigInteger, (ulong)securedBigInteger);
			Assert.Equal((char)(int)plainBigInteger, (char)securedBigInteger);
			Assert.Equal((float)plainBigInteger, (float)securedBigInteger);
			Assert.Equal((double)plainBigInteger, (double)securedBigInteger);
			Assert.Equal((decimal)plainBigInteger, (decimal)securedBigInteger);
			Assert.Equal(plainBigInteger, (BigInteger)securedBigInteger);
		}

		[Fact]
		public void BigInteger_ExplicitFloatTwinsNeverThrowLikeBcl()
		{
			SecureBigInteger hugeFloat = BigInteger.Pow(2, 200);
			Assert.Equal((float)(BigInteger)hugeFloat, (float)(SecureFloat)hugeFloat);
			Assert.True(float.IsPositiveInfinity((float)(SecureFloat)hugeFloat));
			SecureBigInteger hugeDouble = BigInteger.Pow(2, 1100);
			Assert.Equal((double)(BigInteger)hugeDouble, (double)(SecureDouble)hugeDouble);
			Assert.True(double.IsPositiveInfinity((double)(SecureDouble)hugeDouble));
		}

		[Fact]
		public void BigInteger_ExplicitPlainTwinsMirrorBcl()
		{
			BigInteger plain = new BigInteger(100);
			SecureBigInteger secured = plain;
			Assert.Equal((sbyte)plain, (sbyte)secured);
			Assert.Equal((byte)plain, (byte)secured);
			Assert.Equal((short)plain, (short)secured);
			Assert.Equal((ushort)plain, (ushort)secured);
			Assert.Equal((int)plain, (int)secured);
			Assert.Equal((uint)plain, (uint)secured);
			Assert.Equal((long)plain, (long)secured);
			Assert.Equal((ulong)plain, (ulong)secured);
			Assert.Equal((char)100, (char)secured);
			Assert.Equal((float)plain, (float)secured);
			Assert.Equal((double)plain, (double)secured);
			Assert.Equal((decimal)plain, (decimal)secured);
			SecureBigInteger huge = BigInteger.Pow(2, 100);
			Assert.Throws<OverflowException>(() => (int)huge);
			Assert.Throws<OverflowException>(() => (decimal)huge);
			Assert.True(float.IsPositiveInfinity((float)(SecureBigInteger)BigInteger.Pow(2, 200)));
		}

		[Fact]
		public void SecureTwins_HaveMatchingPlainTwins()
		{
			// Every explicit SecureX -> SecureY twin must have a same-source
			// explicit SecureX -> plainY twin returning the same value.
			Type[] wrappers =
			[
				typeof(SecureSByte),
				typeof(SecureByte),
				typeof(SecureShort),
				typeof(SecureUShort),
				typeof(SecureInt),
				typeof(SecureUInt),
				typeof(SecureLong),
				typeof(SecureULong),
				typeof(SecureChar),
				typeof(SecureFloat),
				typeof(SecureDouble),
				typeof(SecureDecimal),
			];
			foreach (Type source in wrappers)
			{
				Type wrapped = source.GetProperty("Decrypted")!.PropertyType;
				object sample = Convert.ChangeType(100, wrapped);
				object secured = Activator.CreateInstance(source, sample)!;
				MethodInfo[] explicits = source
					.GetMethods(BindingFlags.Public | BindingFlags.Static)
					.Where(m => m.Name == "op_Explicit" && MatchesSource(m, source))
					.ToArray();
				Assert.NotEmpty(explicits);
				foreach (MethodInfo twin in explicits)
				{
					if (!IsSecureTwin(twin.ReturnType))
					{
						continue;
					}
					Type twinPlain = twin.ReturnType.GetProperty("Decrypted")!.PropertyType;
					MethodInfo? plainOp = explicits.FirstOrDefault(m => m.ReturnType == twinPlain);
					Assert.NotNull(plainOp);
					object twinValue = twin.Invoke(null, [secured])!;
					object expected = twinValue
						.GetType()
						.GetProperty("Decrypted")!
						.GetValue(twinValue)!;
					Assert.Equal(expected, plainOp.Invoke(null, [secured]));
					// Cast equality against the plain baseline: the secured twin
					// must equal the ordinary primitive built from the same sample,
					// and the secured twin must equal a directly built twin.
					// (Char has no IConvertible path to float/double, so route it
					// through int first — same value, supported conversion. The
					// mirror gap (float/double to char) routes through int too,
					// exactly like the twin body does.)
					object baselineSource = sample is char ch ? (object)(int)ch : sample;
					object baseline =
						twinPlain == typeof(char)
							? (object)(char)Convert.ToInt32(baselineSource)
							: Convert.ChangeType(baselineSource, twinPlain);
					Assert.Equal(baseline, expected);
					Assert.Equal(Activator.CreateInstance(twin.ReturnType, baseline), twinValue);
				}
			}
		}

		private static bool MatchesSource(MethodInfo method, Type source)
		{
			ParameterInfo[] parameters = method.GetParameters();
			return parameters.Length == 1 && parameters[0].ParameterType == source;
		}

		private static bool IsSecureTwin(Type type) =>
			type.Namespace == "SecureValue"
			&& type.Name.StartsWith("Secure", StringComparison.Ordinal);
	}
}
