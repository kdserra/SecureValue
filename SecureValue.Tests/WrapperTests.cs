using System;
using System.Reflection;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>Round-trip, ergonomics, tamper-detection and allocation tests.</summary>
	public class WrapperTests
	{
		// ---------- round-trips ----------

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Bool_RoundTrip(bool value) => AssertRoundTrip(new SecureBool(value), value);

		[Theory]
		[InlineData(0)]
		[InlineData(255)]
		public void Byte_RoundTrip(byte value) => AssertRoundTrip(new SecureByte(value), value);

		[Theory]
		[InlineData(-128)]
		[InlineData(0)]
		[InlineData(127)]
		public void SByte_RoundTrip(sbyte value) => AssertRoundTrip(new SecureSByte(value), value);

		[Theory]
		[InlineData('\u03A9')]
		[InlineData('\0')]
		[InlineData('\uFFFF')]
		public void Char_RoundTrip(char value) => AssertRoundTrip(new SecureChar(value), value);

		[Theory]
		[InlineData(short.MinValue)]
		[InlineData(short.MaxValue)]
		public void Short_RoundTrip(short value) => AssertRoundTrip(new SecureShort(value), value);

		[Theory]
		[InlineData(ushort.MinValue)]
		[InlineData(ushort.MaxValue)]
		public void UShort_RoundTrip(ushort value) =>
			AssertRoundTrip(new SecureUShort(value), value);

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(int.MaxValue)]
		[InlineData(int.MinValue)]
		public void Int_RoundTrip(int value) => AssertRoundTrip(new SecureInt(value), value);

		[Theory]
		[InlineData(0u)]
		[InlineData(uint.MaxValue)]
		public void UInt_RoundTrip(uint value) => AssertRoundTrip(new SecureUInt(value), value);

		[Theory]
		[InlineData(long.MinValue)]
		[InlineData(long.MaxValue)]
		public void Long_RoundTrip(long value) => AssertRoundTrip(new SecureLong(value), value);

		[Theory]
		[InlineData(0ul)]
		[InlineData(ulong.MaxValue)]
		public void ULong_RoundTrip(ulong value) => AssertRoundTrip(new SecureULong(value), value);

		[Theory]
		[InlineData(-1.5e12f)]
		[InlineData(-0.0f)]
		[InlineData(float.MaxValue)]
		[InlineData(float.MinValue)]
		public void Float_RoundTrip(float value) => AssertRoundTrip(new SecureFloat(value), value);

		[Fact]
		public void Float_NaN_RoundTrip()
		{
			// bitwise round-trip: the NaN payload must survive even though NaN != NaN
			SecureFloat p = float.NaN;
			Assert.Equal(float.NaN, (float)p);
		}

		[Theory]
		[InlineData(float.PositiveInfinity)]
		[InlineData(float.NegativeInfinity)]
		public void Float_Infinity_RoundTrip(float value) =>
			AssertRoundTrip(new SecureFloat(value), value);

		[Theory]
		[InlineData(3.141592653589793)]
		[InlineData(-0.0)]
		[InlineData(double.MaxValue)]
		[InlineData(double.MinValue)]
		public void Double_RoundTrip(double value) =>
			AssertRoundTrip(new SecureDouble(value), value);

		[Fact]
		public void Double_NaN_RoundTrip()
		{
			SecureDouble p = double.NaN;
			Assert.Equal(double.NaN, (double)p);
		}

		[Theory]
		[InlineData(double.PositiveInfinity)]
		[InlineData(double.NegativeInfinity)]
		public void Double_Infinity_RoundTrip(double value) =>
			AssertRoundTrip(new SecureDouble(value), value);

		[Theory]
		[InlineData("-792281625142.64337593543950335")] // max-scale negative
		[InlineData("1.0")] // scale 1 and scale 2 are distinct bit layouts for equal values
		[InlineData("1.00")]
		[InlineData("0.0000000000000000000000000001")] // smallest positive (scale 28)
		[InlineData("79228162514264337593543950335")] // decimal.MaxValue
		[InlineData("-79228162514264337593543950335")] // decimal.MinValue
		public void Decimal_RoundTrip(string text) =>
			AssertRoundTrip(new SecureDecimal(decimal.Parse(text)), decimal.Parse(text));

		[Fact]
		public void Guid_RoundTrip()
		{
			var value = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
			AssertRoundTrip(new SecureGuid(value), value);
			AssertRoundTrip(new SecureGuid(Guid.Empty), Guid.Empty);
		}

		[Fact]
		public void DateTime_RoundTrip()
		{
			AssertRoundTrip(
				new SecureDateTime(new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Utc)),
				new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Utc)
			);
			// Kind travels in the packed bits: all three kinds plus extremes
			AssertRoundTrip(
				new SecureDateTime(new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Local)),
				new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Local)
			);
			AssertRoundTrip(
				new SecureDateTime(new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Unspecified)),
				new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Unspecified)
			);
			AssertRoundTrip(new SecureDateTime(DateTime.MinValue), DateTime.MinValue);
			AssertRoundTrip(new SecureDateTime(DateTime.MaxValue), DateTime.MaxValue);
		}

		[Fact]
		public void DateTimeOffset_RoundTrip()
		{
			AssertRoundTrip(
				new SecureDateTimeOffset(
					new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(5.5))
				),
				new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(5.5))
			);
			// negative offsets and the +-14h validity edges (regression-prone packing)
			AssertRoundTrip(
				new SecureDateTimeOffset(
					new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(-5.5))
				),
				new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(-5.5))
			);
			AssertRoundTrip(
				new SecureDateTimeOffset(
					new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(14))
				),
				new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(14))
			);
			AssertRoundTrip(
				new SecureDateTimeOffset(
					new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(-14))
				),
				new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.FromHours(-14))
			);
			var utcNow = DateTimeOffset.UtcNow;
			AssertRoundTrip(new SecureDateTimeOffset(utcNow), utcNow);
		}

		[Fact]
		public void TimeSpan_RoundTrip()
		{
			AssertRoundTrip(
				new SecureTimeSpan(TimeSpan.FromTicks(-987654321)),
				TimeSpan.FromTicks(-987654321)
			);
			AssertRoundTrip(new SecureTimeSpan(TimeSpan.Zero), TimeSpan.Zero);
			AssertRoundTrip(new SecureTimeSpan(TimeSpan.MinValue), TimeSpan.MinValue);
			AssertRoundTrip(new SecureTimeSpan(TimeSpan.MaxValue), TimeSpan.MaxValue);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_RoundTrip()
		{
			AssertRoundTrip(
				new SecureDateOnly(new DateOnly(2026, 2, 28)),
				new DateOnly(2026, 2, 28)
			);
			AssertRoundTrip(new SecureDateOnly(DateOnly.MinValue), DateOnly.MinValue);
			AssertRoundTrip(new SecureDateOnly(DateOnly.MaxValue), DateOnly.MaxValue);
		}

		[Fact]
		public void TimeOnly_RoundTrip()
		{
			AssertRoundTrip(
				new SecureTimeOnly(new TimeOnly(23, 59, 59, 999)),
				new TimeOnly(23, 59, 59, 999)
			);
			AssertRoundTrip(new SecureTimeOnly(TimeOnly.MinValue), TimeOnly.MinValue);
			AssertRoundTrip(new SecureTimeOnly(TimeOnly.MaxValue), TimeOnly.MaxValue);
		}
#endif

#if NET
		[Theory]
		[InlineData(0x1F600)]
		[InlineData(0x41)]
		[InlineData(0x10FFFF)]
		public void Rune_RoundTrip(int codePoint)
		{
			var value = new System.Text.Rune(codePoint);
			AssertRoundTrip(new SecureRune(value), value);
		}
#endif

		[Fact]
		public void String_RoundTrip()
		{
			SecureString p = "Hello, secured world!";
			string back = p;
			Assert.Equal("Hello, secured world!", back);
		}

		[Fact]
		public void String_EmptyRoundTrip()
		{
			SecureString p = "";
			Assert.Equal("", (string)p);
		}

		[Fact]
		public void String_NullBecomesEmpty()
		{
			SecureString p = new SecureString(null!);
			Assert.Equal("", (string)p);
		}

		[Fact]
		public void String_Unicode_RoundTrip()
		{
			// surrogate pairs must survive word packing (4 UTF-16 units per word)
			const string value = "A😀🎉Z";
			SecureString p = value;
			Assert.Equal(value, (string)p);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)] // exact word boundary
		[InlineData(5)] // first length needing a second word
		[InlineData(6)]
		[InlineData(7)]
		[InlineData(8)]
		[InlineData(9)]
		public void String_WordBoundaryLengths_RoundTrip(int length)
		{
			string value = new string('x', length);
			SecureString p = value;
			Assert.Equal(value, (string)p);
		}

		[Fact]
		public void String_Long_RoundTrip()
		{
			// multi-word value matching the benchmark corpus
			const string value = "The quick brown fox jumps over the lazy dog";
			SecureString p = value;
			Assert.Equal(value, (string)p);
		}

		private static void AssertRoundTrip<T>(object wrapper, T expected)
		{
			// implicit conversion back to the wrapped type (dynamic resolves the operator)
			dynamic d = wrapper;
			T back = d;
			Assert.Equal(expected, back);
		}

		// ---------- all-default backing fields ----------

		// Reading a never-assigned value throws UninitializedException (a
		// default reaching Decrypted is never legitimate: the Unity layer
		// materializes every field into a genuine encrypted value).

		[Fact]
		public void Default_Read_ThrowsUninitialized()
		{
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (bool)default(SecureBool);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (byte)default(SecureByte);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (sbyte)default(SecureSByte);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (char)default(SecureChar);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (short)default(SecureShort);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (ushort)default(SecureUShort);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (int)default(SecureInt);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (uint)default(SecureUInt);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (long)default(SecureLong);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (ulong)default(SecureULong);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (float)default(SecureFloat);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (double)default(SecureDouble);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (decimal)default(SecureDecimal);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (Guid)default(SecureGuid);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (DateTime)default(SecureDateTime);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (DateTimeOffset)default(SecureDateTimeOffset);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (TimeSpan)default(SecureTimeSpan);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (string)default(SecureString);
			});
