#nullable enable
using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Lane isolation: mutating one lane of a multi-lane value must leave every
	/// other lane bit-exact. Single-value core wrappers have no lanes (noted, not tested).
	/// Regression context: the Unity Plane inspector drawer rebuilt values through the
	/// normalizing Plane constructor, so editing distance rescales the normal — the
	/// wrapper-level counterpart is pinned here with a deliberately non-unit normal.
	/// </summary>
	public class LaneIsolationTests
	{
		[Fact]
		public void Vector3_MutateEachLane_OthersUnchanged()
		{
			var p = new SecureVector3(new Vector3(10f, 200f, 500f));

			Vector3 v = p.Decrypted;
			p = new SecureVector3(new Vector3(851f, v.Y, v.Z));
			Assert.Equal(new Vector3(851f, 200f, 500f), p.Decrypted);

			v = p.Decrypted;
			p = new SecureVector3(new Vector3(v.X, -17.5f, v.Z));
			Assert.Equal(new Vector3(851f, -17.5f, 500f), p.Decrypted);

			v = p.Decrypted;
			p = new SecureVector3(new Vector3(v.X, v.Y, 0.125f));
			Assert.Equal(new Vector3(851f, -17.5f, 0.125f), p.Decrypted);
		}

		[Fact]
		public void Vector2_MutateEachLane_OthersUnchanged()
		{
			var p = new SecureVector2(new Vector2(3f, 7f));

			Vector2 v = p.Decrypted;
			p = new SecureVector2(new Vector2(851f, v.Y));
			Assert.Equal(new Vector2(851f, 7f), p.Decrypted);

			v = p.Decrypted;
			p = new SecureVector2(new Vector2(v.X, -0.5f));
			Assert.Equal(new Vector2(851f, -0.5f), p.Decrypted);
		}

		[Fact]
		public void Vector4_MutateEachLane_OthersUnchanged()
		{
			float[] lanes = new float[] { 1f, 2f, 3f, 4f };
			var p = new SecureVector4(V4(lanes));
			for (int i = 0; i < 4; i++)
			{
				float[] current = ToArray(p.Decrypted);
				current[i] = 851f + i;
				p = new SecureVector4(V4(current));
				float[] expect = new float[] { 1f, 2f, 3f, 4f };
				for (int j = 0; j <= i; j++)
				{
					expect[j] = 851f + j;
				}
				Assert.Equal(V4(expect), p.Decrypted);
			}
		}

		[Fact]
		public void Quaternion_MutateEachLane_OthersUnchanged()
		{
			float[] lanes = new float[] { 0.5f, 0.25f, 0.125f, 0.0625f };
			var p = new SecureQuaternion(Q(lanes));
			for (int i = 0; i < 4; i++)
			{
				Quaternion q = p.Decrypted;
				float[] current = new float[] { q.X, q.Y, q.Z, q.W };
				current[i] = 0.75f - i * 0.125f;
				p = new SecureQuaternion(Q(current));
				Quaternion back = p.Decrypted;
				float[] got = new float[] { back.X, back.Y, back.Z, back.W };
				for (int j = 0; j < 4; j++)
				{
					float want = j <= i ? 0.75f - j * 0.125f : lanes[j];
					Assert.Equal(want, got[j]);
				}
			}
		}

		[Fact]
		public void Complex_MutateEachLane_OthersUnchanged()
		{
			var p = new SecureComplex(new Complex(1.5, -2.5));

			Complex c = p.Decrypted;
			p = new SecureComplex(new Complex(851.0, c.Imaginary));
			Assert.Equal(new Complex(851.0, -2.5), p.Decrypted);

			c = p.Decrypted;
			p = new SecureComplex(new Complex(c.Real, 0.125));
			Assert.Equal(new Complex(851.0, 0.125), p.Decrypted);
		}

		[Fact]
		public void Plane_MutateEachLane_OthersUnchanged()
		{
			// Deliberately non-unit normal: any renormalization on lane edits
			// (the inspector drawer bug class) would move the other lanes here.
			var p = new SecurePlane(new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f));

			System.Numerics.Plane v = p.Decrypted;
			Vector3 noRm = v.Normal;
			p = new SecurePlane(new Plane(new Vector3(851f, noRm.Y, noRm.Z), v.D));
			System.Numerics.Plane back = p.Decrypted;
			Assert.Equal(851f, back.Normal.X);
			Assert.Equal(0.9f, back.Normal.Y);
			Assert.Equal(0.35f, back.Normal.Z);
			Assert.Equal(5.25f, back.D);

			v = p.Decrypted;
			p = new SecurePlane(new Plane(v.Normal, -1.5f));
			back = p.Decrypted;
			Assert.Equal(851f, back.Normal.X);
			Assert.Equal(0.9f, back.Normal.Y);
			Assert.Equal(0.35f, back.Normal.Z);
			Assert.Equal(-1.5f, back.D);
		}

		[Fact]
		public void Matrix3x2_MutateEachLane_OthersUnchanged()
		{
			float[] lanes = new float[] { 1f, 2f, 3f, 4f, 5f, 6f };
			var p = new SecureMatrix3x2(M3x2(lanes));
			for (int i = 0; i < 6; i++)
			{
				float[] current = ToM3x2(p.Decrypted);
				current[i] = 851f + i;
				p = new SecureMatrix3x2(M3x2(current));
				float[] got = ToM3x2(p.Decrypted);
				for (int j = 0; j < 6; j++)
				{
					float want = j <= i ? 851f + j : lanes[j];
					Assert.Equal(want, got[j]);
				}
			}
		}

		[Fact]
		public void Matrix4x4_MutateEachLane_OthersUnchanged()
		{
			float[] lanes = new float[]
			{
				1f,
				2f,
				3f,
				4f,
				5f,
				6f,
				7f,
				8f,
				9f,
				10f,
				11f,
				12f,
				13f,
				14f,
				15f,
				16f,
			};
			var p = new SecureMatrix4x4(M4(lanes));
			for (int i = 0; i < 16; i++)
			{
				float[] current = ToM4(p.Decrypted);
				current[i] = 851f + i;
				p = new SecureMatrix4x4(M4(current));
				float[] got = ToM4(p.Decrypted);
				for (int j = 0; j < 16; j++)
				{
					float want = j <= i ? 851f + j : lanes[j];
					Assert.Equal(want, got[j]);
				}
			}
		}

		// ---------- helpers ----------

		private static Vector4 V4(float[] a) => new Vector4(a[0], a[1], a[2], a[3]);

		private static float[] ToArray(Vector4 v) => new float[] { v.X, v.Y, v.Z, v.W };

		private static Quaternion Q(float[] a) => new Quaternion(a[0], a[1], a[2], a[3]);

		private static Matrix3x2 M3x2(float[] a) =>
			new Matrix3x2(a[0], a[1], a[2], a[3], a[4], a[5]);

		private static float[] ToM3x2(Matrix3x2 m) =>
			new float[] { m.M11, m.M12, m.M21, m.M22, m.M31, m.M32 };

		private static Matrix4x4 M4(float[] a) =>
			new Matrix4x4(
				a[0],
				a[1],
				a[2],
				a[3],
				a[4],
				a[5],
				a[6],
				a[7],
				a[8],
				a[9],
				a[10],
				a[11],
				a[12],
				a[13],
				a[14],
				a[15]
			);

		private static float[] ToM4(Matrix4x4 m) =>
			new float[]
			{
				m.M11,
				m.M12,
				m.M13,
				m.M14,
				m.M21,
				m.M22,
				m.M23,
				m.M24,
				m.M31,
				m.M32,
				m.M33,
				m.M34,
				m.M41,
				m.M42,
				m.M43,
				m.M44,
			};
	}
}
