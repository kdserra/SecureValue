using System;
using System.Collections.Generic;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace SecureValue.Tests
{
	/// <summary>
	/// Random round-trip coverage: roughly 10M values (exact total is logged) drawn from
	/// the full bit space of every wrapper type. The seed is time-derived so every run
	/// tests different values, and it is ALWAYS logged — with any failing values — so a
	/// failure can be reproduced and retested by re-seeding a Random with the printed
	/// seed and regenerating the same sequence. Multi-component aggregates mask NaN
	/// lanes to 0 (documented): struct Equals cannot compare NaN, and NaN payload
	/// preservation is already covered bit-exactly by the extreme suites.
	/// </summary>
	[Trait("Category", "Thoroughness")]
	public class RandomThoroughnessTests
	{
		private readonly ITestOutputHelper _output;

		// One base seed per run: every test derives its own seed from it, so runs differ
		// from each other while each failure log still names an exact reproducible seed.
		private static readonly int RunSeed = unchecked((int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF));

		public RandomThoroughnessTests(ITestOutputHelper output)
		{
			_output = output;
		}

		private static int Seed(int index) => unchecked(RunSeed * 397 ^ index);

		[Fact]
		public void Bool_RandomThoroughness()
		{
			int seed = Seed(1);
			var random = new Random(seed);
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

		[Fact]
		public void Byte_RandomThoroughness()
		{
			int seed = Seed(2);
			var random = new Random(seed);
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

		[Fact]
		public void SByte_RandomThoroughness()
		{
			int seed = Seed(3);
			var random = new Random(seed);
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

		[Fact]
		public void Char_RandomThoroughness()
		{
			int seed = Seed(4);
			var random = new Random(seed);
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

		[Fact]
		public void Short_RandomThoroughness()
		{
			int seed = Seed(5);
			var random = new Random(seed);
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

		[Fact]
		public void UShort_RandomThoroughness()
		{
			int seed = Seed(6);
			var random = new Random(seed);
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

		[Fact]
		public void Int_RandomThoroughness()
		{
			int seed = Seed(7);
			var random = new Random(seed);
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

		[Fact]
		public void UInt_RandomThoroughness()
		{
			int seed = Seed(8);
			var random = new Random(seed);
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

		[Fact]
		public void Long_RandomThoroughness()
		{
			int seed = Seed(9);
			var random = new Random(seed);
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

		[Fact]
		public void ULong_RandomThoroughness()
		{
			int seed = Seed(10);
			var random = new Random(seed);
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

		[Fact]
		public void Float_RandomThoroughness()
		{
			int seed = Seed(11);
			var random = new Random(seed);
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

		[Fact]
		public void Double_RandomThoroughness()
		{
			int seed = Seed(12);
			var random = new Random(seed);
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

		[Fact]
		public void Decimal_RandomThoroughness()
		{
			int seed = Seed(13);
			var random = new Random(seed);
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

		[Fact]
		public void String_RandomThoroughness()
		{
			int seed = Seed(14);
			var random = new Random(seed);
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

		[Fact]
		public void Guid_RandomThoroughness()
		{
			int seed = Seed(15);
			var random = new Random(seed);
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

		[Fact]
		public void DateTime_RandomThoroughness()
		{
			int seed = Seed(16);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			for (int n = 0; n < count; n++)
			{
				var value = new DateTime(
					random.NextInt64(0, DateTime.MaxValue.Ticks + 1),
					(DateTimeKind)random.Next(3)
				);
				SecureDateTime p = value;
				Sample(value, (DateTime)p, "datetime", ref failures, samples);
			}
			Report("datetime", seed, count, failures, samples);
		}

		[Fact]
		public void DateTimeOffset_RandomThoroughness()
		{
			int seed = Seed(17);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 300_000;
			int[] offsets = { 0, 60, -60, 330, -330, 840, -840 };
			long edge = TimeSpan.FromHours(14).Ticks;
			for (int n = 0; n < count; n++)
			{
				var value = new DateTimeOffset(
					random.NextInt64(edge, DateTimeOffset.MaxValue.Ticks - edge),
					TimeSpan.FromMinutes(offsets[random.Next(offsets.Length)])
				);
				SecureDateTimeOffset p = value;
				Sample(value, (DateTimeOffset)p, "dateoffset", ref failures, samples);
			}
			Report("dateoffset", seed, count, failures, samples);
		}

		[Fact]
		public void TimeSpan_RandomThoroughness()
		{
			int seed = Seed(18);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 400_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				var value = new TimeSpan(BitConverter.ToInt64(buf.Randomize(random), 0));
				SecureTimeSpan p = value;
				Sample(value, (TimeSpan)p, "timespan", ref failures, samples);
			}
			Report("timespan", seed, count, failures, samples);
		}

#if NET6_0_OR_GREATER
		[Fact]
		public void DateOnly_RandomThoroughness()
		{
			int seed = Seed(19);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			for (int n = 0; n < count; n++)
			{
				var value = DateOnly.FromDayNumber(random.Next(3652059));
				SecureDateOnly p = value;
				Sample(value, (DateOnly)p, "dateonly", ref failures, samples);
			}
			Report("dateonly", seed, count, failures, samples);
		}

		[Fact]
		public void TimeOnly_RandomThoroughness()
		{
			int seed = Seed(20);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			for (int n = 0; n < count; n++)
			{
				var value = new TimeOnly(random.NextInt64(0, TimeOnly.MaxValue.Ticks + 1));
				SecureTimeOnly p = value;
				Sample(value, (TimeOnly)p, "timeonly", ref failures, samples);
			}
			Report("timeonly", seed, count, failures, samples);
		}
#endif

#if NET
		[Fact]
		public void Rune_RandomThoroughness()
		{
			int seed = Seed(21);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 200_000;
			for (int n = 0; n < count; n++)
			{
				int codePoint = random.Next(0x110000 - 0x800);
				if (codePoint >= 0xD800)
				{
					codePoint += 0x800;
				}
				var value = new System.Text.Rune(codePoint);
				SecureRune p = value;
				Sample(value, (System.Text.Rune)p, "rune", ref failures, samples);
			}
			Report("rune", seed, count, failures, samples);
		}
#endif

		[Fact]
		public void BigInteger_RandomThoroughness()
		{
			int seed = Seed(22);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 150_000;
			for (int n = 0; n < count; n++)
			{
				byte[] buf = new byte[random.Next(1, 34)];
				random.NextBytes(buf);
				BigInteger value = new BigInteger(buf);
				SecureBigInteger p = value;
				Sample(value, (BigInteger)p, "bigint", ref failures, samples);
			}
			Report("bigint", seed, count, failures, samples);
		}

		[Fact]
		public void Complex_RandomThoroughness()
		{
			int seed = Seed(23);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			byte[] buf = new byte[8];
			for (int n = 0; n < count; n++)
			{
				var value = new Complex(
					NoNaN(
						BitConverter.Int64BitsToDouble(
							BitConverter.ToInt64(buf.Randomize(random), 0)
						)
					),
					NoNaN(
						BitConverter.Int64BitsToDouble(
							BitConverter.ToInt64(buf.Randomize(random), 0)
						)
					)
				);
				SecureComplex p = value;
				SampleBits(
					BitConverter.DoubleToInt64Bits(value.Real),
					BitConverter.DoubleToInt64Bits(((Complex)p).Real),
					"complex.real",
					value.ToString(),
					ref failures,
					samples
				);
				SampleBits(
					BitConverter.DoubleToInt64Bits(value.Imaginary),
					BitConverter.DoubleToInt64Bits(((Complex)p).Imaginary),
					"complex.imag",
					value.ToString(),
					ref failures,
					samples
				);
			}
			Report("complex", seed, count, failures, samples);
		}

		[Fact]
		public void Vector2_RandomThoroughness()
		{
			int seed = Seed(24);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector2(random.NextFloat(), random.NextFloat());
				SecureVector2 p = value;
				Sample(value, (Vector2)p, "vector2", ref failures, samples);
			}
			Report("vector2", seed, count, failures, samples);
		}

		[Fact]
		public void Vector3_RandomThoroughness()
		{
			int seed = Seed(25);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat());
				SecureVector3 p = value;
				Sample(value, (Vector3)p, "vector3", ref failures, samples);
			}
			Report("vector3", seed, count, failures, samples);
		}

		[Fact]
		public void Vector4_RandomThoroughness()
		{
			int seed = Seed(26);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Vector4(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureVector4 p = value;
				Sample(value, (Vector4)p, "vector4", ref failures, samples);
			}
			Report("vector4", seed, count, failures, samples);
		}

		[Fact]
		public void Quaternion_RandomThoroughness()
		{
			int seed = Seed(27);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Quaternion(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureQuaternion p = value;
				Sample(value, (Quaternion)p, "quaternion", ref failures, samples);
			}
			Report("quaternion", seed, count, failures, samples);
		}

		[Fact]
		public void Plane_RandomThoroughness()
		{
			int seed = Seed(28);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Plane(
					new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()),
					random.NextFloat()
				);
				SecurePlane p = value;
				Sample(value, (Plane)p, "plane", ref failures, samples);
			}
			Report("plane", seed, count, failures, samples);
		}

		[Fact]
		public void Matrix3x2_RandomThoroughness()
		{
			int seed = Seed(29);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Matrix3x2(
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat(),
					random.NextFloat()
				);
				SecureMatrix3x2 p = value;
				Sample(value, (Matrix3x2)p, "matrix3x2", ref failures, samples);
			}
			Report("matrix3x2", seed, count, failures, samples);
		}

		[Fact]
		public void Matrix4x4_RandomThoroughness()
		{
			int seed = Seed(30);
			var random = new Random(seed);
			long failures = 0;
			var samples = new List<string>();
			const int count = 250_000;
			for (int n = 0; n < count; n++)
			{
				var value = new Matrix4x4(
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
				SecureMatrix4x4 p = value;
				Sample(value, (Matrix4x4)p, "matrix4x4", ref failures, samples);
			}
			Report("matrix4x4", seed, count, failures, samples);
		}

		private void Report(string name, int seed, long count, long failures, List<string> samples)
		{
			_output.WriteLine($"{name}: seed {seed}, {count - failures}/{count} round-tripped.");
			Assert.True(
				failures == 0,
				$"{name} seed {seed}: {failures}/{count} mismatched. "
					+ $"Re-run with this seed to reproduce. Samples: {string.Join("; ", samples)}"
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
					samples.Add($"{label}: expected {expected}, got {actual}");
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
						$"{label}: expected bits {expected:X16}, got {actual:X16} ({text})"
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
					samples.Add($"{label}: expected bits {expected:X8}, got {actual:X8} ({text})");
				}
			}
		}

		private static float NoNaN(float value) => float.IsNaN(value) ? 0f : value;

		private static double NoNaN(double value) => double.IsNaN(value) ? 0.0 : value;

		private static string RandomString(Random random)
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

	internal static class RandomExtensions
	{
		// Reuses the caller's buffer: no per-iteration allocation inside sweep loops.
		public static byte[] Randomize(this byte[] buf, Random random)
		{
			random.NextBytes(buf);
			return buf;
		}

		// Full-bit-space float with NaN masked out (see test class doc).
		public static float NextFloat(this Random random)
		{
			byte[] buf = new byte[4];
			random.NextBytes(buf);
			float value = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(buf, 0));
			return float.IsNaN(value) ? 0f : value;
		}
	}
}
