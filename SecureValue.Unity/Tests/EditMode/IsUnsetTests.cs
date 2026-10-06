#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit IsUnsetTests for the 16 Unity wrappers:
	/// never-assigned reads true; any sealed value — including a sealed zero —
	/// reads false. Fully-qualified wrappers (Numerics/Unity name shadowing).
	/// </summary>
	public class IsUnsetTests
	{
		[Test]
		public void Defaults_AreDefault()
		{
			Assert.IsTrue(default(SecureValue.Unity.SecureVector2).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureVector2Int).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureVector3).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureVector3Int).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureVector4).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureRect).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureRectInt).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureBounds).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureBoundsInt).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureColor).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureColor32).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureQuaternion).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureMatrix4x4).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecurePlane).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureRay).IsUnset);
			Assert.IsTrue(default(SecureValue.Unity.SecureLayerMask).IsUnset);
		}

		[Test]
		public void SealedValues_AreNotDefault()
		{
			SecureValue.Unity.SecureVector2 v2 = new Vector2(1f, 2f);
			Assert.IsFalse(v2.IsUnset);

			SecureValue.Unity.SecureVector2Int v2i = new Vector2Int(1, 2);
			Assert.IsFalse(v2i.IsUnset);

			SecureValue.Unity.SecureVector3 v3 = new Vector3(1f, 2f, 3f);
			Assert.IsFalse(v3.IsUnset);

			SecureValue.Unity.SecureVector3Int v3i = new Vector3Int(1, 2, 3);
			Assert.IsFalse(v3i.IsUnset);

			SecureValue.Unity.SecureVector4 v4 = new Vector4(1f, 2f, 3f, 4f);
			Assert.IsFalse(v4.IsUnset);

			SecureValue.Unity.SecureRect rect = new Rect(1f, 2f, 3f, 4f);
			Assert.IsFalse(rect.IsUnset);

			SecureValue.Unity.SecureRectInt rectInt = new RectInt(1, 2, 3, 4);
			Assert.IsFalse(rectInt.IsUnset);

			SecureValue.Unity.SecureBounds bounds = new Bounds(Vector3.one, Vector3.one);
			Assert.IsFalse(bounds.IsUnset);

			SecureValue.Unity.SecureBoundsInt boundsInt = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);
			Assert.IsFalse(boundsInt.IsUnset);

			SecureValue.Unity.SecureColor color = new Color(0.1f, 0.2f, 0.3f);
			Assert.IsFalse(color.IsUnset);

			SecureValue.Unity.SecureColor32 color32 = new Color32(1, 2, 3, 4);
			Assert.IsFalse(color32.IsUnset);

			SecureValue.Unity.SecureQuaternion quat = Quaternion.identity;
			Assert.IsFalse(quat.IsUnset);

			SecureValue.Unity.SecureMatrix4x4 mat = Matrix4x4.identity;
			Assert.IsFalse(mat.IsUnset);

			SecureValue.Unity.SecurePlane plane = new Plane(Vector3.up, 1f);
			Assert.IsFalse(plane.IsUnset);

			SecureValue.Unity.SecureRay ray = new Ray(Vector3.zero, Vector3.forward);
			Assert.IsFalse(ray.IsUnset);

			LayerMask maskValue = default;
			maskValue.value = 3;
			SecureValue.Unity.SecureLayerMask mask = maskValue;
			Assert.IsFalse(mask.IsUnset);
		}

		[Test]
		public void SealedZero_IsNotDefault()
		{
			SecureValue.Unity.SecureVector3 z = Vector3.zero;
			Assert.IsFalse(z.IsUnset);

			SecureValue.Unity.SecureColor c = new Color(0f, 0f, 0f, 0f);
			Assert.IsFalse(c.IsUnset);
		}

		[Test]
		public void Assignment_ClearsDefault()
		{
			SecureValue.Unity.SecureVector3 p = default;
			Assert.IsTrue(p.IsUnset);
			p = Vector3.one;
			Assert.IsFalse(p.IsUnset);
		}
	}
}
#endif
