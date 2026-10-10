#nullable enable
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using NUnit.Framework;
using SecureValue;
using SecureValue.Numerics;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit OperatorMatrixTests: minimal-operator contract
	/// (every wrapper owns O(P,P) + O(P,T_own) + O(T_own,P) plus int-literal
	/// interop twins), cross-Secure explicit conversion twins (Secure-to-Secure
	/// and Secure-to-plain, mirroring BCL), and BigInteger twin coverage.
	/// Same sources as the library build, so operator behavior is identical;
	/// only the harness differs (NUnit, C# 9).
	/// </summary>
	public class OperatorMatrixTests
	{
		[Test]
		public void Int_OwnAndIntTwinsBindLiterals()
		{
			SecureInt p = 1000;
			Assert.AreEqual(1007, (int)(p + 7));
			Assert.AreEqual(993, (int)(p - 7));
			Assert.AreEqual(7000, (int)(p * 7));
			Assert.AreEqual(142, (int)(p / 7));
			Assert.AreEqual(6, (int)(p % 7));
			Assert.IsTrue(p == 1000);
			Assert.IsTrue(p != 7);
			Assert.IsTrue(p > 7);
			Assert.IsTrue(p >= 1000);
			Assert.IsTrue(p < 1001);
			Assert.IsTrue(p <= 1000);
			Assert.AreEqual(1000 & 7, (int)(p & 7));
			Assert.AreEqual(1000 | 7, (int)(p | 7));
			Assert.AreEqual(1000 ^ 7, (int)(p ^ 7));
			Assert.AreEqual(1000 << 2, (int)(p << 2));
			Assert.AreEqual(1000 >> 2, (int)(p >> 2));
			p += 7;
			Assert.AreEqual(1007, (int)p);
			p -= 7;
			Assert.AreEqual(1000, (int)p);
			p *= 2;
			Assert.AreEqual(2000, (int)p);
			p /= 2;
			Assert.AreEqual(1000, (int)p);
			p %= 7;
			Assert.AreEqual(6, (int)p);
			p = 1000;
			p &= 7;
			Assert.AreEqual(1000 & 7, (int)p);
			p = 1000;
			p |= 7;
			Assert.AreEqual(1000 | 7, (int)p);
			p = 1000;
			p ^= 7;
			Assert.AreEqual(1000 ^ 7, (int)p);
		}

		[Test]
		public void ReverseOrder_BindsExactMirror()
		{
			SecureInt p = 1000;
			SecureLong big = 1000L;
			Assert.AreEqual(1007, (int)(7 + p));
			Assert.AreEqual(1007L, (long)(7L + big));
			Assert.IsTrue(7 < p);
			Assert.IsTrue(1000 == (int)p);
			SecureULong u = 100UL;
			Assert.IsTrue((decimal)u > 2.5m);
			Assert.IsTrue(2.5m < (decimal)u);
			Assert.AreEqual(102.5m, 2.5m + (decimal)u);
		}

		[Test]
		public void CrossSecure_NeedsExplicitCast()
		{
			// No (P1,P2) pairs: cross-Secure mixes do not resolve implicitly.
			// Spell the widening explicitly; the value math is unchanged.
			SecureLong bossHP = 1000L;
			SecureInt heal = 7;
			SecureLong healed = bossHP + new SecureLong(heal.Decrypted);
			Assert.AreEqual(1007L, (long)healed);
			bossHP += new SecureLong(heal.Decrypted);
			Assert.AreEqual(1007L, (long)bossHP);
			Assert.IsTrue(bossHP > new SecureLong(heal.Decrypted));
		}

		[Test]
		public void SmallTypes_IntLiteralsBindWithoutSuffix()
		{
			// sbyte/byte/short/ushort have no literal suffix; every literal
			// is int, so the (P,int) twin is what makes these compile.
			SecureSByte a = 100;
			Assert.IsInstanceOf<SecureInt>(a + 5);
			Assert.AreEqual(105, (int)(a + 5));
			SecureShort s = 100;
			Assert.AreEqual(95, (int)(s - 5));
			SecureByte b = 100;
			Assert.AreEqual(107, (int)(b + 7));
			SecureUShort us = 100;
			Assert.AreEqual(93, (int)(us - 7));
			// Typed sub-int variables still need a visible cast (no silent
			// bridge): short -> int is explicit at the call site.
			short sv = 7;
			Assert.AreEqual(107, (int)(a + (int)sv));
		}

		[Test]
		public void Widening_IntInteropReturnsWidenedSecure()
		{
			// Returns follow the BCL: never narrow silently.
			SecureLong a = 100L;
			Assert.IsInstanceOf<SecureLong>(a + 7);
			Assert.AreEqual(107L, (long)(a + 7));
			SecureUInt u = 4000000000U;
			Assert.IsInstanceOf<SecureLong>(u + 7);
			Assert.AreEqual(4000000007L, (long)(u + 7));
			SecureInt n = 100;
			Assert.IsInstanceOf<SecureInt>(n + 7);
			SecureSByte tiny = 100;
			Assert.IsInstanceOf<SecureInt>(tiny + 5);
		}

		[Test]
		public void ULong_SuffixedLiteralsBindOwnTwins()
		{
			SecureULong v = ulong.MaxValue;
			// BCL rejects ulong+int variables, so there is no (P,int) twin
			// here; the UL-suffixed literal binds (P,ulong) exactly.
			Assert.AreEqual(ulong.MaxValue - 1, (ulong)(v - 1UL));
			Assert.IsTrue(v == ulong.MaxValue);
			Assert.IsTrue(v != 1UL);
			Assert.IsTrue(v > 1UL);
			SecureUInt u = 4000000000U;
			Assert.AreEqual(4000000001UL, (ulong)(u + 1U));
		}

		[Test]
		public void CharTwin_BindsExplicitInt()
		{
			SecureInt n = 100;
			Assert.AreEqual(100 + 'A', (int)(n + (int)'A'));
		}

		[Test]
		public void Float_MixesWithIntBothOrders()
		{
			SecureFloat f = 100f;
			Assert.IsInstanceOf<SecureFloat>(2 + f);
			Assert.AreEqual(102f, (float)(2 + f));
			SecureDouble d = 100.0;
			Assert.IsInstanceOf<SecureDouble>(d + 2);
			Assert.AreEqual(102.0, (double)(d + 2));
			SecureDecimal m = 100m;
			Assert.IsInstanceOf<SecureDecimal>(m + 2);
			Assert.AreEqual(102m, (decimal)(m + 2));
		}

		[Test]
		public void Char_MixesWithIntLiterals()
		{
			SecureChar c = 'A';
			Assert.IsInstanceOf<SecureInt>(c + 1);
			Assert.AreEqual('A' + 1, (int)(c + 1));
			Assert.AreEqual('A' + 1, (int)(1 + c));
			Assert.IsTrue(c == 'A');
			Assert.IsTrue('A' == c);
			// Cross-Secure (char + SecureInt) is a mirror tie by design;
			// cast one side to the plain type.
			SecureInt n = 7;
			Assert.AreEqual('A' + 7, (int)(c + (int)n));
			Assert.AreEqual('A' + 7, (int)((int)n + c));
		}

		[Test]
		public void Bool_EqualityWithPrimitive()
		{
			SecureBool p = true;
			Assert.IsTrue(p == true);
			Assert.IsTrue(true == p);
			Assert.IsTrue(p != false);
			Assert.IsTrue(false != p);
		}

		[Test]
		public void String_ConcatAndEqualityWithPrimitive()
		{
			SecureString s = "foo";
			Assert.AreEqual("foobar", (string?)(s + "bar"));
			Assert.AreEqual("barfoo", (string?)("bar" + s));
			Assert.IsTrue(s == "foo");
			Assert.IsTrue("foo" == s);
			Assert.IsTrue(s != "bar");
			Assert.IsTrue(s != (string)null!);
		}

		[Test]
		public void DateTime_ArithmeticWithTimeSpan()
		{
			SecureDateTime dt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan ts = TimeSpan.FromDays(1);
			Assert.AreEqual(
				new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
				(DateTime)(dt + ts)
			);
			Assert.AreEqual(
				new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
				(DateTime)(dt - ts)
			);
			Assert.AreEqual(TimeSpan.FromDays(1), (TimeSpan)(dt + ts - dt));
			SecureTimeSpan span = TimeSpan.FromMinutes(90);
			Assert.AreEqual(TimeSpan.FromMinutes(180), (TimeSpan)(span * 2.0));
			Assert.AreEqual(TimeSpan.FromMinutes(180), (TimeSpan)(2.0 * span));
			Assert.AreEqual(TimeSpan.FromMinutes(45), (TimeSpan)(span / 2.0));
			Assert.AreEqual(2.0, (double)(span * 2.0 / span));
		}

		[Test]
		public void PrimitiveInterfaces_DispatchThroughInterface()
		{
			SecureInt p = 100;
			Assert.IsTrue(((IEquatable<int>)p).Equals(100));
			Assert.IsTrue(((IComparable<int>)p).CompareTo(99) > 0);
			SecureString s = "foo";
			Assert.IsTrue(((IEquatable<string>)s).Equals("foo"));
			SecureGuid g = Guid.Empty;
			Assert.IsTrue(((IEquatable<Guid>)g).Equals(Guid.Empty));
		}

		[Test]
		public void Convertible_ForwardsToDecrypted()
		{
			SecureInt p = 100;
			Assert.AreEqual(100, ((IConvertible)p).ToInt32(null));
			Assert.AreEqual(TypeCode.Int32, ((IConvertible)p).GetTypeCode());
		}

		[Test]
		public void SecureConversions_AreExplicitAndMirrorBcl()
		{
			SecureInt n = 100;
			Assert.AreEqual(100L, (long)(SecureLong)n);
			Assert.AreEqual(100.0, (double)(SecureDouble)n);
			SecureSByte small = 100;
			Assert.AreEqual(100, (int)(SecureInt)small);
			SecureChar ch = 'A';
			Assert.AreEqual((int)'A', (int)(SecureInt)ch);
			SecureLong big = 100L;
			Assert.AreEqual(100, (int)(SecureInt)big);
			SecureDouble dbl = 100.0;
			Assert.AreEqual(100, (int)(SecureInt)dbl);
		}

		[Test]
		public void BigInteger_ExplicitTwinsMirrorBcl()
		{
			BigInteger plain = new BigInteger(100);
			SecureBigInteger secured = plain;
			Assert.AreEqual((sbyte)plain, (sbyte)(SecureSByte)secured);
			Assert.AreEqual((byte)plain, (byte)(SecureByte)secured);
			Assert.AreEqual((short)plain, (short)(SecureShort)secured);
			Assert.AreEqual((ushort)plain, (ushort)(SecureUShort)secured);
			Assert.AreEqual((int)plain, (int)(SecureInt)secured);
			Assert.AreEqual((uint)plain, (uint)(SecureUInt)secured);
			Assert.AreEqual((long)plain, (long)(SecureLong)secured);
			Assert.AreEqual((ulong)plain, (ulong)(SecureULong)secured);
			Assert.AreEqual((char)100, (char)(SecureChar)secured);
			Assert.AreEqual((float)plain, (float)(SecureFloat)secured);
			Assert.AreEqual((double)plain, (double)(SecureDouble)secured);
			Assert.AreEqual((decimal)plain, (decimal)(SecureDecimal)secured);
		}

		[Test]
		public void BigInteger_ExplicitTwinsThrowOnOverflowLikeBcl()
		{
			SecureBigInteger huge = BigInteger.Pow(2, 100);
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureSByte)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureByte)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureShort)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureUShort)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureInt)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureUInt)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureLong)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureULong)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureDecimal)huge;
			});
			SecureBigInteger negative = new BigInteger(-1);
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureUInt)negative;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (SecureULong)negative;
			});
		}

		[Test]
		public void BigInteger_ExplicitFloatTwinsNeverThrowLikeBcl()
		{
			SecureBigInteger hugeFloat = BigInteger.Pow(2, 200);
			Assert.AreEqual((float)(BigInteger)hugeFloat, (float)(SecureFloat)hugeFloat);
			Assert.IsTrue(float.IsPositiveInfinity((float)(SecureFloat)hugeFloat));
			SecureBigInteger hugeDouble = BigInteger.Pow(2, 1100);
			Assert.AreEqual((double)(BigInteger)hugeDouble, (double)(SecureDouble)hugeDouble);
			Assert.IsTrue(double.IsPositiveInfinity((double)(SecureDouble)hugeDouble));
		}

		[Test]
		public void BigInteger_ExplicitPlainTwinsMirrorBcl()
		{
			BigInteger plain = new BigInteger(100);
			SecureBigInteger secured = plain;
			Assert.AreEqual((sbyte)plain, (sbyte)secured);
			Assert.AreEqual((byte)plain, (byte)secured);
			Assert.AreEqual((short)plain, (short)secured);
			Assert.AreEqual((ushort)plain, (ushort)secured);
			Assert.AreEqual((int)plain, (int)secured);
			Assert.AreEqual((uint)plain, (uint)secured);
			Assert.AreEqual((long)plain, (long)secured);
			Assert.AreEqual((ulong)plain, (ulong)secured);
			Assert.AreEqual((char)100, (char)secured);
			Assert.AreEqual((float)plain, (float)secured);
			Assert.AreEqual((double)plain, (double)secured);
			Assert.AreEqual((decimal)plain, (decimal)secured);
			SecureBigInteger huge = BigInteger.Pow(2, 100);
			Assert.Throws<OverflowException>(() =>
			{
				_ = (int)huge;
			});
			Assert.Throws<OverflowException>(() =>
			{
				_ = (decimal)huge;
			});
			Assert.IsTrue(
				float.IsPositiveInfinity((float)(SecureBigInteger)BigInteger.Pow(2, 200))
			);
		}

		[Test]
		public void SecureTwins_HaveMatchingPlainTwins()
		{
			// Every explicit SecureX -> SecureY twin must have a same-source
			// explicit SecureX -> plainY twin returning the same value, and both
			// must equal the ordinary primitive baseline for the same sample.
			Type[] wrappers = new Type[]
			{
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
			};
			foreach (Type source in wrappers)
			{
				Type wrapped = source.GetProperty("Decrypted")!.PropertyType;
				object sample = Convert.ChangeType(100, wrapped);
				object secured = Activator.CreateInstance(source, sample)!;
				MethodInfo[] explicits = source
					.GetMethods(BindingFlags.Public | BindingFlags.Static)
					.Where(m => m.Name == "op_Explicit" && MatchesSource(m, source))
					.ToArray();
				Assert.IsNotEmpty(explicits);
				foreach (MethodInfo twin in explicits)
				{
					if (!IsSecureTwin(twin.ReturnType))
					{
						continue;
					}
					Type twinPlain = twin.ReturnType.GetProperty("Decrypted")!.PropertyType;
					MethodInfo? plainOp = explicits.FirstOrDefault(m => m.ReturnType == twinPlain);
					Assert.IsNotNull(plainOp);
					object twinValue = twin.Invoke(null, new object[] { secured })!;
					object expected = twinValue
						.GetType()
						.GetProperty("Decrypted")!
						.GetValue(twinValue)!;
					Assert.AreEqual(expected, plainOp.Invoke(null, new object[] { secured }));
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
					Assert.AreEqual(baseline, expected);
					Assert.AreEqual(Activator.CreateInstance(twin.ReturnType, baseline), twinValue);
				}
			}
		}

		[Test]
		public void ExplicitCasts_PlainAndSecuredAreEqual()
		{
			sbyte plainSbyte = 100;
			SecureSByte securedSbyte = plainSbyte;
			Assert.AreEqual(plainSbyte, (sbyte)securedSbyte);
			Assert.AreEqual((byte)plainSbyte, (byte)securedSbyte);
			Assert.AreEqual((short)plainSbyte, (short)securedSbyte);
			Assert.AreEqual((ushort)plainSbyte, (ushort)securedSbyte);
			Assert.AreEqual((int)plainSbyte, (int)securedSbyte);
			Assert.AreEqual((uint)plainSbyte, (uint)securedSbyte);
			Assert.AreEqual((long)plainSbyte, (long)securedSbyte);
			Assert.AreEqual((ulong)plainSbyte, (ulong)securedSbyte);
			Assert.AreEqual((char)plainSbyte, (char)securedSbyte);
			Assert.AreEqual((float)plainSbyte, (float)securedSbyte);
			Assert.AreEqual((double)plainSbyte, (double)securedSbyte);
			Assert.AreEqual((decimal)plainSbyte, (decimal)securedSbyte);

			byte plainByte = 100;
			SecureByte securedByte = plainByte;
			Assert.AreEqual((sbyte)plainByte, (sbyte)securedByte);
			Assert.AreEqual(plainByte, (byte)securedByte);
			Assert.AreEqual((short)plainByte, (short)securedByte);
			Assert.AreEqual((ushort)plainByte, (ushort)securedByte);
			Assert.AreEqual((int)plainByte, (int)securedByte);
			Assert.AreEqual((uint)plainByte, (uint)securedByte);
			Assert.AreEqual((long)plainByte, (long)securedByte);
			Assert.AreEqual((ulong)plainByte, (ulong)securedByte);
			Assert.AreEqual((char)plainByte, (char)securedByte);
			Assert.AreEqual((float)plainByte, (float)securedByte);
			Assert.AreEqual((double)plainByte, (double)securedByte);
			Assert.AreEqual((decimal)plainByte, (decimal)securedByte);

			short plainShort = 100;
			SecureShort securedShort = plainShort;
			Assert.AreEqual((sbyte)plainShort, (sbyte)securedShort);
			Assert.AreEqual((byte)plainShort, (byte)securedShort);
			Assert.AreEqual(plainShort, (short)securedShort);
			Assert.AreEqual((ushort)plainShort, (ushort)securedShort);
			Assert.AreEqual((int)plainShort, (int)securedShort);
			Assert.AreEqual((uint)plainShort, (uint)securedShort);
			Assert.AreEqual((long)plainShort, (long)securedShort);
			Assert.AreEqual((ulong)plainShort, (ulong)securedShort);
			Assert.AreEqual((char)plainShort, (char)securedShort);
			Assert.AreEqual((float)plainShort, (float)securedShort);
			Assert.AreEqual((double)plainShort, (double)securedShort);
			Assert.AreEqual((decimal)plainShort, (decimal)securedShort);

			ushort plainUShort = 100;
			SecureUShort securedUShort = plainUShort;
			Assert.AreEqual((sbyte)plainUShort, (sbyte)securedUShort);
			Assert.AreEqual((byte)plainUShort, (byte)securedUShort);
			Assert.AreEqual((short)plainUShort, (short)securedUShort);
			Assert.AreEqual(plainUShort, (ushort)securedUShort);
			Assert.AreEqual((int)plainUShort, (int)securedUShort);
			Assert.AreEqual((uint)plainUShort, (uint)securedUShort);
			Assert.AreEqual((long)plainUShort, (long)securedUShort);
			Assert.AreEqual((ulong)plainUShort, (ulong)securedUShort);
			Assert.AreEqual((char)plainUShort, (char)securedUShort);
			Assert.AreEqual((float)plainUShort, (float)securedUShort);
			Assert.AreEqual((double)plainUShort, (double)securedUShort);
			Assert.AreEqual((decimal)plainUShort, (decimal)securedUShort);

			int plainInt = 100;
			SecureInt securedInt = plainInt;
			Assert.AreEqual((sbyte)plainInt, (sbyte)securedInt);
			Assert.AreEqual((byte)plainInt, (byte)securedInt);
			Assert.AreEqual((short)plainInt, (short)securedInt);
			Assert.AreEqual((ushort)plainInt, (ushort)securedInt);
			Assert.AreEqual(plainInt, (int)securedInt);
			Assert.AreEqual((uint)plainInt, (uint)securedInt);
			Assert.AreEqual((long)plainInt, (long)securedInt);
			Assert.AreEqual((ulong)plainInt, (ulong)securedInt);
			Assert.AreEqual((char)plainInt, (char)securedInt);
			Assert.AreEqual((float)plainInt, (float)securedInt);
			Assert.AreEqual((double)plainInt, (double)securedInt);
			Assert.AreEqual((decimal)plainInt, (decimal)securedInt);

			uint plainUInt = 100;
			SecureUInt securedUInt = plainUInt;
			Assert.AreEqual((sbyte)plainUInt, (sbyte)securedUInt);
			Assert.AreEqual((byte)plainUInt, (byte)securedUInt);
			Assert.AreEqual((short)plainUInt, (short)securedUInt);
			Assert.AreEqual((ushort)plainUInt, (ushort)securedUInt);
			Assert.AreEqual((int)plainUInt, (int)securedUInt);
			Assert.AreEqual(plainUInt, (uint)securedUInt);
			Assert.AreEqual((long)plainUInt, (long)securedUInt);
			Assert.AreEqual((ulong)plainUInt, (ulong)securedUInt);
			Assert.AreEqual((char)plainUInt, (char)securedUInt);
			Assert.AreEqual((float)plainUInt, (float)securedUInt);
			Assert.AreEqual((double)plainUInt, (double)securedUInt);
			Assert.AreEqual((decimal)plainUInt, (decimal)securedUInt);

			long plainLong = 100;
			SecureLong securedLong = plainLong;
			Assert.AreEqual((sbyte)plainLong, (sbyte)securedLong);
			Assert.AreEqual((byte)plainLong, (byte)securedLong);
			Assert.AreEqual((short)plainLong, (short)securedLong);
			Assert.AreEqual((ushort)plainLong, (ushort)securedLong);
			Assert.AreEqual((int)plainLong, (int)securedLong);
			Assert.AreEqual((uint)plainLong, (uint)securedLong);
			Assert.AreEqual(plainLong, (long)securedLong);
			Assert.AreEqual((ulong)plainLong, (ulong)securedLong);
			Assert.AreEqual((char)plainLong, (char)securedLong);
			Assert.AreEqual((float)plainLong, (float)securedLong);
			Assert.AreEqual((double)plainLong, (double)securedLong);
			Assert.AreEqual((decimal)plainLong, (decimal)securedLong);

			ulong plainULong = 100;
			SecureULong securedULong = plainULong;
			Assert.AreEqual((sbyte)plainULong, (sbyte)securedULong);
			Assert.AreEqual((byte)plainULong, (byte)securedULong);
			Assert.AreEqual((short)plainULong, (short)securedULong);
			Assert.AreEqual((ushort)plainULong, (ushort)securedULong);
			Assert.AreEqual((int)plainULong, (int)securedULong);
			Assert.AreEqual((uint)plainULong, (uint)securedULong);
			Assert.AreEqual((long)plainULong, (long)securedULong);
			Assert.AreEqual(plainULong, (ulong)securedULong);
			Assert.AreEqual((char)plainULong, (char)securedULong);
			Assert.AreEqual((float)plainULong, (float)securedULong);
			Assert.AreEqual((double)plainULong, (double)securedULong);
			Assert.AreEqual((decimal)plainULong, (decimal)securedULong);

			char plainChar = 'd';
			SecureChar securedChar = plainChar;
			Assert.AreEqual((sbyte)plainChar, (sbyte)securedChar);
			Assert.AreEqual((byte)plainChar, (byte)securedChar);
			Assert.AreEqual((short)plainChar, (short)securedChar);
			Assert.AreEqual((ushort)plainChar, (ushort)securedChar);
			Assert.AreEqual((int)plainChar, (int)securedChar);
			Assert.AreEqual((uint)plainChar, (uint)securedChar);
			Assert.AreEqual((long)plainChar, (long)securedChar);
			Assert.AreEqual((ulong)plainChar, (ulong)securedChar);
			Assert.AreEqual(plainChar, (char)securedChar);
			Assert.AreEqual((float)plainChar, (float)securedChar);
			Assert.AreEqual((double)plainChar, (double)securedChar);
			Assert.AreEqual((decimal)plainChar, (decimal)securedChar);

			float plainFloat = 100f;
			SecureFloat securedFloat = plainFloat;
			Assert.AreEqual((sbyte)plainFloat, (sbyte)securedFloat);
			Assert.AreEqual((byte)plainFloat, (byte)securedFloat);
			Assert.AreEqual((short)plainFloat, (short)securedFloat);
			Assert.AreEqual((ushort)plainFloat, (ushort)securedFloat);
			Assert.AreEqual((int)plainFloat, (int)securedFloat);
			Assert.AreEqual((uint)plainFloat, (uint)securedFloat);
			Assert.AreEqual((long)plainFloat, (long)securedFloat);
			Assert.AreEqual((ulong)plainFloat, (ulong)securedFloat);
			Assert.AreEqual((char)plainFloat, (char)securedFloat);
			Assert.AreEqual(plainFloat, (float)securedFloat);
			Assert.AreEqual((double)plainFloat, (double)securedFloat);
			Assert.AreEqual((decimal)plainFloat, (decimal)securedFloat);

			double plainDouble = 100.0;
			SecureDouble securedDouble = plainDouble;
			Assert.AreEqual((sbyte)plainDouble, (sbyte)securedDouble);
			Assert.AreEqual((byte)plainDouble, (byte)securedDouble);
			Assert.AreEqual((short)plainDouble, (short)securedDouble);
			Assert.AreEqual((ushort)plainDouble, (ushort)securedDouble);
			Assert.AreEqual((int)plainDouble, (int)securedDouble);
			Assert.AreEqual((uint)plainDouble, (uint)securedDouble);
			Assert.AreEqual((long)plainDouble, (long)securedDouble);
			Assert.AreEqual((ulong)plainDouble, (ulong)securedDouble);
			Assert.AreEqual((char)plainDouble, (char)securedDouble);
			Assert.AreEqual((float)plainDouble, (float)securedDouble);
			Assert.AreEqual(plainDouble, (double)securedDouble);
			Assert.AreEqual((decimal)plainDouble, (decimal)securedDouble);

			decimal plainDecimal = 100m;
			SecureDecimal securedDecimal = plainDecimal;
			Assert.AreEqual((sbyte)plainDecimal, (sbyte)securedDecimal);
			Assert.AreEqual((byte)plainDecimal, (byte)securedDecimal);
			Assert.AreEqual((short)plainDecimal, (short)securedDecimal);
			Assert.AreEqual((ushort)plainDecimal, (ushort)securedDecimal);
			Assert.AreEqual((int)plainDecimal, (int)securedDecimal);
			Assert.AreEqual((uint)plainDecimal, (uint)securedDecimal);
			Assert.AreEqual((long)plainDecimal, (long)securedDecimal);
			Assert.AreEqual((ulong)plainDecimal, (ulong)securedDecimal);
			Assert.AreEqual((char)plainDecimal, (char)securedDecimal);
			Assert.AreEqual((float)plainDecimal, (float)securedDecimal);
			Assert.AreEqual((double)plainDecimal, (double)securedDecimal);
			Assert.AreEqual(plainDecimal, (decimal)securedDecimal);

			BigInteger plainBigInteger = new BigInteger(100);
			SecureBigInteger securedBigInteger = plainBigInteger;
			Assert.AreEqual((sbyte)plainBigInteger, (sbyte)securedBigInteger);
			Assert.AreEqual((byte)plainBigInteger, (byte)securedBigInteger);
			Assert.AreEqual((short)plainBigInteger, (short)securedBigInteger);
			Assert.AreEqual((ushort)plainBigInteger, (ushort)securedBigInteger);
			Assert.AreEqual((int)plainBigInteger, (int)securedBigInteger);
			Assert.AreEqual((uint)plainBigInteger, (uint)securedBigInteger);
			Assert.AreEqual((long)plainBigInteger, (long)securedBigInteger);
			Assert.AreEqual((ulong)plainBigInteger, (ulong)securedBigInteger);
			Assert.AreEqual((char)(int)plainBigInteger, (char)securedBigInteger);
			Assert.AreEqual((float)plainBigInteger, (float)securedBigInteger);
			Assert.AreEqual((double)plainBigInteger, (double)securedBigInteger);
			Assert.AreEqual((decimal)plainBigInteger, (decimal)securedBigInteger);
			Assert.AreEqual(plainBigInteger, (BigInteger)securedBigInteger);
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
#endif
