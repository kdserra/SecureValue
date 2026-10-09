#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit WrapperTests for the 16 Unity-native wrappers:
	/// round-trips, conversions, operators, default-reads-default, tamper
	/// detection and allocation-freedom. Fully-qualified wrapper names throughout
	/// (Unity/ Numerics families share simple names). No dynamic (no DLR under IL2CPP).
	/// </summary>
	public class WrapperTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Test]
		public void RoundTrip_AllTypes()
		{
			SecureValue.Unity.SecureVector2 v2 = new Vector2(12.5f, -3.25f);
			Assert.AreEqual(new Vector2(12.5f, -3.25f), (Vector2)v2);
			SecureValue.Unity.SecureVector2Int v2i = new Vector2Int(7, -3);
			Assert.AreEqual(new Vector2Int(7, -3), (Vector2Int)v2i);
			SecureValue.Unity.SecureVector3 v3 = new Vector3(1f, 2f, 3f);
			Assert.AreEqual(new Vector3(1f, 2f, 3f), (Vector3)v3);
			SecureValue.Unity.SecureVector3Int v3i = new Vector3Int(1, -2, 3);
			Assert.AreEqual(new Vector3Int(1, -2, 3), (Vector3Int)v3i);
			SecureValue.Unity.SecureVector4 v4 = new Vector4(1f, 2f, 3f, 4f);
			Assert.AreEqual(new Vector4(1f, 2f, 3f, 4f), (Vector4)v4);
			SecureValue.Unity.SecureRect rect = new Rect(10f, 20f, 100f, 50f);
			Assert.AreEqual(new Rect(10f, 20f, 100f, 50f), (Rect)rect);
			SecureValue.Unity.SecureRectInt rectInt = new RectInt(1, 2, 10, 20);
			Assert.AreEqual(new RectInt(1, 2, 10, 20), (RectInt)rectInt);
			SecureValue.Unity.SecureBounds bounds = new Bounds(
				new Vector3(1f, 2f, 3f),
				new Vector3(4f, 5f, 6f)
			);
			Assert.AreEqual(
				new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
				(Bounds)bounds
			);
			SecureValue.Unity.SecureBoundsInt boundsInt = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);
			Assert.AreEqual(
				new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
				(BoundsInt)boundsInt
			);
			SecureValue.Unity.SecureColor color = new Color(0.15f, 0.5f, 0.85f, 0.75f);
			Assert.AreEqual(new Color(0.15f, 0.5f, 0.85f, 0.75f), (Color)color);
			SecureValue.Unity.SecureColor32 color32 = new Color32(237, 129, 64, 210);
			Assert.AreEqual(new Color32(237, 129, 64, 210), (Color32)color32);
			Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);
			SecureValue.Unity.SecureQuaternion quat = rotation;
			Assert.AreEqual(rotation, (Quaternion)quat);
			Matrix4x4 matrix = Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.identity,
				new Vector3(1f, 1f, 1f)
			);
			SecureValue.Unity.SecureMatrix4x4 mat = matrix;
			Assert.AreEqual(matrix, (Matrix4x4)mat);
			SecureValue.Unity.SecurePlane plane = new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f);
			Assert.AreEqual(new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f), (Plane)plane);
			SecureValue.Unity.SecureRay ray = new Ray(
				new Vector3(1f, 2f, 3f),
				new Vector3(0.36f, 0.48f, 0.8f)
			);
			Assert.AreEqual(
				new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f)),
				(Ray)ray
			);
			SecureValue.Unity.SecureLayerMask mask = new LayerMask() { value = 5 };
			Assert.AreEqual(5, ((LayerMask)mask).value);
		}

		[Test]
		public void Conversions_BothDirections()
		{
			// Implicit construction from plain values, implicit read-back.
			SecureValue.Unity.SecureVector3 v3 = new Vector3(1f, 2f, 3f);
			Vector3 plain = v3;
			Assert.AreEqual(new Vector3(1f, 2f, 3f), plain);
			SecureValue.Unity.SecureVector3 rewrapped = plain;
			Assert.AreEqual(new Vector3(1f, 2f, 3f), (Vector3)rewrapped);
			SecureValue.Unity.SecureColor c = new Color(0.5f, 0.5f, 0.5f, 1f);
			Color back = c;
			Assert.AreEqual(new Color(0.5f, 0.5f, 0.5f, 1f), back);
		}

		[Test]
		public void Arithmetic_SupportedTypes()
		{
			SecureValue.Unity.SecureVector2 a2 = new Vector2(1f, 2f);
			SecureValue.Unity.SecureVector2 b2 = new Vector2(4f, 6f);
			Assert.AreEqual(new Vector2(5f, 8f), (Vector2)(a2 + b2));
			Assert.AreEqual(new Vector2(3f, 4f), (Vector2)(b2 - a2));
			Assert.AreEqual(
				new Vector2(2f, 3f),
				(Vector2)(a2 + (SecureValue.Unity.SecureVector2)new Vector2(1f, 1f))
			);
			Assert.AreEqual(
				new Vector2(2f, 3f),
				(Vector2)((SecureValue.Unity.SecureVector2)new Vector2(1f, 1f) + a2)
			);

			SecureValue.Unity.SecureVector2Int a2i = new Vector2Int(1, 2);
			SecureValue.Unity.SecureVector2Int b2i = new Vector2Int(4, 6);
			Assert.AreEqual(new Vector2Int(5, 8), (Vector2Int)(a2i + b2i));
			Assert.AreEqual(new Vector2Int(3, 4), (Vector2Int)(b2i - a2i));

			SecureValue.Unity.SecureVector3 a3 = new Vector3(1f, 2f, 3f);
			SecureValue.Unity.SecureVector3 b3 = new Vector3(4f, 5f, 6f);
			Assert.AreEqual(new Vector3(5f, 7f, 9f), (Vector3)(a3 + b3));
			Assert.AreEqual(new Vector3(3f, 3f, 3f), (Vector3)(b3 - a3));

			SecureValue.Unity.SecureVector3Int a3i = new Vector3Int(1, 2, 3);
			SecureValue.Unity.SecureVector3Int b3i = new Vector3Int(4, 5, 6);
			Assert.AreEqual(new Vector3Int(5, 7, 9), (Vector3Int)(a3i + b3i));
			Assert.AreEqual(new Vector3Int(3, 3, 3), (Vector3Int)(b3i - a3i));

			SecureValue.Unity.SecureVector4 a4 = new Vector4(1f, 2f, 3f, 4f);
			SecureValue.Unity.SecureVector4 b4 = new Vector4(5f, 6f, 7f, 8f);
			Assert.AreEqual(new Vector4(6f, 8f, 10f, 12f), (Vector4)(a4 + b4));
			Assert.AreEqual(new Vector4(4f, 4f, 4f, 4f), (Vector4)(b4 - a4));

			// Dyadic rationals only: 0.2f + 0.5f-style sums are NOT exactly
			// representable (0.2f + 0.5f lands 1 ULP above 0.7f), so decimal-looking
			// literals would fail an exact comparison through no fault of the wrapper.
			SecureValue.Unity.SecureColor ca = new Color(0.25f, 0.5f, 0.125f, 1f);
			SecureValue.Unity.SecureColor cb = new Color(0.5f, 0.25f, 0.625f, 0f);
			Assert.AreEqual(new Color(0.75f, 0.75f, 0.75f, 1f), (Color)(ca + cb));
			Assert.AreEqual(new Color(0.25f, -0.25f, 0.5f, -1f), (Color)(cb - ca));
		}

		[Test]
		public void Equality_AllTypes()
		{
			// Same plaintext in different wrappers (different salts/ciphertext) compares equal.
			SecureValue.Unity.SecureVector2 v2a = new Vector2(1f, 2f);
			SecureValue.Unity.SecureVector2 v2b = new Vector2(1f, 2f);
			SecureValue.Unity.SecureVector2 v2c = new Vector2(9f, 9f);
			Assert.True(v2a == v2b);
			Assert.True(v2a != v2c);
			Assert.True(v2a == (SecureValue.Unity.SecureVector2)new Vector2(1f, 2f));

			SecureValue.Unity.SecureVector2Int v2ia = new Vector2Int(1, 2);
			SecureValue.Unity.SecureVector2Int v2ib = new Vector2Int(1, 2);
			Assert.True(v2ia == v2ib);
			Assert.True(v2ia != (SecureValue.Unity.SecureVector2Int)new Vector2Int(0, 0));

			SecureValue.Unity.SecureVector3 v3a = new Vector3(1f, 2f, 3f);
			SecureValue.Unity.SecureVector3 v3b = new Vector3(1f, 2f, 3f);
			Assert.True(v3a == v3b);
			Assert.True(v3a != (SecureValue.Unity.SecureVector3)new Vector3(0f, 0f, 0f));

			SecureValue.Unity.SecureVector3Int v3ia = new Vector3Int(1, 2, 3);
			SecureValue.Unity.SecureVector3Int v3ib = new Vector3Int(1, 2, 3);
			Assert.True(v3ia == v3ib);
			Assert.True(v3ia != (SecureValue.Unity.SecureVector3Int)new Vector3Int(0, 0, 0));

			SecureValue.Unity.SecureVector4 v4a = new Vector4(1f, 2f, 3f, 4f);
			SecureValue.Unity.SecureVector4 v4b = new Vector4(1f, 2f, 3f, 4f);
			Assert.True(v4a == v4b);
			Assert.True(v4a != (SecureValue.Unity.SecureVector4)new Vector4(0f, 0f, 0f, 0f));

			SecureValue.Unity.SecureRect ra = new Rect(1f, 2f, 3f, 4f);
			SecureValue.Unity.SecureRect rb = new Rect(1f, 2f, 3f, 4f);
			Assert.True(ra == rb);
			Assert.True(ra != (SecureValue.Unity.SecureRect)new Rect(0f, 0f, 0f, 0f));

			SecureValue.Unity.SecureRectInt ria = new RectInt(1, 2, 3, 4);
			SecureValue.Unity.SecureRectInt rib = new RectInt(1, 2, 3, 4);
			Assert.True(ria == rib);
			Assert.True(ria != (SecureValue.Unity.SecureRectInt)new RectInt(0, 0, 0, 0));

			SecureValue.Unity.SecureBounds ba = new Bounds(
				new Vector3(1f, 2f, 3f),
				new Vector3(4f, 5f, 6f)
			);
			SecureValue.Unity.SecureBounds bb = new Bounds(
				new Vector3(1f, 2f, 3f),
				new Vector3(4f, 5f, 6f)
			);
			Assert.True(ba == bb);
			Assert.True(
				ba != (SecureValue.Unity.SecureBounds)new Bounds(Vector3.zero, Vector3.zero)
			);

			SecureValue.Unity.SecureBoundsInt bia = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);
			SecureValue.Unity.SecureBoundsInt bib = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);
			Assert.True(bia == bib);
			Assert.True(
				bia
					!= (SecureValue.Unity.SecureBoundsInt)
						new BoundsInt(Vector3Int.zero, Vector3Int.zero)
			);

			SecureValue.Unity.SecureColor ca = new Color(0.1f, 0.2f, 0.3f, 1f);
			SecureValue.Unity.SecureColor cb = new Color(0.1f, 0.2f, 0.3f, 1f);
			Assert.True(ca == cb);
			Assert.True(ca != (SecureValue.Unity.SecureColor)new Color(0f, 0f, 0f, 0f));

			SecureValue.Unity.SecureColor32 c32a = new Color32(1, 2, 3, 4);
			SecureValue.Unity.SecureColor32 c32b = new Color32(1, 2, 3, 4);
			Assert.True(c32a == c32b);
			Assert.True(c32a != new Color32(0, 0, 0, 0));

			Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);
			SecureValue.Unity.SecureQuaternion qa = rotation;
			SecureValue.Unity.SecureQuaternion qb = rotation;
			Assert.True(qa == qb);
			Assert.True(qa != (SecureValue.Unity.SecureQuaternion)Quaternion.identity);

			Matrix4x4 matrix = Matrix4x4.identity;
			SecureValue.Unity.SecureMatrix4x4 ma = matrix;
			SecureValue.Unity.SecureMatrix4x4 mb = matrix;
			Assert.True(ma == mb);
			Assert.True(ma != (SecureValue.Unity.SecureMatrix4x4)Matrix4x4.zero);

			SecureValue.Unity.SecurePlane pa = new Plane(new Vector3(0f, 1f, 0f), 5f);
			SecureValue.Unity.SecurePlane pb = new Plane(new Vector3(0f, 1f, 0f), 5f);
			Assert.True(pa == pb);
			Assert.True(pa != (SecureValue.Unity.SecurePlane)new Plane(Vector3.zero, 0f));

			SecureValue.Unity.SecureRay ra2 = new Ray(
				new Vector3(1f, 2f, 3f),
				new Vector3(0f, 0f, 1f)
			);
			SecureValue.Unity.SecureRay rb2 = new Ray(
				new Vector3(1f, 2f, 3f),
				new Vector3(0f, 0f, 1f)
			);
			Assert.True(ra2 == rb2);
			Assert.True(ra2 != new Ray(Vector3.zero, new Vector3(0f, 1f, 0f)));

			SecureValue.Unity.SecureLayerMask la = new LayerMask() { value = 5 };
			SecureValue.Unity.SecureLayerMask lb = new LayerMask() { value = 5 };
			Assert.True(la == lb);
			Assert.True(la != new LayerMask() { value = 0 });
		}

		[Test]
		public void Default_Read_ReturnsDefault()
		{
			Assert.AreEqual(default(Vector2), (Vector2)default(SecureValue.Unity.SecureVector2));
			Assert.AreEqual(
				default(Vector2Int),
				(Vector2Int)default(SecureValue.Unity.SecureVector2Int)
			);
			Assert.AreEqual(default(Vector3), (Vector3)default(SecureValue.Unity.SecureVector3));
			Assert.AreEqual(
				default(Vector3Int),
				(Vector3Int)default(SecureValue.Unity.SecureVector3Int)
			);
			Assert.AreEqual(default(Vector4), (Vector4)default(SecureValue.Unity.SecureVector4));
			Assert.AreEqual(default(Rect), (Rect)default(SecureValue.Unity.SecureRect));
			Assert.AreEqual(default(RectInt), (RectInt)default(SecureValue.Unity.SecureRectInt));
			Assert.AreEqual(default(Bounds), (Bounds)default(SecureValue.Unity.SecureBounds));
			Assert.AreEqual(
				default(BoundsInt),
				(BoundsInt)default(SecureValue.Unity.SecureBoundsInt)
			);
			Assert.AreEqual(default(Color), (Color)default(SecureValue.Unity.SecureColor));
			Assert.AreEqual(default(Color32), (Color32)default(SecureValue.Unity.SecureColor32));
			Assert.AreEqual(
				default(Quaternion),
				(Quaternion)default(SecureValue.Unity.SecureQuaternion)
			);
			Assert.AreEqual(
				default(Matrix4x4),
				(Matrix4x4)default(SecureValue.Unity.SecureMatrix4x4)
			);
			Assert.AreEqual(default(Plane), (Plane)default(SecureValue.Unity.SecurePlane));
			Assert.AreEqual(default(Ray), (Ray)default(SecureValue.Unity.SecureRay));
			Assert.AreEqual(0, ((LayerMask)default(SecureValue.Unity.SecureLayerMask)).value);
		}

		[Test]
		public void Default_Members_ReadDefault()
		{
			Assert.AreEqual(
				default(Vector3).ToString(),
				default(SecureValue.Unity.SecureVector3).ToString()
			);
			Assert.AreEqual(
				default(Vector3).GetHashCode(),
				default(SecureValue.Unity.SecureVector3).GetHashCode()
			);
			Assert.AreEqual(
				default(Bounds).ToString(),
				default(SecureValue.Unity.SecureBounds).ToString()
			);
			Assert.AreEqual(
				default(Matrix4x4).ToString(),
				default(SecureValue.Unity.SecureMatrix4x4).ToString()
			);
		}

		[Test]
		public void Tamper_Detected_AllTypes()
		{
			TamperCheck(
				(SecureValue.Unity.SecureVector2)new Vector2(12.5f, -3.25f),
				w => (object)(Vector2)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureVector2Int)new Vector2Int(7, -3),
				w => (object)(Vector2Int)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f),
				w => (object)(Vector3)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureVector3Int)new Vector3Int(1, -2, 3),
				w => (object)(Vector3Int)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureVector4)new Vector4(1f, 2f, 3f, 4f),
				w => (object)(Vector4)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureRect)new Rect(10f, 20f, 100f, 50f),
				w => (object)(Rect)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureRectInt)new RectInt(1, 2, 10, 20),
				w => (object)(RectInt)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureBounds)
					new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
				w => (object)(Bounds)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureBoundsInt)
					new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
				w => (object)(BoundsInt)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureColor)new Color(0.15f, 0.5f, 0.85f, 0.75f),
				w => (object)(Color)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureColor32)new Color32(237, 129, 64, 210),
				w => (object)(Color32)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureQuaternion)Quaternion.Euler(10f, 20f, 30f),
				w => (object)(Quaternion)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureMatrix4x4)Matrix4x4.identity,
				w => (object)(Matrix4x4)w
			);
			TamperCheck(
				(SecureValue.Unity.SecurePlane)new Plane(new Vector3(0f, 1f, 0f), 5f),
				w => (object)(Plane)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureRay)
					new Ray(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 1f)),
				w => (object)(Ray)w
			);
			TamperCheck(
				(SecureValue.Unity.SecureLayerMask)new LayerMask() { value = 5 },
				w => (object)((LayerMask)w).value
			);
		}

		[Test]
		public void Tamper_EventFiresLowerBound()
		{
			// Handlers are global: assert a lower bound, never an exact count.
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			object box = (SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f);
			CorruptCells(box);
			Assert.Throws<TamperedException>(() =>
			{
				Vector3 x = (SecureValue.Unity.SecureVector3)box;
			});
			Assert.Greater(fired, 0);
		}

		[Test]
		public void Wrappers_AllocationFree()
		{
			CheckAlloc(
				"vector2",
				() =>
				{
					SecureValue.Unity.SecureVector2 p = new Vector2(1f, 2f);
					Vector2 x = p;
				}
			);
			CheckAlloc(
				"vector2int",
				() =>
				{
					SecureValue.Unity.SecureVector2Int p = new Vector2Int(1, 2);
					Vector2Int x = p;
				}
			);
			CheckAlloc(
				"vector3",
				() =>
				{
					SecureValue.Unity.SecureVector3 p = new Vector3(1f, 2f, 3f);
					Vector3 x = p;
				}
			);
			CheckAlloc(
				"vector3int",
				() =>
				{
					SecureValue.Unity.SecureVector3Int p = new Vector3Int(1, 2, 3);
					Vector3Int x = p;
				}
			);
			CheckAlloc(
				"vector4",
				() =>
				{
					SecureValue.Unity.SecureVector4 p = new Vector4(1f, 2f, 3f, 4f);
					Vector4 x = p;
				}
			);
			CheckAlloc(
				"rect",
				() =>
				{
					SecureValue.Unity.SecureRect p = new Rect(1f, 2f, 3f, 4f);
					Rect x = p;
				}
			);
			CheckAlloc(
				"rectint",
				() =>
				{
					SecureValue.Unity.SecureRectInt p = new RectInt(1, 2, 3, 4);
					RectInt x = p;
				}
			);
			CheckAlloc(
				"bounds",
				() =>
				{
					SecureValue.Unity.SecureBounds p = new Bounds(
						new Vector3(1f, 2f, 3f),
						new Vector3(4f, 5f, 6f)
					);
					Bounds x = p;
				}
			);
			CheckAlloc(
				"boundsint",
				() =>
				{
					SecureValue.Unity.SecureBoundsInt p = new BoundsInt(
						new Vector3Int(1, 2, 3),
						new Vector3Int(4, 5, 6)
					);
					BoundsInt x = p;
				}
			);
			CheckAlloc(
				"color",
				() =>
				{
					SecureValue.Unity.SecureColor p = new Color(0.1f, 0.2f, 0.3f, 1f);
					Color x = p;
				}
			);
			CheckAlloc(
				"color32",
				() =>
				{
					SecureValue.Unity.SecureColor32 p = new Color32(1, 2, 3, 4);
					Color32 x = p;
				}
			);
			CheckAlloc(
				"quaternion",
				() =>
				{
					SecureValue.Unity.SecureQuaternion p = Quaternion.identity;
					Quaternion x = p;
				}
			);
			CheckAlloc(
				"matrix4x4",
				() =>
				{
					SecureValue.Unity.SecureMatrix4x4 p = Matrix4x4.identity;
					Matrix4x4 x = p;
				}
			);
			CheckAlloc(
				"plane",
				() =>
				{
					SecureValue.Unity.SecurePlane p = new Plane(new Vector3(0f, 1f, 0f), 5f);
					Plane x = p;
				}
			);
			CheckAlloc(
				"ray",
				() =>
				{
					SecureValue.Unity.SecureRay p = new Ray(
						new Vector3(1f, 2f, 3f),
						new Vector3(0f, 0f, 1f)
					);
					Ray x = p;
				}
			);
			CheckAlloc(
				"layermask",
				() =>
				{
					SecureValue.Unity.SecureLayerMask p = new LayerMask() { value = 5 };
					LayerMask x = p;
				}
			);
		}

		private static void TamperCheck<TWrapper>(TWrapper wrapper, Func<TWrapper, object> read)
		{
			// Box the live wrapper, corrupt every backing cell on the box, then
			// confirm the read throws instead of decrypting.
			object box = wrapper;
			CorruptCells(box);
			Assert.Throws<TamperedException>(() => read((TWrapper)box));
		}

		private static void CorruptCells(object box)
		{
			// Flip BOTH cipher words of every backing cell directly: only Cell
			// carries a CorruptForTesting hook (Cell128 does not), and singly
			// damaged cells now restore from the good copy instead of throwing.
			Type t = box.GetType();
			foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (f.FieldType == typeof(Cell) || f.FieldType == typeof(Cell128))
				{
					object cell = f.GetValue(box);
					Type cellType = cell.GetType();
					foreach (
						string name in new[] { "_cipher", "_cipherB", "_cipherLo", "_cipherLoB" }
					)
					{
						FieldInfo cipher = cellType.GetField(
							name,
							BindingFlags.NonPublic | BindingFlags.Instance
						);
						if (cipher != null)
						{
							cipher.SetValue(cell, (ulong)cipher.GetValue(cell) ^ 1UL);
						}
					}
					f.SetValue(box, cell);
				}
			}
		}

		private static void CheckAlloc(string name, Action action)
		{
			for (int i = 0; i < 100; i++)
			{
				action();
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			for (int i = 0; i < 1000; i++)
			{
				action();
			}
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.True(after == before, name + " allocated managed memory");
		}
	}
}
#endif
