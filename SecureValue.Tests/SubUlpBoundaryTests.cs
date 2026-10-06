using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Sub-sweep-resolution boundary coverage: what index-stepped sweeps cannot reach.
	/// Integer/tick/counter regions are owned 1:1 by ThoroughnessTests (low/high/mid
	/// blocks, identical value sets) and are deliberately NOT repeated here. This suite
	/// covers only the unique parts: single-ULP neighborhoods of float boundaries
	/// (integer stepping rounds onto the boundary for gigascale magnitudes, leaping over
	/// the neighborhood entirely) and extended BigInteger windows (10x wider than the
	/// scaled thoroughness counts, plus the otherwise untested -2^256 range).
	/// </summary>
	[Trait("Category", "Thoroughness")]
	public class SubUlpBoundaryTests
	{
		private const long Region = 1_000_000;

		private static readonly int FloatMinBits = BitConverter.SingleToInt32Bits(float.MinValue);
		private static readonly int FloatMaxBits = BitConverter.SingleToInt32Bits(float.MaxValue);
		private static readonly long DoubleMinBits = BitConverter.DoubleToInt64Bits(
			double.MinValue
		);
		private static readonly long DoubleMaxBits = BitConverter.DoubleToInt64Bits(
			double.MaxValue
		);

		[Fact]
		public void Float_SubUlpBoundaries()
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
			for (long i = 0; i < Region; i++)
			{
				int i32 = (int)i;
				Check(
					BitConverter.Int32BitsToSingle(unchecked(FloatMinBits - i32)),
					"float.aboveMin"
				);
				Check(BitConverter.Int32BitsToSingle(i32), "float.atZero");
				Check(
					BitConverter.Int32BitsToSingle(unchecked(FloatMaxBits - i32)),
					"float.belowMax"
				);
			}
			Assert.True(
				failures == 0,
				$"float regions: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Double_SubUlpBoundaries()
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
			for (long i = 0; i < Region; i++)
			{
				Check(
					BitConverter.Int64BitsToDouble(unchecked(DoubleMinBits - i)),
					"double.aboveMin"
				);
				Check(BitConverter.Int64BitsToDouble(i), "double.atZero");
				Check(
					BitConverter.Int64BitsToDouble(unchecked(DoubleMaxBits - i)),
					"double.belowMax"
				);
			}
			Assert.True(
				failures == 0,
				$"double regions: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void BigInteger_WideWindows()
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
			BigInteger huge = BigInteger.Pow(2, 256);
			for (long i = 0; i < Region; i++)
			{
				Check(-huge + i, "bigint.aboveMin");
				Check(new BigInteger(i), "bigint.atZero");
				Check(huge - i, "bigint.belowMax");
			}
			Assert.True(
				failures == 0,
				$"bigint regions: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Complex_SubUlpBoundaries()
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
			for (long i = 0; i < Region; i++)
			{
				double up = BitConverter.Int64BitsToDouble(unchecked(DoubleMinBits - i));
				double down = BitConverter.Int64BitsToDouble(unchecked(DoubleMaxBits - i));
				double zero = BitConverter.Int64BitsToDouble(i);
				Check(new Complex(up, up), "complex.aboveMin");
				Check(new Complex(zero, zero), "complex.atZero");
				Check(new Complex(down, down), "complex.belowMax");
			}
			Assert.True(
				failures == 0,
				$"complex regions: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Fact]
		public void Vector2_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"vector2",
				2,
				(lanes, block, st) =>
				{
					var value = new Vector2(lanes[0], lanes[1]);
					SecureVector2 p = value;
					Report((Vector2)p == value, block, value.ToString(), st);
				}
			);
		}

		[Fact]
		public void Vector3_SubUlpBoundaries()
		{
			FloatRegionLanes(
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
		public void Vector4_SubUlpBoundaries()
		{
			FloatRegionLanes(
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
		public void Quaternion_SubUlpBoundaries()
		{
			FloatRegionLanes(
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
		public void Plane_SubUlpBoundaries()
		{
			FloatRegionLanes(
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
		public void Matrix3x2_SubUlpBoundaries()
		{
			FloatRegionLanes(
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
		public void Matrix4x4_SubUlpBoundaries()
		{
			FloatRegionLanes(
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

		private sealed class RegionState
		{
			public long Failures;
			public string First = "";
			public long Tested;
		}

		private static void Report(bool ok, string block, string text, RegionState st)
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

		// One ULP step per lane from each boundary (lane k starts k ULPs in, so lanes
		// never share a value).
		private static void FloatRegionLanes(
			string name,
			int laneCount,
			Action<float[], string, RegionState> check
		)
		{
			var st = new RegionState();
			for (long i = 0; i < Region; i++)
			{
				float[] aboveMin = new float[laneCount];
				float[] atZero = new float[laneCount];
				float[] belowMax = new float[laneCount];
				for (int lane = 0; lane < laneCount; lane++)
				{
					int k32 = (int)(i + lane);
					aboveMin[lane] = BitConverter.Int32BitsToSingle(unchecked(FloatMinBits - k32));
					atZero[lane] = BitConverter.Int32BitsToSingle(k32);
					belowMax[lane] = BitConverter.Int32BitsToSingle(unchecked(FloatMaxBits - k32));
				}
				check(aboveMin, $"{name}.aboveMin", st);
				check(atZero, $"{name}.atZero", st);
				check(belowMax, $"{name}.belowMax", st);
			}
			Assert.True(
				st.Failures == 0,
				$"{name} regions: {st.Failures}/{st.Tested} mismatched, first: {st.First}"
			);
		}
	}
}
