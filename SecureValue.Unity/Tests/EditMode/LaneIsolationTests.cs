#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode lane isolation for the 16 Unity-native wrappers: mutating one lane
	/// must leave every other lane bit-exact. Float lanes compare via R-format
	/// strings (UnityEngine == is approximate and would mask ULP drift); int lanes
	/// compare directly. Core single-value wrappers have no lanes (not tested).
	/// Regression context: the Plane drawer rebuilt values through the normalizing
	/// constructor, so editing distance rescaled the normal — pinned here with a
	/// deliberately non-unit normal (wrapper level; drawer edits need the sandbox).
	/// </summary>
	public class LaneIsolationTests
	{
		[Test]
		public void Vector3_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureVector3 p = new Vector3(10f, 200f, 500f);

			Vector3 v = p;
			p = new Vector3(851f, v.y, v.z);
			CheckVector3(new Vector3(851f, 200f, 500f), p);

			v = p;
			p = new Vector3(v.x, -17.5f, v.z);
			CheckVector3(new Vector3(851f, -17.5f, 500f), p);

			v = p;
			p = new Vector3(v.x, v.y, 0.125f);
			CheckVector3(new Vector3(851f, -17.5f, 0.125f), p);
		}

		[Test]
		public void Vector2_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureVector2 p = new Vector2(3f, 7f);

			Vector2 v = p;
			p = new Vector2(851f, v.y);
			CheckVector2(new Vector2(851f, 7f), p);

			v = p;
			p = new Vector2(v.x, -0.5f);
			CheckVector2(new Vector2(851f, -0.5f), p);
		}

		[Test]
		public void Vector2Int_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureVector2Int p = new Vector2Int(3, 7);

			Vector2Int v = p;
			p = new Vector2Int(851, v.y);
			Assert.AreEqual(new Vector2Int(851, 7), (Vector2Int)p);

			v = p;
			p = new Vector2Int(v.x, -5);
			Assert.AreEqual(new Vector2Int(851, -5), (Vector2Int)p);
		}

		[Test]
		public void Vector3Int_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureVector3Int p = new Vector3Int(10, 200, 500);

			Vector3Int v = p;
			p = new Vector3Int(851, v.y, v.z);
			Assert.AreEqual(new Vector3Int(851, 200, 500), (Vector3Int)p);

			v = p;
			p = new Vector3Int(v.x, -17, v.z);
			Assert.AreEqual(new Vector3Int(851, -17, 500), (Vector3Int)p);

			v = p;
			p = new Vector3Int(v.x, v.y, 2);
			Assert.AreEqual(new Vector3Int(851, -17, 2), (Vector3Int)p);
		}

		[Test]
		public void Vector4_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureVector4 p = new Vector4(1f, 2f, 3f, 4f);

			Vector4 v = p;
			p = new Vector4(851f, v.y, v.z, v.w);
			CheckVector4(new Vector4(851f, 2f, 3f, 4f), p);

			v = p;
			p = new Vector4(v.x, v.y, v.z, 0.125f);
			CheckVector4(new Vector4(851f, 2f, 3f, 0.125f), p);
		}

		[Test]
		public void Rect_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureRect p = new Rect(10f, 20f, 100f, 50f);

			Rect r = p;
			p = new Rect(851f, r.y, r.width, r.height);
			CheckRect(new Rect(851f, 20f, 100f, 50f), p);

			r = p;
			p = new Rect(r.x, r.y, r.width, 0.125f);
			CheckRect(new Rect(851f, 20f, 100f, 0.125f), p);
		}

		[Test]
		public void RectInt_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureRectInt p = new RectInt(1, 2, 10, 20);

			RectInt r = p;
			p = new RectInt(851, r.y, r.width, r.height);
			Assert.AreEqual(new RectInt(851, 2, 10, 20), (RectInt)p);

			r = p;
			p = new RectInt(r.x, r.y, r.width, 3);
			Assert.AreEqual(new RectInt(851, 2, 10, 3), (RectInt)p);
		}

		[Test]
		public void Bounds_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureBounds p = new Bounds(
				new Vector3(1f, 2f, 3f),
				new Vector3(4f, 5f, 6f)
			);

			Bounds b = p;
			p = new Bounds(new Vector3(851f, b.center.y, b.center.z), b.size);
			CheckBounds(new Bounds(new Vector3(851f, 2f, 3f), new Vector3(4f, 5f, 6f)), p);

			b = p;
			p = new Bounds(b.center, new Vector3(b.size.x, b.size.y, 0.125f));
			CheckBounds(new Bounds(new Vector3(851f, 2f, 3f), new Vector3(4f, 5f, 0.125f)), p);
		}

		[Test]
		public void BoundsInt_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureBoundsInt p = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);

			BoundsInt b = p;
			p = new BoundsInt(new Vector3Int(851, b.position.y, b.position.z), b.size);
			Assert.AreEqual(
				new BoundsInt(new Vector3Int(851, 2, 3), new Vector3Int(4, 5, 6)),
				(BoundsInt)p
			);

			b = p;
			p = new BoundsInt(b.position, new Vector3Int(b.size.x, b.size.y, 7));
			Assert.AreEqual(
				new BoundsInt(new Vector3Int(851, 2, 3), new Vector3Int(4, 5, 7)),
				(BoundsInt)p
			);
		}

		[Test]
		public void Color_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureColor p = new Color(0.5f, 0.25f, 0.125f, 1f);

			Color c = p;
			p = new Color(0.75f, c.g, c.b, c.a);
			CheckColor(new Color(0.75f, 0.25f, 0.125f, 1f), p);

			c = p;
			p = new Color(c.r, c.g, c.b, 0.5f);
			CheckColor(new Color(0.75f, 0.25f, 0.125f, 0.5f), p);
		}

		[Test]
		public void Color32_MutateEachLane_OthersUnchanged()
		{
			SecureValue.Unity.SecureColor32 p = new Color32(237, 129, 64, 210);

			Color32 c = p;
			p = new Color32(11, c.g, c.b, c.a);
			Assert.AreEqual(new Color32(11, 129, 64, 210), (Color32)p);

			c = p;
			p = new Color32(c.r, c.g, c.b, 12);
			Assert.AreEqual(new Color32(11, 129, 64, 12), (Color32)p);
		}

		[Test]
		public void Quaternion_MutateEachLane_OthersUnchanged()
		{
			Quaternion start = Quaternion.Euler(15f, 30f, 45f);
			SecureValue.Unity.SecureQuaternion p = start;

			Quaternion q = p;
			p = new Quaternion(0.5f, q.y, q.z, q.w);
			CheckQuaternion(new Quaternion(0.5f, q.y, q.z, q.w), p);

			q = p;
			p = new Quaternion(q.x, q.y, q.z, 0.25f);
			CheckQuaternion(new Quaternion(0.5f, q.y, q.z, 0.25f), p);
		}

		[Test]
		public void Matrix4x4_MutateEachLane_OthersUnchanged()
		{
			Matrix4x4 start = Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.Euler(15f, 30f, 45f),
				new Vector3(2.5f, 0.5f, 1.75f)
			);
			SecureValue.Unity.SecureMatrix4x4 p = start;

			Matrix4x4 m = p;
			m[0, 0] = 851f;
			p = m;
			Matrix4x4 expect = start;
			expect[0, 0] = 851f;
			CheckMatrix4x4(expect, p);

			m = p;
			m[3, 3] = 0.125f;
			p = m;
			expect[3, 3] = 0.125f;
			CheckMatrix4x4(expect, p);
		}

		[Test]
		public void Plane_MutateEachLane_OthersUnchanged()
		{
			// Deliberately non-unit normal: any renormalization on lane edits
			// (the reported drawer bug class) moves the other lanes here.
			Plane start = DefaultPlane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f);
			SecureValue.Unity.SecurePlane p = start;

			Plane v = p;
			p = DefaultPlane(v.normal, 851f);
			CheckPlane(DefaultPlane(new Vector3(0.25f, 0.9f, 0.35f), 851f), p);

			v = p;
			p = DefaultPlane(new Vector3(v.normal.x, 0.125f, v.normal.z), v.distance);
			CheckPlane(DefaultPlane(new Vector3(0.25f, 0.125f, 0.35f), 851f), p);
		}

		[Test]
		public void Ray_MutateEachLane_OthersUnchanged()
		{
			Ray start = DefaultRay(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 1f));
			SecureValue.Unity.SecureRay p = start;

			Ray r = p;
			p = DefaultRay(new Vector3(851f, r.origin.y, r.origin.z), r.direction);
			CheckRay(DefaultRay(new Vector3(851f, 2f, 3f), new Vector3(0f, 0f, 1f)), p);

			r = p;
			p = DefaultRay(r.origin, new Vector3(r.direction.x, r.direction.y, 0.5f));
			CheckRay(DefaultRay(new Vector3(851f, 2f, 3f), new Vector3(0f, 0f, 0.5f)), p);
		}

		[Test]
		public void LayerMask_ReassignKeepsValue()
		{
			SecureValue.Unity.SecureLayerMask p = new LayerMask() { value = 5 };
			Assert.AreEqual(5, ((LayerMask)p).value);
			p = new LayerMask() { value = 9 };
			Assert.AreEqual(9, ((LayerMask)p).value);
		}

		// ---------- exact lane comparison (R-format: Unity == is approximate) ----------

		private static void CheckFloat(float expected, float actual)
		{
			Assert.AreEqual(expected.ToString("R"), actual.ToString("R"));
		}

		private static void CheckVector2(Vector2 expected, SecureValue.Unity.SecureVector2 actual)
		{
			Vector2 v = actual;
			CheckFloat(expected.x, v.x);
			CheckFloat(expected.y, v.y);
		}

		private static void CheckVector3(Vector3 expected, SecureValue.Unity.SecureVector3 actual)
		{
			Vector3 v = actual;
			CheckFloat(expected.x, v.x);
			CheckFloat(expected.y, v.y);
			CheckFloat(expected.z, v.z);
		}

		private static void CheckVector4(Vector4 expected, SecureValue.Unity.SecureVector4 actual)
		{
			Vector4 v = actual;
			CheckFloat(expected.x, v.x);
			CheckFloat(expected.y, v.y);
			CheckFloat(expected.z, v.z);
			CheckFloat(expected.w, v.w);
		}

		private static void CheckRect(Rect expected, SecureValue.Unity.SecureRect actual)
		{
			Rect v = actual;
			CheckFloat(expected.x, v.x);
			CheckFloat(expected.y, v.y);
			CheckFloat(expected.width, v.width);
			CheckFloat(expected.height, v.height);
		}

		private static void CheckBounds(Bounds expected, SecureValue.Unity.SecureBounds actual)
		{
			Bounds v = actual;
			CheckVector3(expected.center, v.center);
			CheckVector3(expected.size, v.size);
		}

		private static void CheckVector3(Vector3 expected, Vector3 actual)
		{
			CheckFloat(expected.x, actual.x);
			CheckFloat(expected.y, actual.y);
			CheckFloat(expected.z, actual.z);
		}

		private static void CheckColor(Color expected, SecureValue.Unity.SecureColor actual)
		{
			Color v = actual;
			CheckFloat(expected.r, v.r);
			CheckFloat(expected.g, v.g);
			CheckFloat(expected.b, v.b);
			CheckFloat(expected.a, v.a);
		}

		private static void CheckQuaternion(
			Quaternion expected,
			SecureValue.Unity.SecureQuaternion actual
		)
		{
			Quaternion v = actual;
			CheckFloat(expected.x, v.x);
			CheckFloat(expected.y, v.y);
			CheckFloat(expected.z, v.z);
			CheckFloat(expected.w, v.w);
		}

		private static void CheckMatrix4x4(
			Matrix4x4 expected,
			SecureValue.Unity.SecureMatrix4x4 actual
		)
		{
			Matrix4x4 v = actual;
			for (int row = 0; row < 4; row++)
			{
				for (int column = 0; column < 4; column++)
				{
					CheckFloat(expected[row, column], v[row, column]);
				}
			}
		}

		private static void CheckPlane(Plane expected, SecureValue.Unity.SecurePlane actual)
		{
			Plane v = actual;
			CheckVector3(expected.normal, v.normal);
			CheckFloat(expected.distance, v.distance);
		}

		private static void CheckRay(Ray expected, SecureValue.Unity.SecureRay actual)
		{
			Ray v = actual;
			CheckVector3(expected.origin, v.origin);
			CheckVector3(expected.direction, v.direction);
		}

		// ---------- non-normalizing construction (mirrors the drawer fix) ----------

		private static Plane DefaultPlane(Vector3 normal, float distance)
		{
			Plane result = default;
			result.normal = normal;
			result.distance = distance;
			return result;
		}

		private static Ray DefaultRay(Vector3 origin, Vector3 direction)
		{
			Ray result = default;
			System.Span<float> floats = System.Runtime.InteropServices.MemoryMarshal.Cast<
				Ray,
				float
			>(System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref result, 1));
			floats[0] = origin.x;
			floats[1] = origin.y;
			floats[2] = origin.z;
			floats[3] = direction.x;
			floats[4] = direction.y;
			floats[5] = direction.z;
			return result;
		}
	}
}
#endif
