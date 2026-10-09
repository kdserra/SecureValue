#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode random round-trip coverage for every wrapper type Unity can serialize:
	/// roughly 10M values (each test logs its count) drawn from the full bit space.
	/// The seed is time-derived so every run tests different values, and it is ALWAYS
	/// logged — with any failing values — so a failure can be reproduced by re-seeding a
	/// Random with the printed seed and regenerating the same sequence. Multi-component
	/// aggregates mask NaN lanes to 0: struct Equals cannot compare NaN, and NaN payload
	/// preservation is already covered bit-exactly by the extreme suites. No dynamic
	/// (unavailable under IL2CPP/AOT): every conversion is statically typed. Unity-family
	/// and Numerics-family types share simple names, so both are FULLY QUALIFIED below.
	/// </summary>
	public class RandomThoroughnessTests
	{
		private static readonly int RunSeed = unchecked((int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF));

		private static int Seed(int index) => unchecked(RunSeed * 397 ^ index);

		[Test]
		public void Bool_RandomThoroughness()
		{
			int seed = Seed(1);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 20_000;
			for (int n = 0; n < count; n++)
			{
				bool value = random.Next(2) == 0;
				SecureBool p = value;
				Sample(value, (bool)p, "bool", ref failures, samples);
			}
			Report("bool", seed, count, failures, samples);
		}

		[Test]
		public void Byte_RandomThoroughness()
		{
			int seed = Seed(2);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			byte[] buf = new byte[1];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				byte value = buf[0];
				SecureByte p = value;
				Sample(value, (byte)p, "byte", ref failures, samples);
			}
			Report("byte", seed, count, failures, samples);
		}

		[Test]
		public void SByte_RandomThoroughness()
		{
			int seed = Seed(3);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			byte[] buf = new byte[1];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				sbyte value = unchecked((sbyte)buf[0]);
				SecureSByte p = value;
				Sample(value, (sbyte)p, "sbyte", ref failures, samples);
			}
			Report("sbyte", seed, count, failures, samples);
		}

		[Test]
		public void Char_RandomThoroughness()
		{
			int seed = Seed(4);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			for (int n = 0; n < count; n++)
			{
				char value = (char)random.Next(0x10000);
				SecureChar p = value;
				Sample(value, (char)p, "char", ref failures, samples);
			}
			Report("char", seed, count, failures, samples);
		}

		[Test]
		public void Short_RandomThoroughness()
		{
			int seed = Seed(5);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			byte[] buf = new byte[2];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				short value = BitConverter.ToInt16(buf, 0);
				SecureShort p = value;
				Sample(value, (short)p, "short", ref failures, samples);
			}
			Report("short", seed, count, failures, samples);
		}

		[Test]
		public void UShort_RandomThoroughness()
		{
			int seed = Seed(6);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			byte[] buf = new byte[2];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				ushort value = BitConverter.ToUInt16(buf, 0);
				SecureUShort p = value;
				Sample(value, (ushort)p, "ushort", ref failures, samples);
			}
			Report("ushort", seed, count, failures, samples);
		}

		[Test]
		public void Int_RandomThoroughness()
		{
			int seed = Seed(7);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			for (int n = 0; n < count; n++)
			{
				int value = random.Next(int.MinValue, int.MaxValue);
				SecureInt p = value;
				Sample(value, (int)p, "int", ref failures, samples);
			}
			Report("int", seed, count, failures, samples);
		}

		[Test]
		public void UInt_RandomThoroughness()
		{
			int seed = Seed(8);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				uint value = BitConverter.ToUInt32(buf, 0);
				SecureUInt p = value;
				Sample(value, (uint)p, "uint", ref failures, samples);
			}
			Report("uint", seed, count, failures, samples);
		}

		[Test]
		public void Long_RandomThoroughness()
		{
			int seed = Seed(9);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				long value = BitConverter.ToInt64(buf, 0);
				SecureLong p = value;
				Sample(value, (long)p, "long", ref failures, samples);
			}
			Report("long", seed, count, failures, samples);
		}

		[Test]
		public void ULong_RandomThoroughness()
		{
			int seed = Seed(10);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				ulong value = BitConverter.ToUInt64(buf, 0);
				SecureULong p = value;
				Sample(value, (ulong)p, "ulong", ref failures, samples);
			}
			Report("ulong", seed, count, failures, samples);
		}

		[Test]
		public void Float_RandomThoroughness()
		{
			int seed = Seed(11);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				float value = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(buf, 0));
				SecureFloat p = value;
				SampleBits(
					BitConverter.SingleToInt32Bits(value),
					BitConverter.SingleToInt32Bits((float)p),
					"float",
					value.ToString(),
					ref failures,
					samples
				);
			}
			Report("float", seed, count, failures, samples);
		}

		[Test]
		public void Double_RandomThoroughness()
		{
			int seed = Seed(12);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 600_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				double value = BitConverter.Int64BitsToDouble(BitConverter.ToInt64(buf, 0));
				SecureDouble p = value;
				SampleBits(
					BitConverter.DoubleToInt64Bits(value),
					BitConverter.DoubleToInt64Bits((double)p),
					"double",
					value.ToString(),
					ref failures,
					samples
				);
			}
			Report("double", seed, count, failures, samples);
		}

		[Test]
		public void Decimal_RandomThoroughness()
		{
			int seed = Seed(13);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			for (int n = 0; n < count; n++)
			{
				var value = new decimal(
					random.Next(int.MinValue, int.MaxValue),
					random.Next(int.MinValue, int.MaxValue),
					random.Next(int.MinValue, int.MaxValue),
					random.Next(2) == 0,
					(byte)random.Next(29)
				);
				SecureDecimal p = value;
				Sample(value, (decimal)p, "decimal", ref failures, samples);
			}
			Report("decimal", seed, count, failures, samples);
		}

		[Test]
		public void String_RandomThoroughness()
		{
			int seed = Seed(14);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				string value = RandomString(random);
				SecureString p = value;
				Sample(value, (string?)p, "string", ref failures, samples);
			}
			Report("string", seed, count, failures, samples);
		}

		[Test]
		public void Guid_RandomThoroughness()
		{
			int seed = Seed(15);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			byte[] buf = new byte[16];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				var value = new Guid(buf);
				SecureGuid p = value;
				Sample(value, (Guid)p, "guid", ref failures, samples);
			}
			Report("guid", seed, count, failures, samples);
		}

		[Test]
		public void DateTime_RandomThoroughness()
		{
			int seed = Seed(16);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				long ticks = BitConverter.ToInt64(buf, 0) & 0x3FFFFFFFFFFFFFFFL;
				if (ticks > DateTime.MaxValue.Ticks)
				{
					ticks %= (DateTime.MaxValue.Ticks + 1);
				}
				var value = new DateTime(ticks, (DateTimeKind)random.Next(3));
				SecureDateTime p = value;
				Sample(value, (DateTime)p, "datetime", ref failures, samples);
			}
			Report("datetime", seed, count, failures, samples);
		}

		[Test]
		public void DateTimeOffset_RandomThoroughness()
		{
			int seed = Seed(17);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 300_000;
			int[] offsets = { 0, 60, -60, 330, -330, 840, -840 };
			long edge = TimeSpan.FromHours(14).Ticks;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				long span = DateTimeOffset.MaxValue.Ticks - edge - edge;
				long ticks =
					edge + (BitConverter.ToInt64(buf, 0) & 0x7FFFFFFFFFFFFFFFL) % (span + 1);
				var value = new DateTimeOffset(
					ticks,
					TimeSpan.FromMinutes(offsets[random.Next(offsets.Length)])
				);
				SecureDateTimeOffset p = value;
				Sample(value, (DateTimeOffset)p, "dateoffset", ref failures, samples);
			}
			Report("dateoffset", seed, count, failures, samples);
		}

		[Test]
		public void TimeSpan_RandomThoroughness()
		{
			int seed = Seed(18);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				var value = new TimeSpan(BitConverter.ToInt64(buf, 0));
				SecureTimeSpan p = value;
				Sample(value, (TimeSpan)p, "timespan", ref failures, samples);
			}
			Report("timespan", seed, count, failures, samples);
		}

		[Test]
		public void Vector2_RandomThoroughness()
		{
			int seed = Seed(19);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Vector2(random.NextFloat(), random.NextFloat());
				SecureValue.Numerics.SecureVector2 p = value;
				Sample(value, (System.Numerics.Vector2)p, "vector2", ref failures, samples);
			}
			Report("vector2", seed, count, failures, samples);
		}

		[Test]
		public void Vector3_RandomThoroughness()
		{
			int seed = Seed(20);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Vector3(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Numerics.SecureVector3 p = value;
				Sample(value, (System.Numerics.Vector3)p, "vector3", ref failures, samples);
			}
			Report("vector3", seed, count, failures, samples);
		}

		[Test]
		public void Vector4_RandomThoroughness()
		{
			int seed = Seed(21);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Vector4(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Numerics.SecureVector4 p = value;
				Sample(value, (System.Numerics.Vector4)p, "vector4", ref failures, samples);
			}
			Report("vector4", seed, count, failures, samples);
		}

		[Test]
		public void Quaternion_RandomThoroughness()
		{
			int seed = Seed(22);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Quaternion(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Numerics.SecureQuaternion p = value;
				Sample(value, (System.Numerics.Quaternion)p, "quaternion", ref failures, samples);
			}
			Report("quaternion", seed, count, failures, samples);
		}

		[Test]
		public void Plane_RandomThoroughness()
		{
			int seed = Seed(23);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Plane(
					new System.Numerics.Vector3(
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat()
					),
					random.NextFloat()
				);
				SecureValue.Numerics.SecurePlane p = value;
				Sample(value, (System.Numerics.Plane)p, "plane", ref failures, samples);
			}
			Report("plane", seed, count, failures, samples);
		}

		[Test]
		public void Matrix3x2_RandomThoroughness()
		{
			int seed = Seed(24);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Matrix3x2(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Numerics.SecureMatrix3x2 p = value;
				Sample(value, (System.Numerics.Matrix3x2)p, "matrix3x2", ref failures, samples);
			}
			Report("matrix3x2", seed, count, failures, samples);
		}

		[Test]
		public void Matrix4x4_RandomThoroughness()
		{
			int seed = Seed(25);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new System.Numerics.Matrix4x4(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Numerics.SecureMatrix4x4 p = value;
				Sample(value, (System.Numerics.Matrix4x4)p, "matrix4x4", ref failures, samples);
			}
			Report("matrix4x4", seed, count, failures, samples);
		}

		[Test]
		public void Complex_RandomThoroughness()
		{
			int seed = Seed(26);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				double real = NoNaN(BitConverter.Int64BitsToDouble(BitConverter.ToInt64(buf, 0)));
				random.NextBytes(buf);
				double imag = NoNaN(BitConverter.Int64BitsToDouble(BitConverter.ToInt64(buf, 0)));
				var value = new System.Numerics.Complex(real, imag);
				SecureValue.Numerics.SecureComplex p = value;
				System.Numerics.Complex back = p;
				SampleBits(
					BitConverter.DoubleToInt64Bits(value.Real),
					BitConverter.DoubleToInt64Bits(back.Real),
					"complex.real",
					value.ToString(),
					ref failures,
					samples
				);
				SampleBits(
					BitConverter.DoubleToInt64Bits(value.Imaginary),
					BitConverter.DoubleToInt64Bits(back.Imaginary),
					"complex.imag",
					value.ToString(),
					ref failures,
					samples
				);
			}
			Report("complex", seed, count, failures, samples);
		}

		[Test]
		public void BigInteger_RandomThoroughness()
		{
			int seed = Seed(27);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				byte[] buf = new byte[random.Next(1, 34)];
				random.NextBytes(buf);
				System.Numerics.BigInteger value = new System.Numerics.BigInteger(buf);
				SecureValue.Numerics.SecureBigInteger p = value;
				Sample(value, (System.Numerics.BigInteger)p, "bigint", ref failures, samples);
			}
			Report("bigint", seed, count, failures, samples);
		}

		[Test]
		public void UnityVector2_RandomThoroughness()
		{
			int seed = Seed(28);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector2(random.NextFloat(), random.NextFloat());
				SecureValue.Unity.SecureVector2 p = value;
				Sample(value, (Vector2)p, "uvector2", ref failures, samples);
			}
			Report("uvector2", seed, count, failures, samples);
		}

		[Test]
		public void UnityVector2Int_RandomThoroughness()
		{
			int seed = Seed(29);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				int x = BitConverter.ToInt32(buf, 0);
				random.NextBytes(buf);
				int y = BitConverter.ToInt32(buf, 0);
				var value = new Vector2Int(x, y);
				SecureValue.Unity.SecureVector2Int p = value;
				Sample(value, (Vector2Int)p, "uvector2int", ref failures, samples);
			}
			Report("uvector2int", seed, count, failures, samples);
		}

		[Test]
		public void UnityVector3_RandomThoroughness()
		{
			int seed = Seed(30);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat());
				SecureValue.Unity.SecureVector3 p = value;
				Sample(value, (Vector3)p, "uvector3", ref failures, samples);
			}
			Report("uvector3", seed, count, failures, samples);
		}

		[Test]
		public void UnityVector3Int_RandomThoroughness()
		{
			int seed = Seed(31);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				int x = BitConverter.ToInt32(buf, 0);
				random.NextBytes(buf);
				int y = BitConverter.ToInt32(buf, 0);
				random.NextBytes(buf);
				int z = BitConverter.ToInt32(buf, 0);
				var value = new Vector3Int(x, y, z);
				SecureValue.Unity.SecureVector3Int p = value;
				Sample(value, (Vector3Int)p, "uvector3int", ref failures, samples);
			}
			Report("uvector3int", seed, count, failures, samples);
		}

		[Test]
		public void UnityVector4_RandomThoroughness()
		{
			int seed = Seed(32);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector4(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Unity.SecureVector4 p = value;
				Sample(value, (Vector4)p, "uvector4", ref failures, samples);
			}
			Report("uvector4", seed, count, failures, samples);
		}

		[Test]
		public void UnityRect_RandomThoroughness()
		{
			int seed = Seed(33);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Rect(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Unity.SecureRect p = value;
				Sample(value, (Rect)p, "urect", ref failures, samples);
			}
			Report("urect", seed, count, failures, samples);
		}

		[Test]
		public void UnityRectInt_RandomThoroughness()
		{
			int seed = Seed(34);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				int[] c = new int[4];
				for (int k = 0; k < 4; k++)
				{
					random.NextBytes(buf);
					c[k] = BitConverter.ToInt32(buf, 0);
				}
				var value = new RectInt(c[0], c[1], c[2], c[3]);
				SecureValue.Unity.SecureRectInt p = value;
				Sample(value, (RectInt)p, "urectint", ref failures, samples);
			}
			Report("urectint", seed, count, failures, samples);
		}

		[Test]
		public void UnityBounds_RandomThoroughness()
		{
			int seed = Seed(35);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Bounds(
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()),
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat())
				);
				SecureValue.Unity.SecureBounds p = value;
				Sample(value, (Bounds)p, "ubounds", ref failures, samples);
			}
			Report("ubounds", seed, count, failures, samples);
		}

		[Test]
		public void UnityBoundsInt_RandomThoroughness()
		{
			int seed = Seed(36);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				int[] c = new int[6];
				for (int k = 0; k < 6; k++)
				{
					random.NextBytes(buf);
					c[k] = BitConverter.ToInt32(buf, 0);
				}
				var value = new BoundsInt(
					new Vector3Int(c[0], c[1], c[2]),
					new Vector3Int(c[3], c[4], c[5])
				);
				SecureValue.Unity.SecureBoundsInt p = value;
				Sample(value, (BoundsInt)p, "uboundsint", ref failures, samples);
			}
			Report("uboundsint", seed, count, failures, samples);
		}

		[Test]
		public void UnityColor_RandomThoroughness()
		{
			int seed = Seed(37);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Color(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Unity.SecureColor p = value;
				Sample(value, (Color)p, "ucolor", ref failures, samples);
			}
			Report("ucolor", seed, count, failures, samples);
		}

		[Test]
		public void UnityColor32_RandomThoroughness()
		{
			int seed = Seed(38);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				var value = new Color32(buf[0], buf[1], buf[2], buf[3]);
				SecureValue.Unity.SecureColor32 p = value;
				Sample(value, (Color32)p, "ucolor32", ref failures, samples);
			}
			Report("ucolor32", seed, count, failures, samples);
		}

		[Test]
		public void UnityQuaternion_RandomThoroughness()
		{
			int seed = Seed(39);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Quaternion(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureValue.Unity.SecureQuaternion p = value;
				Sample(value, (Quaternion)p, "uquaternion", ref failures, samples);
			}
			Report("uquaternion", seed, count, failures, samples);
		}

		[Test]
		public void UnityMatrix4x4_RandomThoroughness()
		{
			int seed = Seed(40);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Matrix4x4(
					new Vector4(
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat()
					),
					new Vector4(
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat()
					),
					new Vector4(
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat()
					),
					new Vector4(
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat(),
						random.NextFloat()
					)
				);
				SecureValue.Unity.SecureMatrix4x4 p = value;
				Sample(value, (Matrix4x4)p, "umatrix4x4", ref failures, samples);
			}
			Report("umatrix4x4", seed, count, failures, samples);
		}

		[Test]
		public void UnityPlane_RandomThoroughness()
		{
			int seed = Seed(41);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Plane(
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()),
					random.NextFloat()
				);
				SecureValue.Unity.SecurePlane p = value;
				Sample(value, (Plane)p, "uplane", ref failures, samples);
			}
			Report("uplane", seed, count, failures, samples);
		}

		[Test]
		public void UnityRay_RandomThoroughness()
		{
			int seed = Seed(42);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Ray(
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()),
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat())
				);
				SecureValue.Unity.SecureRay p = value;
				Sample(value, (Ray)p, "uray", ref failures, samples);
			}
			Report("uray", seed, count, failures, samples);
		}

		[Test]
		public void UnityLayerMask_RandomThoroughness()
		{
			int seed = Seed(43);
			var random = new System.Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			byte[] buf = new byte[4];
			for (int n = 0; n < count; n++)
			{
				random.NextBytes(buf);
				int maskValue = BitConverter.ToInt32(buf, 0);
				SecureValue.Unity.SecureLayerMask p = new LayerMask() { value = maskValue };
				Sample(maskValue, ((LayerMask)p).value, "ulayermask", ref failures, samples);
			}
			Report("ulayermask", seed, count, failures, samples);
		}

		private static void Report(
			string name,
			int seed,
			long count,
			long failures,
			List<string> samples
		)
		{
			TestContext.WriteLine(
				$"{name}: seed {seed}, {count - failures}/{count} round-tripped."
			);
			Assert.True(
				failures == 0,
				$"{name} seed {seed}: {failures}/{count} mismatched. "
					+ $"Re-run with this seed to reproduce. Samples: {string.Join("; ", samples.ToArray())}"
			);
		}

		private static void Sample<T>(
			T expected,
			T actual,
			string label,
			ref long failures,
			List<string> samples
		)
		{
			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				failures++;
				if (samples.Count < 8)
				{
					samples.Add(
						string.Format("{0}: expected {1}, got {2}", label, expected, actual)
					);
				}
			}
		}

		private static void SampleBits(
			long expected,
			long actual,
			string label,
			string text,
			ref long failures,
			List<string> samples
		)
		{
			if (expected != actual)
			{
				failures++;
				if (samples.Count < 8)
				{
					samples.Add(
						string.Format(
							"{0}: expected bits {1:X16}, got {2:X16} ({3})",
							label,
							expected,
							actual,
							text
						)
					);
				}
			}
		}

		private static void SampleBits(
			int expected,
			int actual,
			string label,
			string text,
			ref long failures,
			List<string> samples
		)
		{
			if (expected != actual)
			{
				failures++;
				if (samples.Count < 8)
				{
					samples.Add(
						string.Format(
							"{0}: expected bits {1:X8}, got {2:X8} ({3})",
							label,
							expected,
							actual,
							text
						)
					);
				}
			}
		}

		private static double NoNaN(double value) => double.IsNaN(value) ? 0.0 : value;

		private static string RandomString(System.Random random)
		{
			int length = random.Next(25);
			char[] chars = new char[length];
			for (int k = 0; k < length; k++)
			{
				chars[k] =
					random.NextDouble() < 0.9
						? (char)random.Next(32, 127)
						: (char)('a' + random.Next(0x400));
			}
			return new string(chars);
		}
	}

	internal static class RandomTestExtensions
	{
		public static float NextFloat(this System.Random random)
		{
			byte[] buf = new byte[4];
			random.NextBytes(buf);
			float value = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(buf, 0));
			return float.IsNaN(value) ? 0f : value;
		}
	}
}
#endif
