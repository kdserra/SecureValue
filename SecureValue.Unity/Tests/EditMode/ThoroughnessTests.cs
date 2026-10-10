#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode range-sweep round-trip coverage for every wrapper type Unity can serialize:
	/// 1M values up from the lowest, 1M down from the highest, and 5M around zero (0..5M
	/// for unsigned-only ranges) — 7M per full-range type. Types whose whole range is
	/// smaller are swept EXHAUSTIVELY instead: byte/sbyte (256), short/ushort/char (65536),
	/// bool (2). SecureString and SecureBigInteger use scaled counts (70K and 700K): they
	/// allocate per operation. Lanes of multi-component values use strided indices so no
	/// two lanes share a value (identical lanes would mask lane-swap packing bugs).
	/// Unity-family and Numerics-family types share simple names, so both are FULLY
	/// QUALIFIED below — never rely on usings here.
	/// </summary>
	public class ThoroughnessTests
	{
		private const long LowSweep = 1_000_000;
		private const long HighSweep = 1_000_000;
		private const long MidSweep = 5_000_000;
		private const long MidHalf = MidSweep / 2;

		private const long StringLow = 10_000;
		private const long StringHigh = 10_000;
		private const long StringMid = 50_000;
		private const long BigLow = 100_000;
		private const long BigHigh = 100_000;
		private const long BigMid = 500_000;

		private static readonly float FloatStep = float.MaxValue / LowSweep;
		private static readonly double DoubleStep = double.MaxValue / LowSweep;
		private static readonly System.Numerics.BigInteger BigHuge = System.Numerics.BigInteger.Pow(
			2,
			256
		);
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

		[Test]
		public void Bool_Thoroughness()
		{
			Sweep("bool", 2, i => i == 1, v => (bool)(SecureBool)v);
		}

		[Test]
		public void Byte_Thoroughness()
		{
			Sweep("byte", 256, i => (byte)i, v => (byte)(SecureByte)v);
		}

		[Test]
		public void SByte_Thoroughness()
		{
			Sweep("sbyte", 256, i => unchecked((sbyte)(i - 128)), v => (sbyte)(SecureSByte)v);
		}

		[Test]
		public void Char_Thoroughness()
		{
			Sweep("char", 65536, i => (char)i, v => (char)(SecureChar)v);
		}

		[Test]
		public void Short_Thoroughness()
		{
			Sweep("short", 65536, i => unchecked((short)(i - 32768)), v => (short)(SecureShort)v);
		}

		[Test]
		public void UShort_Thoroughness()
		{
			Sweep("ushort", 65536, i => (ushort)i, v => (ushort)(SecureUShort)v);
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void Vector2_Thoroughness()
		{
			Sweep(
				"vector2.low",
				LowSweep,
				i => new System.Numerics.Vector2(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true)
				),
				v => (System.Numerics.Vector2)(SecureValue.Numerics.SecureVector2)v
			);
			Sweep(
				"vector2.high",
				HighSweep,
				i => new System.Numerics.Vector2(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false)
				),
				v => (System.Numerics.Vector2)(SecureValue.Numerics.SecureVector2)v
			);
			Sweep(
				"vector2.mid",
				MidSweep,
				i => new System.Numerics.Vector2(MidLane(i, 0), MidLane(i, 1)),
				v => (System.Numerics.Vector2)(SecureValue.Numerics.SecureVector2)v
			);
		}

		[Test]
		public void Vector3_Thoroughness()
		{
			Sweep(
				"vector3.low",
				LowSweep,
				i => new System.Numerics.Vector3(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true)
				),
				v => (System.Numerics.Vector3)(SecureValue.Numerics.SecureVector3)v
			);
			Sweep(
				"vector3.high",
				HighSweep,
				i => new System.Numerics.Vector3(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false)
				),
				v => (System.Numerics.Vector3)(SecureValue.Numerics.SecureVector3)v
			);
			Sweep(
				"vector3.mid",
				MidSweep,
				i => new System.Numerics.Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
				v => (System.Numerics.Vector3)(SecureValue.Numerics.SecureVector3)v
			);
		}

		[Test]
		public void Vector4_Thoroughness()
		{
			Sweep(
				"vector4.low",
				LowSweep,
				i => new System.Numerics.Vector4(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (System.Numerics.Vector4)(SecureValue.Numerics.SecureVector4)v
			);
			Sweep(
				"vector4.high",
				HighSweep,
				i => new System.Numerics.Vector4(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (System.Numerics.Vector4)(SecureValue.Numerics.SecureVector4)v
			);
			Sweep(
				"vector4.mid",
				MidSweep,
				i => new System.Numerics.Vector4(
					MidLane(i, 0),
					MidLane(i, 1),
					MidLane(i, 2),
					MidLane(i, 3)
				),
				v => (System.Numerics.Vector4)(SecureValue.Numerics.SecureVector4)v
			);
		}

		[Test]
		public void Quaternion_Thoroughness()
		{
			Sweep(
				"quaternion.low",
				LowSweep,
				i => new System.Numerics.Quaternion(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (System.Numerics.Quaternion)(SecureValue.Numerics.SecureQuaternion)v
			);
			Sweep(
				"quaternion.high",
				HighSweep,
				i => new System.Numerics.Quaternion(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (System.Numerics.Quaternion)(SecureValue.Numerics.SecureQuaternion)v
			);
			Sweep(
				"quaternion.mid",
				MidSweep,
				i => new System.Numerics.Quaternion(
					MidLane(i, 0),
					MidLane(i, 1),
					MidLane(i, 2),
					MidLane(i, 3)
				),
				v => (System.Numerics.Quaternion)(SecureValue.Numerics.SecureQuaternion)v
			);
		}

		[Test]
		public void Plane_Thoroughness()
		{
			Sweep(
				"plane.low",
				LowSweep,
				i => new System.Numerics.Plane(
					new System.Numerics.Vector3(
						Lane(i, 0, LowSweep, true),
						Lane(i, 1, LowSweep, true),
						Lane(i, 2, LowSweep, true)
					),
					Lane(i, 3, LowSweep, true)
				),
				v => (System.Numerics.Plane)(SecureValue.Numerics.SecurePlane)v
			);
			Sweep(
				"plane.high",
				HighSweep,
				i => new System.Numerics.Plane(
					new System.Numerics.Vector3(
						Lane(i, 0, HighSweep, false),
						Lane(i, 1, HighSweep, false),
						Lane(i, 2, HighSweep, false)
					),
					Lane(i, 3, HighSweep, false)
				),
				v => (System.Numerics.Plane)(SecureValue.Numerics.SecurePlane)v
			);
			Sweep(
				"plane.mid",
				MidSweep,
				i => new System.Numerics.Plane(
					new System.Numerics.Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
					MidLane(i, 3)
				),
				v => (System.Numerics.Plane)(SecureValue.Numerics.SecurePlane)v
			);
		}

		[Test]
		public void Matrix3x2_Thoroughness()
		{
			Sweep(
				"matrix3x2.low",
				LowSweep,
				i => new System.Numerics.Matrix3x2(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true),
					Lane(i, 4, LowSweep, true),
					Lane(i, 5, LowSweep, true)
				),
				v => (System.Numerics.Matrix3x2)(SecureValue.Numerics.SecureMatrix3x2)v
			);
			Sweep(
				"matrix3x2.high",
				HighSweep,
				i => new System.Numerics.Matrix3x2(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false),
					Lane(i, 4, HighSweep, false),
					Lane(i, 5, HighSweep, false)
				),
				v => (System.Numerics.Matrix3x2)(SecureValue.Numerics.SecureMatrix3x2)v
			);
			Sweep(
				"matrix3x2.mid",
				MidSweep,
				i => new System.Numerics.Matrix3x2(
					MidLane(i, 0),
					MidLane(i, 1),
					MidLane(i, 2),
					MidLane(i, 3),
					MidLane(i, 4),
					MidLane(i, 5)
				),
				v => (System.Numerics.Matrix3x2)(SecureValue.Numerics.SecureMatrix3x2)v
			);
		}

		[Test]
		public void Matrix4x4_Thoroughness()
		{
			Sweep(
				"matrix4x4.low",
				LowSweep,
				i => FillNumerics(LowSweep, i, true),
				v => (System.Numerics.Matrix4x4)(SecureValue.Numerics.SecureMatrix4x4)v
			);
			Sweep(
				"matrix4x4.high",
				HighSweep,
				i => FillNumerics(HighSweep, i, false),
				v => (System.Numerics.Matrix4x4)(SecureValue.Numerics.SecureMatrix4x4)v
			);
			Sweep(
				"matrix4x4.mid",
				MidSweep,
				i => new System.Numerics.Matrix4x4(
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
				v => (System.Numerics.Matrix4x4)(SecureValue.Numerics.SecureMatrix4x4)v
			);
		}

		[Test]
		public void Complex_Thoroughness()
		{
			Func<System.Numerics.Complex, System.Numerics.Complex, bool> bits = (a, b) =>
				BitConverter.DoubleToInt64Bits(a.Real) == BitConverter.DoubleToInt64Bits(b.Real)
				&& BitConverter.DoubleToInt64Bits(a.Imaginary)
					== BitConverter.DoubleToInt64Bits(b.Imaginary);
			Sweep(
				"complex.low",
				LowSweep,
				i => new System.Numerics.Complex(
					double.MinValue + i * DoubleStep,
					double.MinValue + (LowSweep - 1 - i) * DoubleStep
				),
				v => (System.Numerics.Complex)(SecureValue.Numerics.SecureComplex)v,
				bits
			);
			Sweep(
				"complex.high",
				HighSweep,
				i => new System.Numerics.Complex(
					double.MaxValue - i * DoubleStep,
					double.MaxValue - (HighSweep - 1 - i) * DoubleStep
				),
				v => (System.Numerics.Complex)(SecureValue.Numerics.SecureComplex)v,
				bits
			);
			Sweep(
				"complex.mid",
				MidSweep,
				i => new System.Numerics.Complex((i - MidHalf) * 0.5, (MidHalf - i) * 0.5),
				v => (System.Numerics.Complex)(SecureValue.Numerics.SecureComplex)v,
				bits
			);
		}

		[Test]
		public void BigInteger_Thoroughness()
		{
			Sweep(
				"bigint.low",
				BigLow,
				i => new System.Numerics.BigInteger(i),
				v => (System.Numerics.BigInteger)(SecureValue.Numerics.SecureBigInteger)v
			);
			Sweep(
				"bigint.high",
				BigHigh,
				i => BigHuge - i,
				v => (System.Numerics.BigInteger)(SecureValue.Numerics.SecureBigInteger)v
			);
			Sweep(
				"bigint.mid",
				BigMid,
				i => new System.Numerics.BigInteger(i - BigMid / 2),
				v => (System.Numerics.BigInteger)(SecureValue.Numerics.SecureBigInteger)v
			);
		}

		[Test]
		public void UnityVector2_Thoroughness()
		{
			Sweep(
				"uvector2.low",
				LowSweep,
				i => new Vector2(Lane(i, 0, LowSweep, true), Lane(i, 1, LowSweep, true)),
				v => (Vector2)(SecureValue.Unity.SecureVector2)v
			);
			Sweep(
				"uvector2.high",
				HighSweep,
				i => new Vector2(Lane(i, 0, HighSweep, false), Lane(i, 1, HighSweep, false)),
				v => (Vector2)(SecureValue.Unity.SecureVector2)v
			);
			Sweep(
				"uvector2.mid",
				MidSweep,
				i => new Vector2(MidLane(i, 0), MidLane(i, 1)),
				v => (Vector2)(SecureValue.Unity.SecureVector2)v
			);
		}

		[Test]
		public void UnityVector2Int_Thoroughness()
		{
			Sweep(
				"uvector2int.low",
				LowSweep,
				i => new Vector2Int(IntLane(i, 0, LowSweep, true), IntLane(i, 1, LowSweep, true)),
				v => (Vector2Int)(SecureValue.Unity.SecureVector2Int)v
			);
			Sweep(
				"uvector2int.high",
				HighSweep,
				i => new Vector2Int(
					IntLane(i, 0, HighSweep, false),
					IntLane(i, 1, HighSweep, false)
				),
				v => (Vector2Int)(SecureValue.Unity.SecureVector2Int)v
			);
			Sweep(
				"uvector2int.mid",
				MidSweep,
				i => new Vector2Int(IntMidLane(i, 0), IntMidLane(i, 1)),
				v => (Vector2Int)(SecureValue.Unity.SecureVector2Int)v
			);
		}

		[Test]
		public void UnityVector3_Thoroughness()
		{
			Sweep(
				"uvector3.low",
				LowSweep,
				i => new Vector3(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true)
				),
				v => (Vector3)(SecureValue.Unity.SecureVector3)v
			);
			Sweep(
				"uvector3.high",
				HighSweep,
				i => new Vector3(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false)
				),
				v => (Vector3)(SecureValue.Unity.SecureVector3)v
			);
			Sweep(
				"uvector3.mid",
				MidSweep,
				i => new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
				v => (Vector3)(SecureValue.Unity.SecureVector3)v
			);
		}

		[Test]
		public void UnityVector3Int_Thoroughness()
		{
			Sweep(
				"uvector3int.low",
				LowSweep,
				i => new Vector3Int(
					IntLane(i, 0, LowSweep, true),
					IntLane(i, 1, LowSweep, true),
					IntLane(i, 2, LowSweep, true)
				),
				v => (Vector3Int)(SecureValue.Unity.SecureVector3Int)v
			);
			Sweep(
				"uvector3int.high",
				HighSweep,
				i => new Vector3Int(
					IntLane(i, 0, HighSweep, false),
					IntLane(i, 1, HighSweep, false),
					IntLane(i, 2, HighSweep, false)
				),
				v => (Vector3Int)(SecureValue.Unity.SecureVector3Int)v
			);
			Sweep(
				"uvector3int.mid",
				MidSweep,
				i => new Vector3Int(IntMidLane(i, 0), IntMidLane(i, 1), IntMidLane(i, 2)),
				v => (Vector3Int)(SecureValue.Unity.SecureVector3Int)v
			);
		}

		[Test]
		public void UnityVector4_Thoroughness()
		{
			Sweep(
				"uvector4.low",
				LowSweep,
				i => new Vector4(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Vector4)(SecureValue.Unity.SecureVector4)v
			);
			Sweep(
				"uvector4.high",
				HighSweep,
				i => new Vector4(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Vector4)(SecureValue.Unity.SecureVector4)v
			);
			Sweep(
				"uvector4.mid",
				MidSweep,
				i => new Vector4(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Vector4)(SecureValue.Unity.SecureVector4)v
			);
		}

		[Test]
		public void UnityRect_Thoroughness()
		{
			Sweep(
				"urect.low",
				LowSweep,
				i => new Rect(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Rect)(SecureValue.Unity.SecureRect)v
			);
			Sweep(
				"urect.high",
				HighSweep,
				i => new Rect(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Rect)(SecureValue.Unity.SecureRect)v
			);
			Sweep(
				"urect.mid",
				MidSweep,
				i => new Rect(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Rect)(SecureValue.Unity.SecureRect)v
			);
		}

		[Test]
		public void UnityRectInt_Thoroughness()
		{
			Sweep(
				"urectint.low",
				LowSweep,
				i => new RectInt(
					IntLane(i, 0, LowSweep, true),
					IntLane(i, 1, LowSweep, true),
					IntLane(i, 2, LowSweep, true),
					IntLane(i, 3, LowSweep, true)
				),
				v => (RectInt)(SecureValue.Unity.SecureRectInt)v
			);
			Sweep(
				"urectint.high",
				HighSweep,
				i => new RectInt(
					IntLane(i, 0, HighSweep, false),
					IntLane(i, 1, HighSweep, false),
					IntLane(i, 2, HighSweep, false),
					IntLane(i, 3, HighSweep, false)
				),
				v => (RectInt)(SecureValue.Unity.SecureRectInt)v
			);
			Sweep(
				"urectint.mid",
				MidSweep,
				i => new RectInt(
					IntMidLane(i, 0),
					IntMidLane(i, 1),
					IntMidLane(i, 2),
					IntMidLane(i, 3)
				),
				v => (RectInt)(SecureValue.Unity.SecureRectInt)v
			);
		}

		[Test]
		public void UnityBounds_Thoroughness()
		{
			Sweep(
				"ubounds.low",
				LowSweep,
				i => new Bounds(
					new Vector3(
						Lane(i, 0, LowSweep, true),
						Lane(i, 1, LowSweep, true),
						Lane(i, 2, LowSweep, true)
					),
					new Vector3(
						Lane(i, 3, LowSweep, true),
						Lane(i, 4, LowSweep, true),
						Lane(i, 5, LowSweep, true)
					)
				),
				v => (Bounds)(SecureValue.Unity.SecureBounds)v
			);
			Sweep(
				"ubounds.high",
				HighSweep,
				i => new Bounds(
					new Vector3(
						Lane(i, 0, HighSweep, false),
						Lane(i, 1, HighSweep, false),
						Lane(i, 2, HighSweep, false)
					),
					new Vector3(
						Lane(i, 3, HighSweep, false),
						Lane(i, 4, HighSweep, false),
						Lane(i, 5, HighSweep, false)
					)
				),
				v => (Bounds)(SecureValue.Unity.SecureBounds)v
			);
			Sweep(
				"ubounds.mid",
				MidSweep,
				i => new Bounds(
					new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
					new Vector3(MidLane(i, 3), MidLane(i, 4), MidLane(i, 5))
				),
				v => (Bounds)(SecureValue.Unity.SecureBounds)v
			);
		}

		[Test]
		public void UnityBoundsInt_Thoroughness()
		{
			Sweep(
				"uboundsint.low",
				LowSweep,
				i => new BoundsInt(
					new Vector3Int(
						IntLane(i, 0, LowSweep, true),
						IntLane(i, 1, LowSweep, true),
						IntLane(i, 2, LowSweep, true)
					),
					new Vector3Int(
						IntLane(i, 3, LowSweep, true),
						IntLane(i, 4, LowSweep, true),
						IntLane(i, 5, LowSweep, true)
					)
				),
				v => (BoundsInt)(SecureValue.Unity.SecureBoundsInt)v
			);
			Sweep(
				"uboundsint.high",
				HighSweep,
				i => new BoundsInt(
					new Vector3Int(
						IntLane(i, 0, HighSweep, false),
						IntLane(i, 1, HighSweep, false),
						IntLane(i, 2, HighSweep, false)
					),
					new Vector3Int(
						IntLane(i, 3, HighSweep, false),
						IntLane(i, 4, HighSweep, false),
						IntLane(i, 5, HighSweep, false)
					)
				),
				v => (BoundsInt)(SecureValue.Unity.SecureBoundsInt)v
			);
			Sweep(
				"uboundsint.mid",
				MidSweep,
				i => new BoundsInt(
					new Vector3Int(IntMidLane(i, 0), IntMidLane(i, 1), IntMidLane(i, 2)),
					new Vector3Int(IntMidLane(i, 3), IntMidLane(i, 4), IntMidLane(i, 5))
				),
				v => (BoundsInt)(SecureValue.Unity.SecureBoundsInt)v
			);
		}

		[Test]
		public void UnityColor_Thoroughness()
		{
			Sweep(
				"ucolor.low",
				LowSweep,
				i => new Color(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Color)(SecureValue.Unity.SecureColor)v
			);
			Sweep(
				"ucolor.high",
				HighSweep,
				i => new Color(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Color)(SecureValue.Unity.SecureColor)v
			);
			Sweep(
				"ucolor.mid",
				MidSweep,
				i => new Color(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Color)(SecureValue.Unity.SecureColor)v
			);
		}

		[Test]
		public void UnityColor32_Thoroughness()
		{
			Sweep(
				"ucolor32.low",
				LowSweep,
				i => new Color32(ByteLane(i, 0), ByteLane(i, 1), ByteLane(i, 2), ByteLane(i, 3)),
				v => (Color32)(SecureValue.Unity.SecureColor32)v
			);
			Sweep(
				"ucolor32.high",
				HighSweep,
				i => new Color32(ByteLane(i, 0), ByteLane(i, 1), ByteLane(i, 2), ByteLane(i, 3)),
				v => (Color32)(SecureValue.Unity.SecureColor32)v
			);
			Sweep(
				"ucolor32.mid",
				MidSweep,
				i => new Color32(ByteLane(i, 0), ByteLane(i, 1), ByteLane(i, 2), ByteLane(i, 3)),
				v => (Color32)(SecureValue.Unity.SecureColor32)v
			);
		}

		[Test]
		public void UnityQuaternion_Thoroughness()
		{
			Sweep(
				"uquaternion.low",
				LowSweep,
				i => new Quaternion(
					Lane(i, 0, LowSweep, true),
					Lane(i, 1, LowSweep, true),
					Lane(i, 2, LowSweep, true),
					Lane(i, 3, LowSweep, true)
				),
				v => (Quaternion)(SecureValue.Unity.SecureQuaternion)v
			);
			Sweep(
				"uquaternion.high",
				HighSweep,
				i => new Quaternion(
					Lane(i, 0, HighSweep, false),
					Lane(i, 1, HighSweep, false),
					Lane(i, 2, HighSweep, false),
					Lane(i, 3, HighSweep, false)
				),
				v => (Quaternion)(SecureValue.Unity.SecureQuaternion)v
			);
			Sweep(
				"uquaternion.mid",
				MidSweep,
				i => new Quaternion(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
				v => (Quaternion)(SecureValue.Unity.SecureQuaternion)v
			);
		}

		[Test]
		public void UnityMatrix4x4_Thoroughness()
		{
			Sweep(
				"umatrix4x4.low",
				LowSweep,
				i => FillUnity(LowSweep, i, true),
				v => (Matrix4x4)(SecureValue.Unity.SecureMatrix4x4)v
			);
			Sweep(
				"umatrix4x4.high",
				HighSweep,
				i => FillUnity(HighSweep, i, false),
				v => (Matrix4x4)(SecureValue.Unity.SecureMatrix4x4)v
			);
			Sweep(
				"umatrix4x4.mid",
				MidSweep,
				i => new Matrix4x4(
					new Vector4(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2), MidLane(i, 3)),
					new Vector4(MidLane(i, 4), MidLane(i, 5), MidLane(i, 6), MidLane(i, 7)),
					new Vector4(MidLane(i, 8), MidLane(i, 9), MidLane(i, 10), MidLane(i, 11)),
					new Vector4(MidLane(i, 12), MidLane(i, 13), MidLane(i, 14), MidLane(i, 15))
				),
				v => (Matrix4x4)(SecureValue.Unity.SecureMatrix4x4)v
			);
		}

		[Test]
		public void UnityPlane_Thoroughness()
		{
			Sweep(
				"uplane.low",
				LowSweep,
				i => new Plane(
					new Vector3(
						Lane(i, 0, LowSweep, true),
						Lane(i, 1, LowSweep, true),
						Lane(i, 2, LowSweep, true)
					),
					Lane(i, 3, LowSweep, true)
				),
				v => (Plane)(SecureValue.Unity.SecurePlane)v
			);
			Sweep(
				"uplane.high",
				HighSweep,
				i => new Plane(
					new Vector3(
						Lane(i, 0, HighSweep, false),
						Lane(i, 1, HighSweep, false),
						Lane(i, 2, HighSweep, false)
					),
					Lane(i, 3, HighSweep, false)
				),
				v => (Plane)(SecureValue.Unity.SecurePlane)v
			);
			Sweep(
				"uplane.mid",
				MidSweep,
				i => new Plane(
					new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
					MidLane(i, 3)
				),
				v => (Plane)(SecureValue.Unity.SecurePlane)v
			);
		}

		[Test]
		public void UnityRay_Thoroughness()
		{
			Sweep(
				"uray.low",
				LowSweep,
				i => new Ray(
					new Vector3(
						Lane(i, 0, LowSweep, true),
						Lane(i, 1, LowSweep, true),
						Lane(i, 2, LowSweep, true)
					),
					new Vector3(
						Lane(i, 3, LowSweep, true),
						Lane(i, 4, LowSweep, true),
						Lane(i, 5, LowSweep, true)
					)
				),
				v => (Ray)(SecureValue.Unity.SecureRay)v
			);
			Sweep(
				"uray.high",
				HighSweep,
				i => new Ray(
					new Vector3(
						Lane(i, 0, HighSweep, false),
						Lane(i, 1, HighSweep, false),
						Lane(i, 2, HighSweep, false)
					),
					new Vector3(
						Lane(i, 3, HighSweep, false),
						Lane(i, 4, HighSweep, false),
						Lane(i, 5, HighSweep, false)
					)
				),
				v => (Ray)(SecureValue.Unity.SecureRay)v
			);
			Sweep(
				"uray.mid",
				MidSweep,
				i => new Ray(
					new Vector3(MidLane(i, 0), MidLane(i, 1), MidLane(i, 2)),
					new Vector3(MidLane(i, 3), MidLane(i, 4), MidLane(i, 5))
				),
				v => (Ray)(SecureValue.Unity.SecureRay)v
			);
		}

		[Test]
		public void UnityLayerMask_Thoroughness()
		{
			Sweep(
				"ulayermask.low",
				LowSweep,
				i => unchecked((int)i),
				v =>
					(
						(LayerMask)(SecureValue.Unity.SecureLayerMask)new LayerMask() { value = v }
					).value
			);
			Sweep(
				"ulayermask.high",
				HighSweep,
				i => unchecked(int.MaxValue - (int)i),
				v =>
					(
						(LayerMask)(SecureValue.Unity.SecureLayerMask)new LayerMask() { value = v }
					).value
			);
			Sweep(
				"ulayermask.mid",
				MidSweep,
				i => unchecked((int)(i - MidHalf)),
				v =>
					(
						(LayerMask)(SecureValue.Unity.SecureLayerMask)new LayerMask() { value = v }
					).value
			);
		}

		private static float Lane(long i, int lane, long count, bool fromLow)
		{
			long j = (i + lane * (count / 16)) % count;
			return fromLow ? float.MinValue + j * FloatStep : float.MaxValue - j * FloatStep;
		}

		private static float MidLane(long i, int lane)
		{
			return (i - MidHalf + lane * (MidSweep / 16)) * 0.5f;
		}

		private static int IntLane(long i, int lane, long count, bool fromLow)
		{
			long j = (i + lane * (count / 16)) % count;
			return fromLow ? unchecked(int.MinValue + (int)j) : unchecked(int.MaxValue - (int)j);
		}

		private static int IntMidLane(long i, int lane)
		{
			return unchecked((int)(i - MidHalf + lane * (MidSweep / 16)));
		}

		private static byte ByteLane(long i, int lane)
		{
			return (byte)((i + lane * 61) % 256);
		}

		private static System.Numerics.Matrix4x4 FillNumerics(long count, long i, bool fromLow)
		{
			return new System.Numerics.Matrix4x4(
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

		private static Matrix4x4 FillUnity(long count, long i, bool fromLow)
		{
			return new Matrix4x4(
				new Vector4(
					Lane(i, 0, count, fromLow),
					Lane(i, 1, count, fromLow),
					Lane(i, 2, count, fromLow),
					Lane(i, 3, count, fromLow)
				),
				new Vector4(
					Lane(i, 4, count, fromLow),
					Lane(i, 5, count, fromLow),
					Lane(i, 6, count, fromLow),
					Lane(i, 7, count, fromLow)
				),
				new Vector4(
					Lane(i, 8, count, fromLow),
					Lane(i, 9, count, fromLow),
					Lane(i, 10, count, fromLow),
					Lane(i, 11, count, fromLow)
				),
				new Vector4(
					Lane(i, 12, count, fromLow),
					Lane(i, 13, count, fromLow),
					Lane(i, 14, count, fromLow),
					Lane(i, 15, count, fromLow)
				)
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
#endif
