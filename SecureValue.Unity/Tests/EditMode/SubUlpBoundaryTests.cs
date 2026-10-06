#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEngine;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode sub-sweep-resolution boundary coverage for the Unity-native float-lane
	/// wrappers: single-ULP neighborhoods of the float boundaries, which index-stepped
	/// sweeps leap over entirely. Integer-lane regions are owned 1:1 by ThoroughnessTests
	/// and are deliberately NOT repeated here. Comparison uses round-trip ("R")
	/// formatting: UnityEngine Vector/Quaternion == is approximate and would mask ULP
	/// drift, while "R" strings are bit-exact. Fully-qualified wrapper names throughout.
	/// No dynamic (no DLR under IL2CPP).
	/// </summary>
	public class SubUlpBoundaryTests
	{
		private const long Region = 1_000_000;

		private static readonly int FloatMinBits = BitConverter.SingleToInt32Bits(float.MinValue);
		private static readonly int FloatMaxBits = BitConverter.SingleToInt32Bits(float.MaxValue);

		[Test]
		public void UnityVector2_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector2",
				2,
				(lanes, block, st) =>
				{
					var value = new Vector2(lanes[0], lanes[1]);
					SecureValue.Unity.SecureVector2 p = value;
					Report(
						((Vector2)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityVector3_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector3",
				3,
				(lanes, block, st) =>
				{
					var value = new Vector3(lanes[0], lanes[1], lanes[2]);
					SecureValue.Unity.SecureVector3 p = value;
					Report(
						((Vector3)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityVector4_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector4",
				4,
				(lanes, block, st) =>
				{
					var value = new Vector4(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureVector4 p = value;
					Report(
						((Vector4)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityRect_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"urect",
				4,
				(lanes, block, st) =>
				{
					var value = new Rect(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureRect p = value;
					Report(
						((Rect)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityBounds_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"ubounds",
				6,
				(lanes, block, st) =>
				{
					var value = new Bounds(
						new Vector3(lanes[0], lanes[1], lanes[2]),
						new Vector3(lanes[3], lanes[4], lanes[5])
					);
					SecureValue.Unity.SecureBounds p = value;
					Report(
						((Bounds)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityColor_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"ucolor",
				4,
				(lanes, block, st) =>
				{
					var value = new Color(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureColor p = value;
					Report(
						((Color)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityQuaternion_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uquaternion",
				4,
				(lanes, block, st) =>
				{
					var value = new Quaternion(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureQuaternion p = value;
					Report(
						((Quaternion)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityMatrix4x4_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"umatrix4x4",
				16,
				(lanes, block, st) =>
				{
					var value = new Matrix4x4(
						new Vector4(lanes[0], lanes[1], lanes[2], lanes[3]),
						new Vector4(lanes[4], lanes[5], lanes[6], lanes[7]),
						new Vector4(lanes[8], lanes[9], lanes[10], lanes[11]),
						new Vector4(lanes[12], lanes[13], lanes[14], lanes[15])
					);
					SecureValue.Unity.SecureMatrix4x4 p = value;
					Report(
						((Matrix4x4)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityPlane_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uplane",
				4,
				(lanes, block, st) =>
				{
					var value = new Plane(new Vector3(lanes[0], lanes[1], lanes[2]), lanes[3]);
					SecureValue.Unity.SecurePlane p = value;
					Report(
						((Plane)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
				}
			);
		}

		[Test]
		public void UnityRay_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uray",
				6,
				(lanes, block, st) =>
				{
					var value = new Ray(
						new Vector3(lanes[0], lanes[1], lanes[2]),
						new Vector3(lanes[3], lanes[4], lanes[5])
					);
					SecureValue.Unity.SecureRay p = value;
					Report(
						((Ray)p).ToString("R") == value.ToString("R"),
						block,
						value.ToString(),
						st
					);
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
#endif
