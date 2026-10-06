using System;
using System.Globalization;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Bit-pattern coverage for every applicable wrapper type: +-1000 around each 2^n
	/// transition in range (exact encodings shift at powers of two), walking single bits
	/// across the full width, and alternating-bit masks sized to the type. Values are
	/// passed as plain literals (running multiply, never bit ops in the runner, except
	/// Guid walking bits where 128-bit literals have no C# syntax). Skipped where the
	/// concept does not apply: bool (no numeric structure), string (length boundaries
	/// already covered by word-length tests), float lanes (alternating integer patterns
	/// round on conversion — exponent sweeps and walking ARE their bit coverage).
	/// Float/double windows skip bit-duplicates: past ULP 2000 the whole window rounds
	/// to the transition, and re-testing identical bits buys nothing.
	/// </summary>
	[Trait("Category", "Thoroughness")]
	public class PowerOfTwoBoundaryTests
	{
		private const int Window = 1000;

		[Fact]
		public void Byte_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < byte.MinValue || value > byte.MaxValue)
				{
					return;
				}
				byte v = (byte)value;
				SecureByte p = v;
				if ((byte)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 7; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"byte.t{n}");
				}
			}
			for (int n = 0; n <= 7; n++)
			{
				Check(1L << n, $"byte.w{n}");
			}
			Check(byte.MaxValue, "byte.max");
			Check(0x55, "byte.alt P");
			Check(0xAA, "byte.alt Q");
			Assert.True(
				failures == 0,
				$"byte patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void SByte_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < sbyte.MinValue || value > sbyte.MaxValue)
				{
					return;
				}
				sbyte v = (sbyte)value;
				SecureSByte p = v;
				if ((sbyte)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 6; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"sbyte.t+{n}");
					Check(-(power + d), $"sbyte.t-{n}");
				}
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(sbyte.MinValue + d, "sbyte.min");
			}
			for (int n = 0; n <= 6; n++)
			{
				Check(1L << n, $"sbyte.w+{n}");
				Check(-(1L << n), $"sbyte.w-{n}");
			}
			Check(sbyte.MinValue, "sbyte.min exact");
			Check(sbyte.MaxValue, "sbyte.max exact");
			Check(0x55, "sbyte.alt P");
			Check(-0x55, "sbyte.alt -P");
			Check(unchecked((sbyte)0xAA), "sbyte.alt Q");
			Assert.True(
				failures == 0,
				$"sbyte patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Short_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < short.MinValue || value > short.MaxValue)
				{
					return;
				}
				short v = (short)value;
				SecureShort p = v;
				if ((short)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 14; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"short.t+{n}");
					Check(-(power + d), $"short.t-{n}");
				}
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(short.MinValue + d, "short.min");
			}
			for (int n = 0; n <= 14; n++)
			{
				Check(1L << n, $"short.w+{n}");
				Check(-(1L << n), $"short.w-{n}");
			}
			Check(short.MinValue, "short.min exact");
			Check(short.MaxValue, "short.max exact");
			Check(0x5555, "short.alt P");
			Check(-0x5555, "short.alt -P");
			Check(unchecked((short)0xAAAA), "short.alt Q");
			Assert.True(
				failures == 0,
				$"short patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void UShort_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < ushort.MinValue || value > ushort.MaxValue)
				{
					return;
				}
				ushort v = (ushort)value;
				SecureUShort p = v;
				if ((ushort)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 15; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"ushort.t{n}");
				}
			}
			for (int n = 0; n <= 15; n++)
			{
				Check(1L << n, $"ushort.w{n}");
			}
			Check(ushort.MaxValue, "ushort.max exact");
			Check(0x5555, "ushort.alt P");
			Check(0xAAAA, "ushort.alt Q");
			Assert.True(
				failures == 0,
				$"ushort patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Int_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < int.MinValue || value > int.MaxValue)
				{
					return;
				}
				int v = (int)value;
				SecureInt p = v;
				if ((int)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 30; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"int.t+{n}");
					Check(-(power + d), $"int.t-{n}");
				}
			}
			for (long d = 0; d <= Window; d++)
			{
				Check((long)int.MinValue + d, "int.min");
			}
			for (int n = 0; n <= 30; n++)
			{
				Check(1L << n, $"int.w+{n}");
				Check(-(1L << n), $"int.w-{n}");
			}
			Check(int.MinValue, "int.min exact");
			Check(int.MaxValue, "int.max exact");
			Check(1431655765, "int.alt P");
			Check(-1431655765, "int.alt -P");
			Check(-1431655766, "int.alt Q");
			Assert.True(
				failures == 0,
				$"int patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void UInt_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < uint.MinValue || value > uint.MaxValue)
				{
					return;
				}
				uint v = (uint)value;
				SecureUInt p = v;
				if ((uint)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = {v}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 31; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"uint.t{n}");
				}
			}
			for (int n = 0; n <= 31; n++)
			{
				Check(1L << n, $"uint.w{n}");
			}
			Check(uint.MaxValue, "uint.max exact");
			Check(1431655765, "uint.alt P");
			Check(2863311530, "uint.alt Q");
			Assert.True(
				failures == 0,
				$"uint patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Long_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				SecureLong p = value;
				if ((long)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			void CheckU(ulong value, string block)
			{
				if (value > long.MaxValue)
				{
					return;
				}
				Check((long)value, block);
			}
			for (int n = 0; n <= 62; n++)
			{
				ulong power = 1UL << n;
				for (long d = -Window; d <= Window; d++)
				{
					if (d < 0 && (ulong)(-d) > power)
					{
						continue;
					}
					ulong v = d < 0 ? power - (ulong)(-d) : power + (ulong)d;
					CheckU(v, $"long.t+{n}");
					if (v != 0 && v - 1 <= (ulong)long.MaxValue)
					{
						Check(-(long)v, $"long.t-{n}");
					}
				}
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(unchecked(long.MinValue + d), "long.min");
			}
			for (int n = 0; n <= 62; n++)
			{
				CheckU(1UL << n, $"long.w+{n}");
				if ((1UL << n) <= (ulong)long.MaxValue)
				{
					Check(-(long)(1UL << n), $"long.w-{n}");
				}
			}
			Check(long.MinValue, "long.min exact");
			Check(long.MaxValue, "long.max exact");
			Check(6148914691236517205L, "long.alt P");
			Check(-6148914691236517205L, "long.alt -P");
			Check(-6148914691236517206L, "long.alt Q");
			Assert.True(
				failures == 0,
				$"long patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void ULong_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(ulong value, string block)
			{
				SecureULong p = value;
				if ((ulong)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 63; n++)
			{
				ulong power = 1UL << n;
				for (long d = -Window; d <= Window; d++)
				{
					if (d < 0 && (ulong)(-d) > power)
					{
						continue;
					}
					Check(d < 0 ? power - (ulong)(-d) : power + (ulong)d, $"ulong.t{n}");
				}
			}
			for (int n = 0; n <= 63; n++)
			{
				Check(1UL << n, $"ulong.w{n}");
			}
			Check(ulong.MaxValue, "ulong.max exact");
			Check(6148914691236517205UL, "ulong.alt P");
			Check(12297829382473034410UL, "ulong.alt Q");
			Assert.True(
				failures == 0,
				$"ulong patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Char_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < char.MinValue || value > char.MaxValue)
				{
					return;
				}
				char v = (char)value;
				SecureChar p = v;
				if ((char)p != v)
				{
					if (failures == 0)
					{
						first = $"{block} = U+{(int)v:X4}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 15; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"char.t{n}");
				}
			}
			for (int n = 0; n <= 15; n++)
			{
				Check(1L << n, $"char.w{n}");
			}
			Check(0x5555, "char.alt P");
			Check(0xAAAA, "char.alt Q");
			Assert.True(
				failures == 0,
				$"char patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Float_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(float value, string block)
			{
				SecureFloat p = value;
				if (
					BitConverter.SingleToInt32Bits((float)p)
					!= BitConverter.SingleToInt32Bits(value)
				)
				{
					if (failures == 0)
					{
						first = $"{block} = {value:R}";
					}
					failures++;
				}
				tested++;
			}
			// Exact powers first (always constructed, never deduped away).
			float up = 1f;
			for (int n = 0; n <= 127; n++)
			{
				Check(up, $"float.w+{n}");
				Check(-up, $"float.w-{n}");
				up *= 2f;
			}
			float down = 0.5f;
			for (int n = 1; n <= 149; n++)
			{
				Check(down, $"float.w+sub{n}");
				Check(-down, $"float.w-sub{n}");
				down /= 2f;
			}
			// Windows with bit-dedupe (see class doc).
			up = 1f;
			for (int n = 0; n <= 127; n++)
			{
				WindowBoth(up, $"float.t+{n}", $"float.t-{n}", Check);
				up *= 2f;
			}
			down = 0.5f;
			for (int n = 1; n <= 149; n++)
			{
				WindowBoth(down, $"float.tsub+{n}", $"float.tsub-{n}", Check);
				down /= 2f;
			}
			Assert.True(
				failures == 0,
				$"float patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Double_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(double value, string block)
			{
				SecureDouble p = value;
				if (
					BitConverter.DoubleToInt64Bits((double)p)
					!= BitConverter.DoubleToInt64Bits(value)
				)
				{
					if (failures == 0)
					{
						first = $"{block} = {value:R}";
					}
					failures++;
				}
				tested++;
			}
			double up = 1.0;
			for (int n = 0; n <= 1023; n++)
			{
				Check(up, $"double.w+{n}");
				Check(-up, $"double.w-{n}");
				up *= 2.0;
			}
			double down = 0.5;
			for (int n = 1; n <= 1074; n++)
			{
				Check(down, $"double.w+sub{n}");
				Check(-down, $"double.w-sub{n}");
				down /= 2.0;
			}
			up = 1.0;
			for (int n = 0; n <= 1023; n++)
			{
				WindowBoth(up, $"double.t+{n}", $"double.t-{n}", Check);
				up *= 2.0;
			}
			down = 0.5;
			for (int n = 1; n <= 1074; n++)
			{
				WindowBoth(down, $"double.tsub+{n}", $"double.tsub-{n}", Check);
				down /= 2.0;
			}
			Assert.True(
				failures == 0,
				$"double patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Decimal_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(decimal value, string block)
			{
				SecureDecimal p = value;
				if ((decimal)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			// 2^0..2^95 (2^96 - 1 is decimal.MaxValue, so 2^96 itself is out of range).
			decimal power = 0.5m;
			for (int n = 0; n <= 95; n++)
			{
				power *= 2m;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"decimal.t+{n}");
					Check(-(power + d), $"decimal.t-{n}");
				}
			}
			power = 0.5m;
			for (int n = 0; n <= 95; n++)
			{
				power *= 2m;
				Check(power, $"decimal.w+{n}");
				Check(-power, $"decimal.w-{n}");
			}
			decimal[] masks =
			{
				1431655765m, // 0x55555555
				-1431655765m,
				2863311530m, // 0xAAAAAAAA
				-2863311530m,
				6148914691236517205m, // 0x5555555555555555
				-6148914691236517205m,
				12297829382473034410m, // 0xAAAAAAAAAAAAAAAA
				-12297829382473034410m,
			};
			foreach (decimal mask in masks)
			{
				Check(mask, "decimal.alt");
			}
			Assert.True(
				failures == 0,
				$"decimal patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void DateTime_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long ticks, string block)
			{
				if (ticks < 0 || ticks > DateTime.MaxValue.Ticks)
				{
					return;
				}
				var value = new DateTime(ticks, DateTimeKind.Utc);
				SecureDateTime p = value;
				if ((DateTime)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {ticks}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 61; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"datetime.t{n}");
				}
				Check(power, $"datetime.w{n}");
			}
			Check(0x55555555L, "datetime.alt P");
			Check(0xAAAAAAAAL, "datetime.alt Q");
			Assert.True(
				failures == 0,
				$"datetime patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void DateTimeOffset_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long ticks, string block)
			{
				if (ticks < 0 || ticks > DateTimeOffset.MaxValue.Ticks)
				{
					return;
				}
				var value = new DateTimeOffset(ticks, TimeSpan.Zero);
				SecureDateTimeOffset p = value;
				if ((DateTimeOffset)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {ticks}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 61; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"dateoffset.t{n}");
				}
				Check(power, $"dateoffset.w{n}");
			}
			Check(0x55555555L, "dateoffset.alt P");
			Check(0xAAAAAAAAL, "dateoffset.alt Q");
			Assert.True(
				failures == 0,
				$"dateoffset patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void TimeSpan_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long ticks, string block)
			{
				var value = new TimeSpan(ticks);
				SecureTimeSpan p = value;
				if ((TimeSpan)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {ticks}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 62; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(unchecked(power + d), $"timespan.t+{n}");
					Check(unchecked(-(power + d)), $"timespan.t-{n}");
				}
				Check(power, $"timespan.w+{n}");
				Check(unchecked(-power), $"timespan.w-{n}");
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(unchecked(long.MinValue + d), "timespan.min");
			}
			Check(long.MinValue, "timespan.min exact");
			Check(long.MaxValue, "timespan.max exact");
			Check(0x5555555555555555L, "timespan.alt P");
			Check(-0x5555555555555555L, "timespan.alt -P");
			Check(unchecked((long)0xAAAAAAAAAAAAAAAAUL), "timespan.alt Q");
			Assert.True(
				failures == 0,
				$"timespan patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long days, string block)
			{
				if (days < 0 || days > 3652058)
				{
					return;
				}
				var value = DateOnly.FromDayNumber((int)days);
				SecureDateOnly p = value;
				if ((DateOnly)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {days}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 21; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"dateonly.t{n}");
				}
				Check(power, $"dateonly.w{n}");
			}
			Check(0x5555, "dateonly.alt P");
			Check(0xAAAA, "dateonly.alt Q");
			Assert.True(
				failures == 0,
				$"dateonly patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void TimeOnly_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long ticks, string block)
			{
				if (ticks < 0 || ticks > TimeOnly.MaxValue.Ticks)
				{
					return;
				}
				var value = new TimeOnly(ticks);
				SecureTimeOnly p = value;
				if ((TimeOnly)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {ticks}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 46; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"timeonly.t{n}");
				}
				Check(power, $"timeonly.w{n}");
			}
			Check(0x55555555L, "timeonly.alt P");
			Check(0xAAAAAAAAL, "timeonly.alt Q");
			Assert.True(
				failures == 0,
				$"timeonly patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}
#endif

#if NET
		[Fact]
		public void Rune_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(int codePoint, string block)
			{
				if (
					codePoint < 0
					|| codePoint > 0x10FFFF
					|| (codePoint >= 0xD800 && codePoint <= 0xDFFF)
				)
				{
					return;
				}
				var value = new System.Text.Rune(codePoint);
				SecureRune p = value;
				if (!value.Equals((System.Text.Rune)p))
				{
					if (failures == 0)
					{
						first = $"{block} = U+{codePoint:X4}";
					}
					failures++;
				}
				tested++;
			}
			for (int n = 0; n <= 20; n++)
			{
				int power = 1 << n;
				for (int d = -Window; d <= Window; d++)
				{
					Check(power + d, $"rune.t{n}");
				}
				Check(power, $"rune.w{n}");
			}
			Check(0x5555, "rune.alt P");
			Check(0xAAAA, "rune.alt Q");
			Assert.True(
				failures == 0,
				$"rune patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}
#endif

		[Fact]
		public void Guid_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(Guid value, string block)
			{
				SecureGuid p = value;
				if ((Guid)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			// Walking single bits across all 128 positions via the (a, b, c, d) fields.
			for (int n = 0; n <= 31; n++)
			{
				Check(new Guid(1 << n, 0, 0, new byte[8]), $"guid.wa{n}");
			}
			for (int n = 0; n <= 15; n++)
			{
				Check(new Guid(0, (short)(1 << n), 0, new byte[8]), $"guid.wb{n}");
				Check(new Guid(0, 0, (short)(1 << n), new byte[8]), $"guid.wc{n}");
			}
			for (int n = 0; n <= 63; n++)
			{
				byte[] d = new byte[8];
				d[n / 8] = (byte)(1 << (n % 8));
				Check(new Guid(0, 0, 0, d), $"guid.wd{n}");
			}
			// Alternating patterns as literals (no bit ops).
			Check(Guid.Parse("55555555-5555-5555-5555-555555555555"), "guid.alt P");
			Check(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "guid.alt Q");
			// Low-word counter sweeps under varied high-word contexts.
			byte[] contexts = { 0x00, 0xFF, 0x55, 0xAA };
			foreach (byte context in contexts)
			{
				for (uint k = 0; k <= 1000; k++)
				{
					Check(GuidFromCounter(k, context), $"guid.low{context:X2}+{k}");
					Check(
						GuidFromCounter(0xFFFFFFFFu - 1000u + k, context),
						$"guid.low{context:X2}-{k}"
					);
				}
			}
			Assert.True(
				failures == 0,
				$"guid patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void BigInteger_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(BigInteger value, string block)
			{
				SecureBigInteger p = value;
				if ((BigInteger)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			BigInteger power = BigInteger.One;
			for (int n = 0; n <= 128; n++)
			{
				for (long d = -Window; d <= Window; d++)
				{
					Check(power + d, $"bigint.t+{n}");
					Check(-(power + d), $"bigint.t-{n}");
				}
				power *= 2;
			}
			power = BigInteger.One;
			for (int n = 0; n <= 256; n++)
			{
				Check(power, $"bigint.w+{n}");
				Check(-power, $"bigint.w-{n}");
				power *= 2;
			}
			string[] masks =
			{
				"55555555",
				"AAAAAAAA",
				"5555555555555555",
				"AAAAAAAAAAAAAAAA",
				"55555555555555555555555555555555",
				"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
				"5555555555555555555555555555555555555555555555555555555555555555",
				"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
			};
			foreach (string mask in masks)
			{
				BigInteger positive = BigInteger.Parse("0" + mask, NumberStyles.HexNumber);
				Check(positive, "bigint.alt P");
				Check(-positive, "bigint.alt -P");
			}
			Assert.True(
				failures == 0,
				$"bigint patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Complex_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(Complex value, string block)
			{
				SecureComplex p = value;
				Complex back = p;
				if (
					BitConverter.DoubleToInt64Bits(back.Real)
						!= BitConverter.DoubleToInt64Bits(value.Real)
					|| BitConverter.DoubleToInt64Bits(back.Imaginary)
						!= BitConverter.DoubleToInt64Bits(value.Imaginary)
				)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			double up = 1.0;
			for (int n = 0; n <= 64; n++)
			{
				long prevReal = 0;
				long prevImag = 0;
				for (long d = -Window; d <= Window; d++)
				{
					double real = up + d;
					double imag = up - d;
					long realBits = BitConverter.DoubleToInt64Bits(real);
					long imagBits = BitConverter.DoubleToInt64Bits(imag);
					if (d == -Window || realBits != prevReal || imagBits != prevImag)
					{
						Check(new Complex(real, imag), $"complex.t+{n}");
						prevReal = realBits;
						prevImag = imagBits;
					}
				}
				Check(new Complex(up, up), $"complex.w+{n}");
				Check(new Complex(-up, -up), $"complex.w-{n}");
				up *= 2.0;
			}
			Check(new Complex(1431655765.0, 2863311530.0), "complex.alt P");
			Check(new Complex(-1431655765.0, -1431655766.0), "complex.alt Q");
			Assert.True(
				failures == 0,
				$"complex patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Vector2_BitPatterns()
		{
			FloatLanes("vector2", 2, (lanes, block, st) => CheckVector2(lanes, block, st));
		}

		[Fact]
		public void Vector3_BitPatterns()
		{
			FloatLanes(
				"vector3",
				3,
				(lanes, block, st) =>
				{
					var value = new Vector3(lanes[0], lanes[1], lanes[2]);
					SecureVector3 p = value;
					Report((Vector3)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Vector4_BitPatterns()
		{
			FloatLanes(
				"vector4",
				4,
				(lanes, block, st) =>
				{
					var value = new Vector4(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureVector4 p = value;
					Report((Vector4)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Quaternion_BitPatterns()
		{
			FloatLanes(
				"quaternion",
				4,
				(lanes, block, st) =>
				{
					var value = new Quaternion(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureQuaternion p = value;
					Report((Quaternion)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Plane_BitPatterns()
		{
			FloatLanes(
				"plane",
				4,
				(lanes, block, st) =>
				{
					var value = new Plane(new Vector3(lanes[0], lanes[1], lanes[2]), lanes[3]);
					SecurePlane p = value;
					Report((Plane)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Matrix3x2_BitPatterns()
		{
			FloatLanes(
				"matrix3x2",
				6,
				(lanes, block, st) =>
				{
					var value = new Matrix3x2(
						lanes[0],
						lanes[1],
						lanes[2],
						lanes[3],
						lanes[4],
						lanes[5]
					);
					SecureMatrix3x2 p = value;
					Report((Matrix3x2)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Matrix4x4_BitPatterns()
		{
			FloatLanes(
				"matrix4x4",
				16,
				(lanes, block, st) =>
				{
					var value = new Matrix4x4(
						lanes[0],
						lanes[1],
						lanes[2],
						lanes[3],
						lanes[4],
						lanes[5],
						lanes[6],
						lanes[7],
						lanes[8],
						lanes[9],
						lanes[10],
						lanes[11],
						lanes[12],
						lanes[13],
						lanes[14],
						lanes[15]
					);
					SecureMatrix4x4 p = value;
					Report((Matrix4x4)p == value, block, value.ToString(), st);
				}
			);
		}

		private sealed class PatternState
		{
			public long Failures;
			public string First = "";
			public long Tested;
		}

		private static void Report(bool ok, string block, string text, PatternState st)
		{
			if (!ok)
			{
				if (st.Failures == 0)
				{
					st.First = $"{block} = {text}";
				}
				st.Failures++;
			}
			st.Tested++;
		}

		private static void CheckVector2(float[] lanes, string block, PatternState st)
		{
			var value = new Vector2(lanes[0], lanes[1]);
			SecureVector2 p = value;
			Report((Vector2)p == value, block, value.ToString(), st);
		}

		// Float-lane treatment for multi-component numerics: per-lane exact powers
		// (lane k holds 2^(n-k): exact in binary FP, always distinct across lanes),
		// walking both signs, windows with per-lane bit-dedupe. No alternating: the
		// integer-word concept does not survive float conversion (patterns round away).
		private static void FloatLanes(
			string name,
			int laneCount,
			Action<float[], string, PatternState> check
		)
		{
			var st = new PatternState();
			double up = 1.0;
			for (int n = 0; n <= 127; n++)
			{
				check(PowerLanes(laneCount, up), $"{name}.w+{n}", st);
				check(Negate(PowerLanes(laneCount, up)), $"{name}.w-{n}", st);
				WindowLanes(name, laneCount, up, n, check, st);
				up *= 2.0;
			}
			double down = 0.5;
			for (int n = 1; n <= 149; n++)
			{
				check(PowerLanes(laneCount, down), $"{name}.wsub+{n}", st);
				check(Negate(PowerLanes(laneCount, down)), $"{name}.wsub-{n}", st);
				WindowLanes(name, laneCount, down, n, check, st);
				down /= 2.0;
			}
			Assert.True(
				st.Failures == 0,
				$"{name} patterns: {st.Failures}/{st.Tested} mismatched, first: {st.First}"
			);
		}

		private static float[] PowerLanes(int laneCount, double power)
		{
			float[] lanes = new float[laneCount];
			double scale = 1.0;
			for (int lane = 0; lane < laneCount; lane++)
			{
				lanes[lane] = (float)(power * scale);
				scale /= 2.0;
			}
			return lanes;
		}

		private static float[] Negate(float[] lanes)
		{
			float[] negated = new float[lanes.Length];
			for (int lane = 0; lane < lanes.Length; lane++)
			{
				negated[lane] = -lanes[lane];
			}
			return negated;
		}

		private static void WindowLanes(
			string name,
			int laneCount,
			double center,
			int n,
			Action<float[], string, PatternState> check,
			PatternState st
		)
		{
			float[] lanes = new float[laneCount];
			long[] prev = new long[laneCount];
			double scale = 1.0;
			float[] centers = new float[laneCount];
			for (int lane = 0; lane < laneCount; lane++)
			{
				centers[lane] = (float)(center * scale);
				scale /= 2.0;
			}
			for (long d = -Window; d <= Window; d++)
			{
				bool changed = d == -Window;
				for (int lane = 0; lane < laneCount; lane++)
				{
					float value = centers[lane] + d;
					long bits = BitConverter.SingleToInt32Bits(value);
					if (d == -Window || bits != prev[lane])
					{
						changed = true;
						prev[lane] = bits;
					}
					lanes[lane] = value;
				}
				if (changed)
				{
					check(lanes, $"{name}.t{n}", st);
				}
			}
		}

		private static void WindowBoth(
			double center,
			string plusBlock,
			string minusBlock,
			Action<double, string> check
		)
		{
			long prevPlus = 0;
			long prevMinus = 0;
			for (long d = -Window; d <= Window; d++)
			{
				double plus = center + d;
				long plusBits = BitConverter.DoubleToInt64Bits(plus);
				if (d == -Window || plusBits != prevPlus)
				{
					check(plus, plusBlock);
					prevPlus = plusBits;
				}
				double minus = -(center + d);
				long minusBits = BitConverter.DoubleToInt64Bits(minus);
				if (d == -Window || minusBits != prevMinus)
				{
					check(minus, minusBlock);
					prevMinus = minusBits;
				}
			}
		}

		private static void WindowBoth(
			float center,
			string plusBlock,
			string minusBlock,
			Action<float, string> check
		)
		{
			int prevPlus = 0;
			int prevMinus = 0;
			for (long d = -Window; d <= Window; d++)
			{
				float plus = center + d;
				int plusBits = BitConverter.SingleToInt32Bits(plus);
				if (d == -Window || plusBits != prevPlus)
				{
					check(plus, plusBlock);
					prevPlus = plusBits;
				}
				float minus = -(center + d);
				int minusBits = BitConverter.SingleToInt32Bits(minus);
				if (d == -Window || minusBits != prevMinus)
				{
					check(minus, minusBlock);
					prevMinus = minusBits;
				}
			}
		}

		private static Guid GuidFromCounter(uint low, byte fill)
		{
			byte[] bytes = new byte[16];
			bytes[0] = (byte)low;
			bytes[1] = (byte)(low >> 8);
			bytes[2] = (byte)(low >> 16);
			bytes[3] = (byte)(low >> 24);
			for (int k = 4; k < 16; k++)
			{
				bytes[k] = fill;
			}
			return new Guid(bytes);
		}
	}
}
