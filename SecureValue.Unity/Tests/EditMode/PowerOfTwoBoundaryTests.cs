#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEngine;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode bit-pattern coverage for the 16 Unity-native wrappers: +-1000 around
	/// each 2^n transition in range, walking single bits across the full width, and
	/// alternating-bit masks sized to the lane type. Integer lanes use exact 2^n + lane
	/// anchors (always distinct); float lanes use exact 2^(n-k) powers with per-lane
	/// bit-dedupe in windows (past ULP 2000 the window rounds onto the transition).
	/// Float lanes skip alternating masks (integer-word concept; patterns would round
	/// on conversion — exponent sweeps and walking are their bit coverage). Fully
	/// qualified wrapper names throughout. No dynamic (no DLR under IL2CPP).
	/// </summary>
	public class PowerOfTwoBoundaryTests
	{
		private const int Window = 1000;

		[Test]
		public void UnityVector2Int_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(Vector2Int value, string block)
			{
				SecureValue.Unity.SecureVector2Int p = value;
				if ((Vector2Int)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
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
					Check(
						new Vector2Int((int)(power + d), (int)(power + 1 + d)),
						$"uvector2int.t+{n}"
					);
					Check(
						new Vector2Int((int)(-(power + d)), (int)(-(power + 1 + d))),
						$"uvector2int.t-{n}"
					);
				}
				Check(new Vector2Int((int)power, (int)power + 1), $"uvector2int.w+{n}");
				Check(new Vector2Int((int)-power, (int)-power - 1), $"uvector2int.w-{n}");
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(
					new Vector2Int(
						unchecked(int.MinValue + (int)d),
						unchecked(int.MinValue + (int)d + 1)
					),
					"uvector2int.min"
				);
			}
			Check(new Vector2Int(int.MinValue, int.MinValue + 1), "uvector2int.min exact");
			Check(new Vector2Int(int.MaxValue - 1, int.MaxValue), "uvector2int.max exact");
			Check(new Vector2Int(1431655765, -1431655765), "uvector2int.alt P");
			Check(new Vector2Int(-1431655766, 1431655765), "uvector2int.alt Q");
			Assert.True(
				failures == 0,
				$"uvector2int patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityVector3Int_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(Vector3Int value, string block)
			{
				SecureValue.Unity.SecureVector3Int p = value;
				if ((Vector3Int)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
					}
					failures++;
				}
				tested++;
			}
			int[] alt = { 1431655765, -1431655765, -1431655766 };
			for (int n = 0; n <= 30; n++)
			{
				long power = 1L << n;
				for (long d = -Window; d <= Window; d++)
				{
					Check(
						new Vector3Int(
							(int)(power + d),
							(int)(power + 1 + d),
							(int)(power + 2 + d)
						),
						$"uvector3int.t+{n}"
					);
					Check(
						new Vector3Int(
							(int)(-(power + d)),
							(int)(-(power + 1 + d)),
							(int)(-(power + 2 + d))
						),
						$"uvector3int.t-{n}"
					);
				}
				Check(
					new Vector3Int((int)power, (int)power + 1, (int)power + 2),
					$"uvector3int.w+{n}"
				);
				Check(
					new Vector3Int((int)-power, (int)-power - 1, (int)-power - 2),
					$"uvector3int.w-{n}"
				);
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(
					new Vector3Int(
						unchecked(int.MinValue + (int)d),
						unchecked(int.MinValue + (int)d + 1),
						unchecked(int.MinValue + (int)d + 2)
					),
					"uvector3int.min"
				);
			}
			Check(new Vector3Int(alt[0], alt[1], alt[2]), "uvector3int.alt");
			Check(
				new Vector3Int(int.MinValue, int.MinValue + 1, int.MinValue + 2),
				"uvector3int.min exact"
			);
			Check(
				new Vector3Int(int.MaxValue - 2, int.MaxValue - 1, int.MaxValue),
				"uvector3int.max exact"
			);
			Assert.True(
				failures == 0,
				$"uvector3int patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityRectInt_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(RectInt value, string block)
			{
				SecureValue.Unity.SecureRectInt p = value;
				if ((RectInt)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
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
					Check(
						new RectInt(
							(int)(power + d),
							(int)(power + 1 + d),
							(int)(power + 2 + d),
							(int)(power + 3 + d)
						),
						$"urectint.t+{n}"
					);
					Check(
						new RectInt(
							(int)(-(power + d)),
							(int)(-(power + 1 + d)),
							(int)(-(power + 2 + d)),
							(int)(-(power + 3 + d))
						),
						$"urectint.t-{n}"
					);
				}
				Check(
					new RectInt((int)power, (int)power + 1, (int)power + 2, (int)power + 3),
					$"urectint.w+{n}"
				);
				Check(
					new RectInt((int)-power, (int)-power - 1, (int)-power - 2, (int)-power - 3),
					$"urectint.w-{n}"
				);
			}
			Check(new RectInt(1431655765, -1431655765, -1431655766, 1431655765), "urectint.alt");
			Assert.True(
				failures == 0,
				$"urectint patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityBoundsInt_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(BoundsInt value, string block)
			{
				SecureValue.Unity.SecureBoundsInt p = value;
				if ((BoundsInt)p != value)
				{
					if (failures == 0)
					{
						first = $"{block} = {value}";
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
					Check(
						new BoundsInt(
							new Vector3Int(
								(int)(power + d),
								(int)(power + 1 + d),
								(int)(power + 2 + d)
							),
							new Vector3Int(
								(int)(power + 3 + d),
								(int)(power + 4 + d),
								(int)(power + 5 + d)
							)
						),
						$"uboundsint.t+{n}"
					);
					Check(
						new BoundsInt(
							new Vector3Int(
								(int)(-(power + d)),
								(int)(-(power + 1 + d)),
								(int)(-(power + 2 + d))
							),
							new Vector3Int(
								(int)(-(power + 3 + d)),
								(int)(-(power + 4 + d)),
								(int)(-(power + 5 + d))
							)
						),
						$"uboundsint.t-{n}"
					);
				}
			}
			Check(
				new BoundsInt(
					new Vector3Int(1431655765, -1431655765, -1431655766),
					new Vector3Int(-1431655765, 1431655765, -1431655766)
				),
				"uboundsint.alt"
			);
			Assert.True(
				failures == 0,
				$"uboundsint patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityLayerMask_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(int maskValue, string block)
			{
				SecureValue.Unity.SecureLayerMask p = new LayerMask() { value = maskValue };
				if (((LayerMask)p).value != maskValue)
				{
					if (failures == 0)
					{
						first = $"{block} = {maskValue}";
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
					Check((int)(power + d), $"ulayermask.t+{n}");
					Check((int)(-(power + d)), $"ulayermask.t-{n}");
				}
				Check((int)power, $"ulayermask.w+{n}");
				Check((int)-power, $"ulayermask.w-{n}");
			}
			for (long d = 0; d <= Window; d++)
			{
				Check(unchecked(int.MinValue + (int)d), "ulayermask.min");
			}
			Check(int.MinValue, "ulayermask.min exact");
			Check(int.MaxValue, "ulayermask.max exact");
			Check(1431655765, "ulayermask.alt P");
			Check(-1431655765, "ulayermask.alt -P");
			Check(-1431655766, "ulayermask.alt Q");
			Assert.True(
				failures == 0,
				$"ulayermask patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityColor32_BitPatterns()
		{
			long failures = 0;
			string first = "";
			long tested = 0;
			void Check(long value, string block)
			{
				if (value < 0 || value > 255)
				{
					return;
				}
				byte r = (byte)value;
				byte g = (byte)((value + 1) % 256);
				byte b = (byte)((value + 2) % 256);
				byte a = (byte)((value + 3) % 256);
				var expected = new Color32(r, g, b, a);
				SecureValue.Unity.SecureColor32 p = expected;
				// No != operator on Color32 (and .rgba is internal): ValueType.Equals
				// is exact here — four byte fields, no float subtleties.
				if (!((Color32)p).Equals(expected))
				{
					if (failures == 0)
					{
						first = $"{block} = {expected}";
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
					Check(power + d, $"ucolor32.t{n}");
				}
				Check(power, $"ucolor32.w{n}");
			}
			Check(0x55, "ucolor32.alt P");
			Check(0xAA, "ucolor32.alt Q");
			Assert.True(
				failures == 0,
				$"ucolor32 patterns: {failures}/{tested} mismatched, first: {first}"
			);
		}

		[Test]
		public void UnityVector2_BitPatterns()
		{
			FloatLanes(
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
		public void UnityVector3_BitPatterns()
		{
			FloatLanes(
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
		public void UnityVector4_BitPatterns()
		{
			FloatLanes(
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
		public void UnityRect_BitPatterns()
		{
			FloatLanes(
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
		public void UnityBounds_BitPatterns()
		{
			FloatLanes(
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
		public void UnityColor_BitPatterns()
		{
			FloatLanes(
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
		public void UnityQuaternion_BitPatterns()
		{
			FloatLanes(
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
		public void UnityMatrix4x4_BitPatterns()
		{
			FloatLanes(
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
		public void UnityPlane_BitPatterns()
		{
			FloatLanes(
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
		public void UnityRay_BitPatterns()
		{
			FloatLanes(
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
	}
}
#endif
