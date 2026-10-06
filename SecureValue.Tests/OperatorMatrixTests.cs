#nullable enable
using SecureValue;
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
			Assert.Equal("foobar", (string)(s + "bar"));
			Assert.Equal("barfoo", (string)("bar" + s));
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
	}
}