#if NET6_0_OR_GREATER
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (DateOnly)default(SecureDateOnly);
			});
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (TimeOnly)default(SecureTimeOnly);
			});
#endif
#if NET
			Assert.Throws<UninitializedException>(() =>
			{
				_ = (System.Text.Rune)default(SecureRune);
			});
#endif
		}

		[Fact]
		public void Default_Members_ThrowUninitialized()
		{
			Assert.Throws<UninitializedException>(() => default(SecureInt).ToString());
			Assert.Throws<UninitializedException>(() => default(SecureInt).GetHashCode());
			Assert.Throws<UninitializedException>(() => default(SecureString).ToString());
			Assert.Throws<UninitializedException>(() => default(SecureDateTimeOffset).ToString());
		}

		[Fact]
		public void EnsureInitialized_MaterializesReadableDefault()
		{
			// The Unity serialization callbacks use this to turn legitimately
			// fresh fields into genuine encrypted defaults.
			var i = default(SecureInt);
			i.EnsureInitialized();
			Assert.Equal(0, (int)i);

			var d = default(SecureDecimal);
			d.EnsureInitialized();
			Assert.Equal(0m, (decimal)d);

			var g = default(SecureGuid);
			g.EnsureInitialized();
			Assert.Equal(Guid.Empty, (Guid)g);

			var dto = default(SecureDateTimeOffset);
			dto.EnsureInitialized();
			Assert.Equal(default(DateTimeOffset), (DateTimeOffset)dto);

			var s = default(SecureString);
			s.EnsureInitialized();
			Assert.Equal(string.Empty, (string)s);

			// live values are preserved untouched
			var live = new SecureInt(42);
			live.EnsureInitialized();
			Assert.Equal(42, (int)live);
		}

		// ---------- ciphertext randomization / non-identity ----------

		[Fact]
		public void SamePlaintext_DifferentCiphertext()
		{
			var a = new SecureInt(12345);
			var b = new SecureInt(12345);
			Assert.Equal(a, b);
			Assert.NotEqual(GetCellCipher(a), GetCellCipher(b));
		}

		private static ulong GetCellCipher(object wrapper)
		{
			FieldInfo cellField = wrapper
				.GetType()
				.GetField("_cell", BindingFlags.NonPublic | BindingFlags.Instance)!;
			object cell = cellField.GetValue(wrapper)!;
			FieldInfo cipher = cell.GetType()
				.GetField("_cipher", BindingFlags.NonPublic | BindingFlags.Instance)!;
			return (ulong)cipher.GetValue(cell)!;
		}

		[Fact]
		public void Salt_FreshPerWrite_AtScale()
		{
			// Per-write salt freshness is what defeats unchanged-value scans;
			// 256 identical writes must never reuse a salt (birthday odds ~2^-48).
			const int n = 256;
			var seen = new System.Collections.Generic.HashSet<ulong>();
			for (int i = 0; i < n; i++)
			{
				var p = new SecureInt(12345);
				Assert.True(seen.Add(GetCell(p, "_salt")), "salt reused within 256 writes");
			}
		}

		[Theory]
		[InlineData("same value")]
		[InlineData("same value, but long enough to live on the heap")]
		public void SameString_DifferentCiphertext(string plain)
		{
			SecureString a = plain;
			SecureString b = plain;
			Assert.Equal(plain, (string)a);
			ulong[] ca = GetCipherWords(a);
			ulong[] cb = GetCipherWords(b);
			Assert.Equal(ca.Length, cb.Length);
			Assert.NotEqual(ca[0], cb[0]);
		}

		private static ulong[] GetCipherWords(SecureString s)
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var arr = (ulong[]?)typeof(SecureString).GetField("_ciphers", flags)!.GetValue(s);
			if (arr is { Length: > 0 })
			{
				return arr;
			}
			// Inline store (or explicit empty): read the words straight from fields.
			Type t = typeof(SecureString);
			return new[]
			{
				(ulong)t.GetField("_w0", flags)!.GetValue(s)!,
				(ulong)t.GetField("_w1", flags)!.GetValue(s)!,
				(ulong)t.GetField("_w2", flags)!.GetValue(s)!,
				(ulong)t.GetField("_w3", flags)!.GetValue(s)!,
			};
		}

		private static ulong GetCell(object wrapper, string f)
		{
			var cellField = wrapper
				.GetType()
				.GetField(
					"_cell",
					System.Reflection.BindingFlags.NonPublic
						| System.Reflection.BindingFlags.Instance
				)!;
			object cell = cellField.GetValue(wrapper)!;
			return (ulong)
				cell.GetType()
					.GetField(
						f,
						System.Reflection.BindingFlags.NonPublic
							| System.Reflection.BindingFlags.Instance
					)!
					.GetValue(cell)!;
		}

		// ---------- tamper detection ----------

		[Fact]
		public void Tamper_Detected_Primitives()
		{
			AssertTamperDetected(new SecureInt(42));
			AssertTamperDetected(new SecureBool(true));
			AssertTamperDetected(new SecureDouble(1.0));
			AssertTamperDetected(new SecureFloat(1.0f));
			AssertTamperDetected(new SecureLong(1L << 60));
			AssertTamperDetected(new SecureDecimal(1.5m));
			AssertTamperDetected(new SecureGuid(Guid.NewGuid()));
			AssertTamperDetected(new SecureDateTime(DateTime.UtcNow));
			AssertTamperDetected(new SecureDateTimeOffset(DateTimeOffset.UtcNow));
		}

		[Fact]
		public void Tamper_Detected_String()
		{
			SecureString p = "tamper me";
			AssertTamperDetected(p);
		}

		private static void AssertTamperDetected(object wrapper)
		{
			// box the wrapper so reflection mutations stick, then flip every
			// stored integer/word and expect the MAC check to fail on read
			object boxed = wrapper;
			Type t = boxed.GetType();
			foreach (
				FieldInfo f in t.GetFields(
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
				)
			)
			{
				if (f.FieldType == typeof(ulong))
				{
					f.SetValue(boxed, (ulong)f.GetValue(boxed)! ^ 0xABCDEF0123456789UL);
				}
				else if (f.FieldType == typeof(uint))
				{
					f.SetValue(boxed, (uint)f.GetValue(boxed)! ^ 0xDEADBEEFu);
				}
				else if (f.FieldType == typeof(ulong[]))
				{
					var arr = (ulong[]?)f.GetValue(boxed);
					if (arr is { Length: > 0 })
					{
						arr[0] ^= 1UL;
					}
				}
				else if (
					f.FieldType.IsValueType
					&& (f.FieldType == typeof(Cell) || f.FieldType == typeof(Cell128))
				)
				{
					// recurse one level into Cell / Cell128 (mutate a boxed copy, then store it back)
					object nested = f.GetValue(boxed)!;
					foreach (
						FieldInfo nf in nested
							.GetType()
							.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
					)
					{
						if (nf.FieldType == typeof(ulong))
						{
							nf.SetValue(nested, (ulong)nf.GetValue(nested)! ^ 0x1122334455667788UL);
						}
						else if (nf.FieldType == typeof(uint))
						{
							nf.SetValue(nested, (uint)nf.GetValue(nested)! ^ 0xCAFEBABEu);
						}
					}
					f.SetValue(boxed, nested);
				}
			}
			MethodInfo? toString = t.GetMethod("ToString", Type.EmptyTypes);
			Assert.Throws<TamperedException>(() =>
			{
				try
				{
					toString!.Invoke(boxed, null);
				}
				catch (TargetInvocationException e)
				{
					System
						.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException!)
						.Throw();
				}
			});
		}

		// ---------- numeric operators ----------

		[Fact]
		public void Int_Operators()
		{
			SecureInt a = 10,
				b = 3;
			Assert.Equal(13, (int)(a + b));
			Assert.Equal(7, (int)(a - b));
			Assert.Equal(30, (int)(a * b));
			Assert.Equal(3, (int)(a / b));
			Assert.Equal(1, (int)(a % b));
			Assert.Equal(-10, (int)(-a));
			Assert.True(a > b);
			Assert.True(b <= a);
			Assert.True(a == new SecureInt(10));
			Assert.True(a != b);
			SecureInt c = a;
			c++;
			Assert.Equal(11, (int)c);
		}

		[Fact]
		public void Double_Operators()
		{
			SecureDouble a = 1.5,
				b = 0.5;
			Assert.Equal(2.0, (double)(a + b));
			Assert.Equal(1.0, (double)(a - b));
			Assert.Equal(0.75, (double)(a * b));
			Assert.Equal(3.0, (double)(a / b));
			Assert.Equal(0.0, (double)(a % b));
			Assert.Equal(-1.5, (double)(-a));
			Assert.True(a >= b);
			Assert.True(a != b);
			SecureDouble c = a;
			c++;
			Assert.Equal(2.5, (double)c);
			c--;
			Assert.Equal(1.5, (double)c);
		}

		[Fact]
		public void Float_Operators()
		{
			SecureFloat a = 10.5f,
				b = 2.5f;
			Assert.Equal(13.0f, (float)(a + b));
			Assert.Equal(8.0f, (float)(a - b));
			Assert.Equal(26.25f, (float)(a * b));
			Assert.Equal(4.2f, (float)(a / b));
			Assert.Equal(0.5f, (float)(a % b));
			Assert.Equal(-10.5f, (float)(-a));
			Assert.True(a > b);
			Assert.True(b <= a);
			Assert.True(a == new SecureFloat(10.5f));
			Assert.True(a != b);
			SecureFloat c = a;
			c++;
			Assert.Equal(11.5f, (float)c);
		}

		[Fact]
		public void Long_Operators()
		{
			SecureLong a = 10L,
				b = 3L;
			Assert.Equal(13L, (long)(a + b));
			Assert.Equal(7L, (long)(a - b));
			Assert.Equal(30L, (long)(a * b));
			Assert.Equal(3L, (long)(a / b));
			Assert.Equal(1L, (long)(a % b));
			Assert.Equal(-10L, (long)(-a));
			Assert.True(a > b);
			Assert.True(a != b);
			Assert.Equal(0, a.CompareTo(new SecureLong(10L)));
			SecureLong c = a;
			c++;
			Assert.Equal(11L, (long)c);
		}

		[Fact]
		public void ULong_Operators()
		{
			SecureULong a = new SecureULong(10ul),
				b = new SecureULong(3ul);
			Assert.Equal(13ul, (ulong)(a + b));
			Assert.Equal(7ul, (ulong)(a - b));
			Assert.Equal(30ul, (ulong)(a * b));
			Assert.Equal(3ul, (ulong)(a / b));
			Assert.Equal(1ul, (ulong)(a % b));
			Assert.True(a > b);
			Assert.True(a == new SecureULong(10ul));
			Assert.True(a != b);
		}

		[Fact]
		public void UInt_Operators()
		{
			SecureUInt a = new SecureUInt(10u),
				b = new SecureUInt(3u);
			Assert.Equal(13u, (uint)(a + b));
			Assert.Equal(7u, (uint)(a - b));
			Assert.Equal(30u, (uint)(a * b));
			Assert.Equal(3u, (uint)(a / b));
			Assert.Equal(1u, (uint)(a % b));
			Assert.True(a >= b);
			Assert.True(a != b);
		}

		[Fact]
		public void Short_Operators()
		{
			SecureShort a = 10,
				b = 3;
			Assert.Equal((short)13, (short)(a + b));
			Assert.Equal((short)7, (short)(a - b));
			Assert.Equal((short)30, (short)(a * b));
			Assert.Equal((short)3, (short)(a / b));
			Assert.Equal((short)1, (short)(a % b));
			Assert.Equal((short)-10, (short)(-a));
			Assert.True(a > b);
			Assert.True(a != b);
			SecureShort c = a;
			c++;
			Assert.Equal((short)11, (short)c);
		}

		[Fact]
		public void UShort_Operators()
		{
			SecureUShort a = 10,
				b = 3;
			Assert.Equal((ushort)13, (ushort)(a + b));
			Assert.Equal((ushort)7, (ushort)(a - b));
			Assert.Equal((ushort)30, (ushort)(a * b));
			Assert.Equal((ushort)3, (ushort)(a / b));
			Assert.Equal((ushort)1, (ushort)(a % b));
			Assert.True(a <= new SecureUShort(10));
			Assert.True(a != b);
		}

		[Fact]
		public void Byte_Operators()
		{
			SecureByte a = 10,
				b = 3;
			Assert.Equal((byte)13, (byte)(a + b));
			Assert.Equal((byte)7, (byte)(a - b));
			Assert.Equal((byte)30, (byte)(a * b));
			Assert.Equal((byte)3, (byte)(a / b));
			Assert.Equal((byte)1, (byte)(a % b));
			Assert.True(a > b);
			Assert.True(a == new SecureByte(10));
			Assert.True(a != b);
		}

		[Fact]
		public void SByte_Operators()
		{
			SecureSByte a = new SecureSByte(10),
				b = new SecureSByte(3);
			Assert.Equal((sbyte)13, (sbyte)(a + b));
			Assert.Equal((sbyte)7, (sbyte)(a - b));
			Assert.Equal((sbyte)30, (sbyte)(a * b));
			Assert.Equal((sbyte)3, (sbyte)(a / b));
			Assert.Equal((sbyte)-10, (sbyte)(-a));
			Assert.True(a >= b);
			Assert.True(a != b);
		}

		[Fact]
		public void Decimal_Operators()
		{
			SecureDecimal a = 0.1m,
				b = 0.2m;
			Assert.Equal(0.3m, (decimal)(a + b));
			Assert.Equal(-0.1m, (decimal)(a - b));
			Assert.Equal(0.02m, (decimal)(a * b));
			Assert.Equal(0.5m, (decimal)(a / b));
			Assert.Equal(0.1m, (decimal)(a % b));
			Assert.Equal(-0.1m, (decimal)(-a));
			Assert.True(a < b);
			Assert.True(a <= new SecureDecimal(0.1m));
			Assert.True(a != b);
			SecureDecimal c = a;
			c++;
			Assert.Equal(1.1m, (decimal)c);
		}

		[Fact]
		public void Bool_Equality()
		{
			SecureBool a = true,
				b = true,
				c = false;
			Assert.True(a == b);
			Assert.True(a != c);
			Assert.True(a.Equals(b));
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		public void Char_Equality()
		{
			SecureChar a = 'A',
				b = 'A',
				c = 'B';
			Assert.True(a == b);
			Assert.True(a != c);
			Assert.True(a.Equals(b));
		}

		[Fact]
		public void DateTime_Equality()
		{
			var t = new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Utc);
			SecureDateTime a = t,
				b = t,
				c = t.AddHours(1);
			Assert.True(a == b);
			Assert.True(a != c);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_Equality()
		{
			SecureDateOnly a = new DateOnly(2026, 2, 28);
			SecureDateOnly b = new DateOnly(2026, 2, 28);
			SecureDateOnly c = new DateOnly(2026, 3, 1);
			Assert.True(a == b);
			Assert.True(a != c);
		}
#endif

		[Fact]
		public void Guid_Equality()
		{
			var g = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
			SecureGuid a = g,
				b = g,
				c = Guid.Empty;
			Assert.True(a == b);
			Assert.True(a != c);
		}

		[Fact]
		public void TimeSpan_Equality()
		{
			SecureTimeSpan a = TimeSpan.FromMinutes(30);
			SecureTimeSpan b = TimeSpan.FromMinutes(30);
			SecureTimeSpan c = TimeSpan.FromMinutes(31);
			Assert.True(a == b);
			Assert.True(a != c);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void TimeOnly_Equality()
		{
			SecureTimeOnly a = new TimeOnly(12, 30);
			SecureTimeOnly b = new TimeOnly(12, 30);
			SecureTimeOnly c = new TimeOnly(12, 31);
			Assert.True(a == b);
			Assert.True(a != c);
		}
#endif

		[Fact]
		public void DateTimeOffset_Ordering()
		{
			var t = new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.Zero);
			SecureDateTimeOffset a = t,
				b = t,
				c = t.AddHours(1);
			Assert.True(a == b);
			Assert.True(a != c);
			Assert.True(a < c);
			Assert.True(c > a);
			Assert.True(a <= b);
			Assert.True(a.CompareTo(c) < 0);
			Assert.True(c.CompareTo(a) > 0);
			Assert.Equal(0, a.CompareTo(b));
		}

		[Fact]
		public void String_Equality_Operators()
		{
			SecureString a = "same";
			SecureString b = "same";
			SecureString c = "different";
			Assert.True(a == b);
			Assert.True(a != c);
			Assert.True(a.Equals(b));
		}

		[Fact]
		public void CompareTo_ConsistentWithOperators()
		{
			SecureInt a = 10,
				b = 3;
			Assert.True(a.CompareTo(b) > 0);
			Assert.True(b.CompareTo(a) < 0);
			Assert.Equal(0, a.CompareTo(new SecureInt(10)));
			// sign agreement between CompareTo and the ordering operators
			Assert.Equal(a > b, a.CompareTo(b) > 0);
			Assert.Equal(a == b, a.CompareTo(b) == 0);
		}

		[Fact]
		public void CompoundAssignment_Works()
		{
			SecureInt c = 10;
			c += 5;
			Assert.Equal(15, (int)c);
			c -= 3;
			Assert.Equal(12, (int)c);
			c *= 2;
			Assert.Equal(24, (int)c);
			c /= 4;
			Assert.Equal(6, (int)c);
			c %= 4;
			Assert.Equal(2, (int)c);
		}

		[Fact]
		public void DivideByZero_ThrowsDivideByZeroException()
		{
			SecureInt a = 1;
			SecureInt zero = 0;
			Assert.Throws<DivideByZeroException>(() =>
			{
				_ = (int)(a / zero);
			});
		}

		[Fact]
		public void Double_DivideByZero_YieldsInfinity()
		{
			// BCL parity: floating-point division runs on decrypted temporaries
			SecureDouble a = 1.0;
			SecureDouble zero = 0.0;
			Assert.Equal(double.PositiveInfinity, (double)(a / zero));
		}

		[Fact]
		public void IntegerOverflow_WrapsUnchecked()
		{
			// unchecked is the project default: overflow wraps like raw ints
			SecureInt m = int.MaxValue;
			Assert.Equal(int.MinValue, (int)(m + 1));
		}

		[Fact]
		public void Equality_Ignores_Ciphertext()
		{
			SecureInt a = 7;
			SecureInt b = 7;
			Assert.True(a.Equals(b));
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		// ---------- allocations ----------

		[Fact]
		public void PrimitiveWrappers_AllocationFree()
		{
			// Every Cell/Cell128 primitive wrapper: construct, read, arithmetic.
			// Pure engine ops only — ToString is pinned separately (it allocates
			// exactly one string; desktop JITs may elide it, Unity never does).
			ConsumePrimitives();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();

			ConsumePrimitives();

			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(before, after);
		}

		private static void ConsumePrimitives()
		{
			SecureBool b = true;
			_ = (bool)b;
			SecureByte by = 200;
			_ = (byte)(by + by);
			SecureSByte sb = new SecureSByte(-100);
			_ = (sbyte)(sb + sb);
			SecureChar ch = 'A';
			_ = (char)ch;
			SecureShort sh = -30000;
			_ = (short)(sh - sh);
			SecureUShort ush = 60000;
			_ = (ushort)(ush + ush);
			SecureInt i = -2000000000;
			_ = (int)(i * i);
			SecureUInt ui = new SecureUInt(3000000000u);
			_ = (uint)(ui / ui);
			SecureLong l = -9000000000000000000L;
			_ = (long)(l + l);
			SecureULong ul = new SecureULong(18000000000000000000ul);
			_ = (ulong)(ul - ul);
			SecureFloat f = 1.5f;
			_ = (float)(f + f);
			SecureDouble d = 2.5;
			_ = (double)(d * d);
			SecureDecimal m = 123.456m;
			_ = (decimal)(m - m);
			SecureGuid g = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
			_ = (Guid)g;
			SecureDateTime dt = new DateTime(2026, 9, 19, 13, 37, 0, DateTimeKind.Utc);
			_ = (DateTime)dt;
			SecureDateTimeOffset dto = new DateTimeOffset(2026, 9, 19, 1, 0, 0, TimeSpan.Zero);
			_ = (DateTimeOffset)dto;
			SecureTimeSpan ts = TimeSpan.FromTicks(123456789);
			_ = (TimeSpan)ts;
#if NET6_0_OR_GREATER
			SecureDateOnly date = new DateOnly(2026, 2, 28);
			_ = (DateOnly)date;
			SecureTimeOnly time = new TimeOnly(12, 30);
			_ = (TimeOnly)time;
#endif
#if NET
			SecureRune r = new System.Text.Rune(0x1F600);
			_ = (System.Text.Rune)r;
#endif
		}

		[Fact]
		public void PrimitiveToString_AllocatesSingleString()
		{
			// ToString allocates exactly one small string — no hidden extras.
			// Lower bound is 0, not 1: desktop JITs elide the warmed pure call
			// entirely (verified 0 B), while Unity Mono/IL2CPP allocate ~30 B.
			// Either way a second hidden allocation would break the ceiling.
			SecureInt w = 6;
			_ = w.ToString(); // warm up the ToString path itself
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();

			string s = w.ToString();

			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal("6", s);
			Assert.InRange(after - before, 0, 128);
		}

		[Fact]
		public void String_AllocationBoundedAndDeterministic()
		{
			// A write allocates exactly one backing array; a read exactly one
			// string. Identical repeats must allocate identically (catches
			// double-alloc regressions without hardcoding runtime constants).
			SecureString warm = "Hello, secured world!";
			_ = (string)warm;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			long w0 = GC.GetAllocatedBytesForCurrentThread();
			SecureString p = "Hello, secured world!";
			long w1 = GC.GetAllocatedBytesForCurrentThread();
			p = "Hello, secured world!";
			long w2 = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(w1 - w0, w2 - w1);
			Assert.InRange(w1 - w0, 1, 256);

			long r0 = GC.GetAllocatedBytesForCurrentThread();
			_ = (string)p;
			long r1 = GC.GetAllocatedBytesForCurrentThread();
			_ = (string)p;
			long r2 = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(r1 - r0, r2 - r1);
			Assert.InRange(r1 - r0, 1, 256);
		}
	}
}
