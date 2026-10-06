#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode serialization/deserialization parity for every Unity-native wrapper: each
	/// value is driven through the genuine Unity save/load path — OnBeforeSerialize
	/// (process-key decrypt, storage-key re-encrypt, word pack) then OnAfterDeserialize
	/// (unpack, storage-tag verify, process-key re-encrypt) — and must come back
	/// bit-identical. The callbacks are reached through the PUBLIC
	/// ISerializationCallbackReceiver interface on a single box: calling them on a struct
	/// local would re-box per call and the stored words would be lost with the box.
	/// Both a typical value and a never-initialized struct (fresh-inspector-field path:
	/// EnsureInitialized materializes an encrypted default on save) are verified.
	/// Unity-family types share simple names with the Numerics family, so every wrapper
	/// below is FULLY QUALIFIED — never rely on usings here.
	/// </summary>
	public class SerializationParityTests
	{
		[Test]
		public void UnityVector2_Parity()
		{
			CheckParity(
				new Vector2(12.5f, -3.25f),
				(Func<Vector2, SecureValue.Unity.SecureVector2>)(v => v),
				w => (Vector2)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureVector2, Vector2>(
				Vector2.zero,
				w => (Vector2)w
			);
		}

		[Test]
		public void UnityVector2Int_Parity()
		{
			CheckParity(
				new Vector2Int(7, -3),
				(Func<Vector2Int, SecureValue.Unity.SecureVector2Int>)(v => v),
				w => (Vector2Int)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureVector2Int, Vector2Int>(
				Vector2Int.zero,
				w => (Vector2Int)w
			);
		}

		[Test]
		public void UnityVector3_Parity()
		{
			CheckParity(
				new Vector3(1f, 2f, 3f),
				(Func<Vector3, SecureValue.Unity.SecureVector3>)(v => v),
				w => (Vector3)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureVector3, Vector3>(
				Vector3.zero,
				w => (Vector3)w
			);
		}

		[Test]
		public void UnityVector3Int_Parity()
		{
			CheckParity(
				new Vector3Int(1, -2, 3),
				(Func<Vector3Int, SecureValue.Unity.SecureVector3Int>)(v => v),
				w => (Vector3Int)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureVector3Int, Vector3Int>(
				Vector3Int.zero,
				w => (Vector3Int)w
			);
		}

		[Test]
		public void UnityVector4_Parity()
		{
			CheckParity(
				new Vector4(1f, 2f, 3f, 4f),
				(Func<Vector4, SecureValue.Unity.SecureVector4>)(v => v),
				w => (Vector4)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureVector4, Vector4>(
				Vector4.zero,
				w => (Vector4)w
			);
		}

		[Test]
		public void UnityRect_Parity()
		{
			CheckParity(
				new Rect(10f, 20f, 100f, 50f),
				(Func<Rect, SecureValue.Unity.SecureRect>)(v => v),
				w => (Rect)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureRect, Rect>(
				new Rect(0f, 0f, 0f, 0f),
				w => (Rect)w
			);
		}

		[Test]
		public void UnityRectInt_Parity()
		{
			CheckParity(
				new RectInt(1, 2, 10, 20),
				(Func<RectInt, SecureValue.Unity.SecureRectInt>)(v => v),
				w => (RectInt)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureRectInt, RectInt>(
				new RectInt(0, 0, 0, 0),
				w => (RectInt)w
			);
		}

		[Test]
		public void UnityBounds_Parity()
		{
			CheckParity(
				new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
				(Func<Bounds, SecureValue.Unity.SecureBounds>)(v => v),
				w => (Bounds)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureBounds, Bounds>(
				new Bounds(Vector3.zero, Vector3.zero),
				w => (Bounds)w
			);
		}

		[Test]
		public void UnityBoundsInt_Parity()
		{
			CheckParity(
				new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
				(Func<BoundsInt, SecureValue.Unity.SecureBoundsInt>)(v => v),
				w => (BoundsInt)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureBoundsInt, BoundsInt>(
				new BoundsInt(Vector3Int.zero, Vector3Int.zero),
				w => (BoundsInt)w
			);
		}

		[Test]
		public void UnityColor_Parity()
		{
			CheckParity(
				new Color(0.15f, 0.5f, 0.85f, 0.75f),
				(Func<Color, SecureValue.Unity.SecureColor>)(v => v),
				w => (Color)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureColor, Color>(
				new Color(0f, 0f, 0f, 0f),
				w => (Color)w
			);
		}

		[Test]
		public void UnityColor32_Parity()
		{
			CheckParity(
				new Color32(237, 129, 64, 210),
				(Func<Color32, SecureValue.Unity.SecureColor32>)(v => v),
				w => (Color32)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureColor32, Color32>(
				new Color32(0, 0, 0, 0),
				w => (Color32)w
			);
		}

		[Test]
		public void UnityQuaternion_Parity()
		{
			Quaternion value = Quaternion.Euler(10f, 20f, 30f);
			CheckParity(
				value,
				(Func<Quaternion, SecureValue.Unity.SecureQuaternion>)(v => v),
				w => (Quaternion)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureQuaternion, Quaternion>(
				new Quaternion(0f, 0f, 0f, 0f),
				w => (Quaternion)w
			);
		}

		[Test]
		public void UnityMatrix4x4_Parity()
		{
			Matrix4x4 value = Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.Euler(15f, 30f, 45f),
				new Vector3(2.5f, 0.5f, 1.75f)
			);
			CheckParity(
				value,
				(Func<Matrix4x4, SecureValue.Unity.SecureMatrix4x4>)(v => v),
				w => (Matrix4x4)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureMatrix4x4, Matrix4x4>(
				Matrix4x4.zero,
				w => (Matrix4x4)w
			);
		}

		[Test]
		public void UnityPlane_Parity()
		{
			CheckParity(
				new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f),
				(Func<Plane, SecureValue.Unity.SecurePlane>)(v => v),
				w => (Plane)w
			);
			CheckDefaultParity<SecureValue.Unity.SecurePlane, Plane>(
				new Plane(Vector3.zero, 0f),
				w => (Plane)w
			);
		}

		[Test]
		public void UnityRay_Parity()
		{
			CheckParity(
				new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f)),
				(Func<Ray, SecureValue.Unity.SecureRay>)(v => v),
				w => (Ray)w
			);
			CheckDefaultParity<SecureValue.Unity.SecureRay, Ray>(
				new Ray(Vector3.zero, Vector3.zero),
				w => (Ray)w
			);
		}

		[Test]
		public void UnityLayerMask_Parity()
		{
			CheckParity(
				5,
				(Func<int, SecureValue.Unity.SecureLayerMask>)(v => new LayerMask() { value = v }),
				w => ((LayerMask)w).value
			);
			CheckDefaultParity<SecureValue.Unity.SecureLayerMask, int>(
				0,
				w => ((LayerMask)w).value
			);
		}

		private static void CheckParity<TWrapper, TValue>(
			TValue value,
			Func<TValue, TWrapper> wrap,
			Func<TWrapper, TValue> read
		)
		{
			// One box for both callbacks: the words written by serialize must be the
			// words read by deserialize.
			object box = wrap(value);
			((ISerializationCallbackReceiver)box).OnBeforeSerialize();
			((ISerializationCallbackReceiver)box).OnAfterDeserialize();
			TWrapper restored = (TWrapper)box;
			Assert.AreEqual(value, read(restored));
		}

		private static void CheckDefaultParity<TWrapper, TValue>(
			TValue expected,
			Func<TWrapper, TValue> read
		)
		{
			// A never-assigned struct: save must materialize an encrypted default
			// (EnsureInitialized) rather than throw, and load must restore it.
			object box = default(TWrapper);
			((ISerializationCallbackReceiver)box).OnBeforeSerialize();
			((ISerializationCallbackReceiver)box).OnAfterDeserialize();
			TWrapper restored = (TWrapper)box;
			Assert.AreEqual(expected, read(restored));
		}
	}
}
#endif
