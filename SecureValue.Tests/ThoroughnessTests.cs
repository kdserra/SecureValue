using System;
using System.Collections.Generic;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Range-sweep round-trip coverage: 1M values up from the lowest, 1M down from the
	/// highest, and 5M around zero (0..5M for unsigned-only ranges) — 7M per full-range
	/// type. Types whose whole range is smaller are swept EXHAUSTIVELY instead (stronger
	/// and faster): byte/sbyte (256), short/ushort/char (65536), bool (2), Rune (~1.11M
	/// valid code points). SecureString and SecureBigInteger use scaled counts (70K and
	/// 700K): they allocate per operation, so the full 7M would take many minutes and
	/// gigabytes of GC churn for no extra signal. No per-item asserts (too slow at this
	/// scale): failures are counted and the first is recorded, with one assert per sweep.
	/// </summary>
	[Trait("Category", "Thoroughness")]
	public class ThoroughnessTests
	{
		private const long LowSweep = 1_000_000;
		private const long HighSweep = 1_000_000;
		private const long MidSweep = 5_000_000;
		private const long MidHalf = MidSweep / 2;

		// String/BigInteger scale-downs (see class doc).
		private const long StringLow = 10_000;
		private const long StringHigh = 10_000;
		private const long StringMid = 50_000;
		private const long BigLow = 100_000;
		private const long BigHigh = 100_000;
		private const long BigMid = 500_000;

		private static readonly float FloatStep = float.MaxValue / LowSweep;
		private static readonly double DoubleStep = double.MaxValue / LowSweep;
		private static readonly BigInteger BigHuge = BigInteger.Pow(2, 256);
		private static readonly long Y2kTicks = new DateTime(
			2000,
			1,
			1,
			0,
			0,
			0,
			DateTimeKind.Utc
		).Ticks;
		private static readonly int[] UtcOffsetsMinutes = { 0, 120, -330, 840, -840 };

		[Fact]
		public void Bool_Thoroughness()
		{
			Sweep("bool", 2, i => i == 1, v => (bool)(SecureBool)v);
		}

		[Fact]
		public void Byte_Thoroughness()
		{
			Sweep("byte", 256, i => (byte)i, v => (byte)(SecureByte)v);
		}

		[Fact]
		public void SByte_Thoroughness()
		{
			Sweep("sbyte", 256, i => unchecked((sbyte)(i - 128)), v => (sbyte)(SecureSByte)v);
		}

		[Fact]
		public void Char_Thoroughness()
		{
			Sweep("char", 65536, i => (char)i, v => (char)(SecureChar)v);
		}

		[Fact]
		public void Short_Thoroughness()
		{
			Sweep("short", 65536, i => unchecked((short)(i - 32768)), v => (short)(SecureShort)v);
		}

		[Fact]
		public void UShort_Thoroughness()
		{
			Sweep("ushort", 65536, i => (ushort)i, v => (ushort)(SecureUShort)v);
		}

		[Fact]
		public void Int_Thoroughness()
		{
			Sweep(
				"int.low",
				LowSweep,
				i => unchecked(int.MinValue + (int)i),
				v => (int)(SecureInt)v
			);
			Sweep(
				"int.high",
				HighSweep,
				i => unchecked(int.MaxValue - (int)i),
				v => (int)(SecureInt)v
			);
			Sweep("int.mid", MidSweep, i => unchecked((int)(i - MidHalf)), v => (int)(SecureInt)v);
		}

		[Fact]
		public void UInt_Thoroughness()
		{
			Sweep("uint.low", LowSweep, i => (uint)i, v => (uint)(SecureUInt)v);
			Sweep(
				"uint.high",
				HighSweep,
				i => unchecked(uint.MaxValue - (uint)i),
				v => (uint)(SecureUInt)v
			);
			Sweep("uint.mid", MidSweep, i => (uint)i, v => (uint)(SecureUInt)v);
		}

		[Fact]
		public void Long_Thoroughness()
		{
			Sweep(
				"long.low",
				LowSweep,
				i => unchecked(long.MinValue + i),
				v => (long)(SecureLong)v
			);
			Sweep(
				"long.high",
				HighSweep,
				i => unchecked(long.MaxValue - i),
				v => (long)(SecureLong)v
			);
			Sweep("long.mid", MidSweep, i => i - MidHalf, v => (long)(SecureLong)v);
		}

		[Fact]
		public void ULong_Thoroughness()
		{
			Sweep("ulong.low", LowSweep, i => (ulong)i, v => (ulong)(SecureULong)v);
			Sweep(
				"ulong.high",
				HighSweep,
				i => unchecked(ulong.MaxValue - (ulong)i),
				v => (ulong)(SecureULong)v
			);
			Sweep("ulong.mid", MidSweep, i => (ulong)i, v => (ulong)(SecureULong)v);
		}

		[Fact]
		public void Float_Thoroughness()
		{
			Func<float, float, bool> bits = (a, b) =>
				BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b);
			Sweep(
				"float.low",
				LowSweep,
				i => float.MinValue + i * FloatStep,
				v => (float)(SecureFloat)v,
				bits
			);
			Sweep(
				"float.high",
				HighSweep,
				i => float.MaxValue - i * FloatStep,
				v => (float)(SecureFloat)v,
				bits
			);
			Sweep(
				"float.mid",
				MidSweep,
				i => (i - MidHalf) * 0.5f,
				v => (float)(SecureFloat)v,
				bits
			);
		}

		[Fact]
		public void Double_Thoroughness()
		{
			Func<double, double, bool> bits = (a, b) =>
				BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
			Sweep(
				"double.low",
				LowSweep,
				i => double.MinValue + i * DoubleStep,
				v => (double)(SecureDouble)v,
				bits
			);
			Sweep(
				"double.high",
				HighSweep,
				i => double.MaxValue - i * DoubleStep,
				v => (double)(SecureDouble)v,
				bits
			);
			Sweep(
				"double.mid",
				MidSweep,
				i => (i - MidHalf) * 0.5,
				v => (double)(SecureDouble)v,
				bits
			);
		}

		[Fact]
		public void Decimal_Thoroughness()
		{
			Sweep(
				"decimal.low",
				LowSweep,
				i => decimal.MinValue + i,
				v => (decimal)(SecureDecimal)v
			);
			Sweep(
				"decimal.high",
				HighSweep,
				i => decimal.MaxValue - i,
				v => (decimal)(SecureDecimal)v
			);
			Sweep(
				"decimal.mid",
				MidSweep,
				i => (decimal)(i - MidHalf),
				v => (decimal)(SecureDecimal)v
			);
		}

		[Fact]
		public void String_Thoroughness()
		{
			Sweep("string.low", StringLow, i => "low-" + i, v => ((string?)(SecureString)v)!);
			Sweep(
				"string.high",
				StringHigh,
				i => new string((char)('a' + (i % 26)), 200 + (int)(i % 100)),
				v => ((string?)(SecureString)v)!
			);
			Sweep(
				"string.mid",
				StringMid,
				i => "mid-" + (i - StringMid / 2),
				v => ((string?)(SecureString)v)!
			);
		}

		[Fact]
		public void Guid_Thoroughness()
		{
			Sweep("guid.low", LowSweep, i => GuidFromULong((ulong)i, 0), v => (Guid)(SecureGuid)v);
			Sweep(
				"guid.high",
				HighSweep,
				i => GuidFromULong(ulong.MaxValue - (ulong)i, 0xFF),
				v => (Guid)(SecureGuid)v
			);
			Sweep("guid.mid", MidSweep, i => GuidFromULong((ulong)i, 0), v => (Guid)(SecureGuid)v);
		}

		[Fact]
		public void DateTime_Thoroughness()
		{
			Sweep(
				"datetime.low",
				LowSweep,
				i => new DateTime(i, Kind(i)),
				v => (DateTime)(SecureDateTime)v
			);
			Sweep(
				"datetime.high",
				HighSweep,
				i => new DateTime(DateTime.MaxValue.Ticks - i, Kind(i)),
				v => (DateTime)(SecureDateTime)v
			);
			Sweep(
				"datetime.mid",
				MidSweep,
				i => new DateTime(Y2kTicks + i - MidHalf, Kind(i)),
				v => (DateTime)(SecureDateTime)v
			);
		}

		[Fact]
		public void DateTimeOffset_Thoroughness()
		{
			Sweep(
				"dateoffset.low",
				LowSweep,
				i => new DateTimeOffset(i, TimeSpan.Zero),
				v => (DateTimeOffset)(SecureDateTimeOffset)v
			);
			Sweep(
				"dateoffset.high",
				HighSweep,
				i => new DateTimeOffset(DateTimeOffset.MaxValue.Ticks - i, TimeSpan.Zero),
				v => (DateTimeOffset)(SecureDateTimeOffset)v
			);
			Sweep(
				"dateoffset.mid",
				MidSweep,
				i => new DateTimeOffset(
					Y2kTicks + i - MidHalf,
					TimeSpan.FromMinutes(UtcOffsetsMinutes[(int)(i % UtcOffsetsMinutes.Length)])
				),
				v => (DateTimeOffset)(SecureDateTimeOffset)v
			);
		}

		[Fact]
		public void TimeSpan_Thoroughness()
		{
			Sweep(
				"timespan.low",
				LowSweep,
				i => new TimeSpan(unchecked(long.MinValue + i)),
				v => (TimeSpan)(SecureTimeSpan)v
			);
			Sweep(
				"timespan.high",
				HighSweep,
				i => new TimeSpan(unchecked(long.MaxValue - i)),
				v => (TimeSpan)(SecureTimeSpan)v
			);
			Sweep(
				"timespan.mid",
				MidSweep,
				i => new TimeSpan(i - MidHalf),
				v => (TimeSpan)(SecureTimeSpan)v
			);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_Thoroughness()
		{
			// Whole range (3.65M day numbers) is smaller than 7M: exhaustive.
			Sweep(
				"dateonly",
				3652059,
				i => DateOnly.FromDayNumber((int)i),
				v => (DateOnly)(SecureDateOnly)v
			);
		}

		[Fact]
		public void TimeOnly_Thoroughness()
		{
			long max = TimeOnly.MaxValue.Ticks;
			Sweep("timeonly.low", LowSweep, i => new TimeOnly(i), v => (TimeOnly)(SecureTimeOnly)v);
			Sweep(
				"timeonly.high",
				HighSweep,
				i => new TimeOnly(max - i),
				v => (TimeOnly)(SecureTimeOnly)v
			);
			Sweep(
				"timeonly.mid",
				MidSweep,
				i => new TimeOnly(432000000000L + i - MidHalf),
				v => (TimeOnly)(SecureTimeOnly)v
			);
		}
#endif

#if NET
		[Fact]
		public void Rune_Thoroughness()
		{
			// Every valid code point (~1.11M, skipping the surrogate range): exhaustive.
			long failures = 0;
			string first = "";
			long tested = 0;
			for (int i = 0; i <= 0x10FFFF; i++)
			{
				if (i >= 0xD800 && i <= 0xDFFF)
				{
					continue;
				}
				var value = new System.Text.Rune(i);
				SecureRune p = value;
				if (!value.Equals((System.Text.Rune)p))
				{
					if (failures == 0)
					{
						first = $"rune[{tested}] = U+{i:X4}";
					}
					failures++;
				}
				tested++;
			}
			Assert.True(failures == 0, $"rune: {failures}/{tested} mismatched, first: {first}");
		}
#endif

		[Fact]
		public void BigInteger_Thoroughness()
		{
			Sweep(
				"bigint.low",
				BigLow,
				i => new BigInteger(i),
				v => (BigInteger)(SecureBigInteger)v
			);
			Sweep("bigint.high", BigHigh, i => BigHuge - i, v => (BigInteger)(SecureBigInteger)v);
			Sweep(
				"bigint.mid",
				BigMid,
				i => new BigInteger(i - BigMid / 2),
				v => (BigInteger)(SecureBigInteger)v
			);
		}

		[Fact]
		public void Complex_Thoroughness()
		{
			Func<Complex, Complex, bool> bits = (a, b) =>
				BitConverter.DoubleToInt64Bits(a.Real) == BitConverter.DoubleToInt64Bits(b.Real)
				&& BitConverter.DoubleToInt64Bits(a.Imaginary)
					== BitConverter.DoubleToInt64Bits(b.Imaginary);
			Sweep(
				"complex.low",
				LowSweep,
				i => new Complex(
					double.MinValue + i * DoubleStep,
					double.MinValue + (LowSweep - 1 - i) * DoubleStep
				),
				v => (Complex)(SecureComplex)v,
				bits
			);
			Sweep(
				"complex.high",
				HighSweep,
				i => new Complex(
					double.MaxValue - i * DoubleStep,
					double.MaxValue - (HighSweep - 1 - i) * DoubleStep
				),
				v => (Complex)(SecureComplex)v,
				bits
			);
			Sweep(
				"complex.mid",
				MidSweep,
				i => new Complex((i - MidHalf) * 0.5, (MidHalf - i) * 0.5),
				v => (Complex)(SecureComplex)v,
				bits
			);
		}

		[Fact]
		public void Vector2_Thoroughness()
		{
			Sweep(
				"vector2.low",
				LowSweep,
				i => new Vector2(
					float.MinValue + i * FloatStep,
					float.MinValue + (LowSweep - 1 - i) * FloatStep
				),
				v => (Vector2)(SecureVector2)v
			);
			Sweep(
				"vector2.high",
				HighSweep,
				i => new Vector2(
					float.MaxValue - i * FloatStep,
					float.MaxValue - (HighSweep - 1 - i) * FloatStep
				),
				v => (Vector2)(SecureVector2)v
			);
			Sweep(
				"vector2.mid",
				MidSweep,
				i => new Vector2((i - MidHalf) * 0.5f, (MidHalf - i) * 0.5f),
				v => (Vector2)(SecureVector2)v
			);
		}

		[Fact]
		public void Vector3_Thoroughness()
		{
			Sweep(
				"vector3.low",
				LowSweep,
				i => new Vector3(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true)
				),
				v => (Vector3)(SecureVector3)v
			);
			Sweep(
				"vector3.high",
				HighSweep,
				i => new Vector3(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false)
				),
				v => (Vector3)(SecureVector3)v
			);
			Sweep(
				"vector3.mid",
				MidSweep,
				i => new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
				v => (Vector3)(SecureVector3)v
			);
		}

		[Fact]
		public void Vector4_Thoroughness()
		{
			Sweep(
				"vector4.low",
				LowSweep,
				i => new Vector4(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Vector4)(SecureVector4)v
			);
			Sweep(
				"vector4.high",
				HighSweep,
				i => new Vector4(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Vector4)(SecureVector4)v
			);
			Sweep(
				"vector4.mid",
				MidSweep,
				i => new Vector4(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Vector4)(SecureVector4)v
			);
		}

		[Fact]
		public void Quaternion_Thoroughness()
		{
			Sweep(
				"quaternion.low",
				LowSweep,
				i => new Quaternion(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Quaternion)(SecureQuaternion)v
			);
			Sweep(
				"quaternion.high",
				HighSweep,
				i => new Quaternion(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Quaternion)(SecureQuaternion)v
			);
			Sweep(
				"quaternion.mid",
				MidSweep,
				i => new Quaternion(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Quaternion)(SecureQuaternion)v
			);
		}

		[Fact]
		public void Plane_Thoroughness()
		{
			Sweep(
				"plane.low",
				LowSweep,
				i => new Plane(
					new Vector3(
						Lane(i, 0, LowSweep, true),
						Lane(i, 1, LowSweep, true),
						Lane(i, 2, LowSweep, true)
					),
					Lane(i, 3, LowSweep, true)
				),
				v => (Plane)(SecurePlane)v
			);
			Sweep(
				"plane.high",
				HighSweep,
				i => new Plane(
					new Vector3(
						Lane(i, 0, HighSweep, false),
						Lane(i, 1, HighSweep, false),
						Lane(i, 2, HighSweep, false)
					),
					Lane(i, 3, HighSweep, false)
				),
				v => (Plane)(SecurePlane)v
			);
			Sweep(
				"plane.mid",
				MidSweep,
				i => new Plane(
					new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
					MidLane(i, 3)
				),
				v => (Plane)(SecurePlane)v
			);
		}

		[Fact]
		public void Matrix3x2_Thoroughness()
		{
			Sweep(
				"matrix3x2.low",
				LowSweep,
				i => new Matrix3x2(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true),
					Lane(i, 4, LowSweep, true),
					Lane(i, 5, LowSweep, true)
				),
				v => (Matrix3x2)(SecureMatrix3x2)v
			);
			Sweep(
				"matrix3x2.high",
				HighSweep,
				i => new Matrix3x2(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false),
					Lane(i, 4, HighSweep, false),
					Lane(i, 5, HighSweep, false)
				),
				v => (Matrix3x2)(SecureMatrix3x2)v
			);
			Sweep(
				"matrix3x2.mid",
				MidSweep,
				i => new Matrix3x2(
					MidLane(i, 0),
					MidLane(i, 1),
					MidLane(i, 2),
					MidLane(i, 3),
					MidLane(i, 4),
					MidLane(i, 5)
				),
				v => (Matrix3x2)(SecureMatrix3x2)v
			);
		}

		[Fact]
		public void Matrix4x4_Thoroughness()
		{
			Sweep(
				"matrix4x4.low",
				LowSweep,
				i => Fill4x4(LowSweep, i, true),
				v => (Matrix4x4)(SecureMatrix4x4)v
			);
			Sweep(
				"matrix4x4.high",
				HighSweep,
				i => Fill4x4(HighSweep, i, false),
				v => (Matrix4x4)(SecureMatrix4x4)v
			);
			Sweep(
				"matrix4x4.mid",
				MidSweep,
				i => new Matrix4x4(
					MidLane(i, 0),
					MidLane(i, 1),
					MidLane(i, 2),
					MidLane(i, 3),
					MidLane(i, 4),
					MidLane(i, 5),
					MidLane(i, 6),
					MidLane(i, 7),
					MidLane(i, 8),
					MidLane(i, 9),
					MidLane(i, 10),
					MidLane(i, 11),
					MidLane(i, 12),
					MidLane(i, 13),
					MidLane(i, 14),
					MidLane(i, 15)
				),
				v => (Matrix4x4)(SecureMatrix4x4)v
			);
		}

		// Lanes of multi-component values use strided indices so every lane holds a
		// distinct value (identical lanes would mask lane-swap packing bugs).
		private static float Lane(long i, int lane, long count, bool fromLow)
		{
			long j = (i + lane * (count / 16)) % count;
			return fromLow ? float.MinValue + j * FloatStep : float.MaxValue - j * FloatStep;
		}

		private static float MidLane(long i, int lane)
		{
			// Exact 0.5 multiples (all magnitudes stay far below 2^24), strided per
			// lane so lanes never share a value.
			return (i - MidHalf + lane * (MidSweep / 16)) * 0.5f;
		}

		private static Matrix4x4 Fill4x4(long count, long i, bool fromLow)
		{
			return new Matrix4x4(
				Lane(i, 0, count, fromLow),
				Lane(i, 1, count, fromLow),
				Lane(i, 2, count, fromLow),
				Lane(i, 3, count, fromLow),
				Lane(i, 4, count, fromLow),
				Lane(i, 5, count, fromLow),
				Lane(i, 6, count, fromLow),
				Lane(i, 7, count, fromLow),
				Lane(i, 8, count, fromLow),
				Lane(i, 9, count, fromLow),
				Lane(i, 10, count, fromLow),
				Lane(i, 11, count, fromLow),
				Lane(i, 12, count, fromLow),
				Lane(i, 13, count, fromLow),
				Lane(i, 14, count, fromLow),
				Lane(i, 15, count, fromLow)
			);
		}

		private static DateTimeKind Kind(long i) => (DateTimeKind)(i % 3);

		private static Guid GuidFromULong(ulong low, byte fill)
		{
			byte[] bytes = BitConverter.GetBytes(low);
			byte[] guid = new byte[16];
			Buffer.BlockCopy(bytes, 0, guid, 0, 8);
			for (int k = 8; k < 16; k++)
			{
				guid[k] = fill;
			}
			return new Guid(guid);
		}

		private static void Sweep<T>(string name, long count, Func<long, T> make, Func<T, T> read)
		{
			Sweep(name, count, make, read, EqualityComparer<T>.Default.Equals);
		}

		private static void Sweep<T>(
			string name,
			long count,
			Func<long, T> make,
			Func<T, T> read,
			Func<T, T, bool> equal
		)
		{
			long failures = 0;
			string first = "";
			for (long i = 0; i < count; i++)
			{
				T value = make(i);
				if (!equal(value, read(value)))
				{
					if (failures == 0)
					{
						first = $"{name}[{i}] = {value}";
					}
					failures++;
				}
			}
			Assert.True(failures == 0, $"{name}: {failures}/{count} mismatched, first: {first}");
		}
	}
}
