#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode sub-sweep-resolution boundary coverage for the Unity-native float-lane
	/// wrappers: single-ULP neighborhoods of the float boundaries, which index-stepped
	/// sweeps leap over entirely. Integer-lane regions are owned 1:1 by ThoroughnessTests
	/// and are deliberately NOT repeated here. Comparison is bitwise over the lanes:
	/// UnityEngine Vector/Quaternion == is approximate and would mask ULP drift, while
	/// string formatting would dominate runtime (it pushed Matrix4x4 past the runner's
	/// 180 s default timeout); reinterpreting the lanes as int32 is both bit-exact and
	/// allocation-free. Failure text is formatted lazily for the same reason.
	/// Fully-qualified wrapper names throughout. No dynamic (no DLR under IL2CPP).
	/// </summary>
	public class SubUlpBoundaryTests
	{
		private const long Region = 1_000_000;

		private static readonly int FloatMinBits = BitConverter.SingleToInt32Bits(float.MinValue);
		private static readonly int FloatMaxBits = BitConverter.SingleToInt32Bits(float.MaxValue);

		[Test, Timeout(600000)]
		public void UnityVector2_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector2",
				2,
				(lanes, block, st) =>
				{
					var value = new Vector2(lanes[0], lanes[1]);
					SecureValue.Unity.SecureVector2 p = value;
					Vector2 got = p;
					Report(
						SameBits(got.x, value.x) && SameBits(got.y, value.y),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityVector3_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector3",
				3,
				(lanes, block, st) =>
				{
					var value = new Vector3(lanes[0], lanes[1], lanes[2]);
					SecureValue.Unity.SecureVector3 p = value;
					Vector3 got = p;
					Report(
						SameBits(got.x, value.x)
							&& SameBits(got.y, value.y)
							&& SameBits(got.z, value.z),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityVector4_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uvector4",
				4,
				(lanes, block, st) =>
				{
					var value = new Vector4(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureVector4 p = value;
					Vector4 got = p;
					Report(
						SameBits(got.x, value.x)
							&& SameBits(got.y, value.y)
							&& SameBits(got.z, value.z)
							&& SameBits(got.w, value.w),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityRect_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"urect",
				4,
				(lanes, block, st) =>
				{
					var value = new Rect(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureRect p = value;
					Rect got = p;
					Report(
						SameBits(got.x, value.x)
							&& SameBits(got.y, value.y)
							&& SameBits(got.width, value.width)
							&& SameBits(got.height, value.height),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
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
					Bounds got = p;
					Report(
						SameBits(got.center.x, value.center.x)
							&& SameBits(got.center.y, value.center.y)
							&& SameBits(got.center.z, value.center.z)
							&& SameBits(got.extents.x, value.extents.x)
							&& SameBits(got.extents.y, value.extents.y)
							&& SameBits(got.extents.z, value.extents.z),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityColor_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"ucolor",
				4,
				(lanes, block, st) =>
				{
					var value = new Color(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureColor p = value;
					Color got = p;
					Report(
						SameBits(got.r, value.r)
							&& SameBits(got.g, value.g)
							&& SameBits(got.b, value.b)
							&& SameBits(got.a, value.a),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityQuaternion_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uquaternion",
				4,
				(lanes, block, st) =>
				{
					var value = new Quaternion(lanes[0], lanes[1], lanes[2], lanes[3]);
					SecureValue.Unity.SecureQuaternion p = value;
					Quaternion got = p;
					Report(
						SameBits(got.x, value.x)
							&& SameBits(got.y, value.y)
							&& SameBits(got.z, value.z)
							&& SameBits(got.w, value.w),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
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
					Matrix4x4 got = p;
					Report(
						SameBits(got.m00, value.m00)
							&& SameBits(got.m01, value.m01)
							&& SameBits(got.m02, value.m02)
							&& SameBits(got.m03, value.m03)
							&& SameBits(got.m10, value.m10)
							&& SameBits(got.m11, value.m11)
							&& SameBits(got.m12, value.m12)
							&& SameBits(got.m13, value.m13)
							&& SameBits(got.m20, value.m20)
							&& SameBits(got.m21, value.m21)
							&& SameBits(got.m22, value.m22)
							&& SameBits(got.m23, value.m23)
							&& SameBits(got.m30, value.m30)
							&& SameBits(got.m31, value.m31)
							&& SameBits(got.m32, value.m32)
							&& SameBits(got.m33, value.m33),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
		public void UnityPlane_SubUlpBoundaries()
		{
			FloatRegionLanes(
				"uplane",
				4,
				(lanes, block, st) =>
				{
					var value = new Plane(new Vector3(lanes[0], lanes[1], lanes[2]), lanes[3]);
					SecureValue.Unity.SecurePlane p = value;
					Plane got = p;
					Report(
						SameBits(got.normal.x, value.normal.x)
							&& SameBits(got.normal.y, value.normal.y)
							&& SameBits(got.normal.z, value.normal.z)
							&& SameBits(got.distance, value.distance),
						block,
						st,
						() => value.ToString()
					);
				}
			);
		}

		[Test, Timeout(600000)]
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
					Ray got = p;
					Report(
						SameBits(got.origin.x, value.origin.x)
							&& SameBits(got.origin.y, value.origin.y)
							&& SameBits(got.origin.z, value.origin.z)
							&& SameBits(got.direction.x, value.direction.x)
							&& SameBits(got.direction.y, value.direction.y)
							&& SameBits(got.direction.z, value.direction.z),
						block,
						st,
						() => value.ToString()
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

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntUnion
		{
			[FieldOffset(0)]
			public float F;

			[FieldOffset(0)]
			public int I;
		}

		/// <summary>
		/// Bitwise float equality: the wrappers preserve lane bits exactly, so any
		/// drift — including one UnityEngine == would call "close enough" — fails.
		/// </summary>
		private static bool SameBits(float a, float b) =>
			new FloatIntUnion { F = a }.I == new FloatIntUnion { F = b }.I;

		private static void Report(bool ok, string block, RegionState st, Func<string> describe)
		{
			if (!ok)
			{
				if (st.Failures == 0)
				{
					st.First = $"{block} = {describe()}";
				}
				st.Failures++;
			}
			st.Tested++;
		}

		// One ULP step per lane from each boundary (lane k starts k ULPs in, so lanes
		// never share a value). Lane buffers are hoisted out of the loop: allocating
		// them per iteration tripled gen-0 churn for no reason.
		private static void FloatRegionLanes(
			string name,
			int laneCount,
			Action<float[], string, RegionState> check
		)
		{
			var st = new RegionState();
			float[] aboveMin = new float[laneCount];
			float[] atZero = new float[laneCount];
			float[] belowMax = new float[laneCount];
			for (long i = 0; i < Region; i++)
			{
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
