#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit TryDecryptTests for all 43 Unity-compilable
	/// types (18 core + 9 numerics + 16 Unity): success round-trips,
	/// uninitialized-false (no event), tampered-false (exactly one event), and
	/// the throwing Decrypted staying intact. Fully-qualified wrappers, no dynamic.
	/// </summary>
	public class TryDecryptTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Test]
		public void Success_CoreTypes()
		{
			SecureBool b = true;
			Assert.IsTrue(b.TryDecrypt(out bool bv));
			Assert.AreEqual(true, bv);

			SecureByte by = 255;
			Assert.IsTrue(by.TryDecrypt(out byte byv));
			Assert.AreEqual(255, byv);

			SecureSByte sb = -128;
			Assert.IsTrue(sb.TryDecrypt(out sbyte sbv));
			Assert.AreEqual(-128, sbv);

			SecureShort s = short.MinValue;
			Assert.IsTrue(s.TryDecrypt(out short sv));
			Assert.AreEqual(short.MinValue, sv);

			SecureUShort us = ushort.MaxValue;
			Assert.IsTrue(us.TryDecrypt(out ushort usv));
			Assert.AreEqual(ushort.MaxValue, usv);

			SecureInt i = int.MinValue;
			Assert.IsTrue(i.TryDecrypt(out int iv));
			Assert.AreEqual(int.MinValue, iv);

			SecureUInt ui = uint.MaxValue;
			Assert.IsTrue(ui.TryDecrypt(out uint uiv));
			Assert.AreEqual(uint.MaxValue, uiv);

			SecureLong l = long.MinValue;
			Assert.IsTrue(l.TryDecrypt(out long lv));
			Assert.AreEqual(long.MinValue, lv);

			SecureULong ul = ulong.MaxValue;
			Assert.IsTrue(ul.TryDecrypt(out ulong ulv));
			Assert.AreEqual(ulong.MaxValue, ulv);

			SecureFloat f = 1.5f;
			Assert.IsTrue(f.TryDecrypt(out float fv));
			Assert.AreEqual(1.5f, fv);

			SecureDouble d = -1e300;
			Assert.IsTrue(d.TryDecrypt(out double dv));
			Assert.AreEqual(-1e300, dv);

			SecureDecimal m = 123456.789m;
			Assert.IsTrue(m.TryDecrypt(out decimal mv));
			Assert.AreEqual(123456.789m, mv);

			SecureChar c = 'S';
			Assert.IsTrue(c.TryDecrypt(out char cv));
			Assert.AreEqual('S', cv);

			SecureString str = "hello";
			Assert.IsTrue(str.TryDecrypt(out string strv));
			Assert.AreEqual("hello", strv);

			SecureString empty = string.Empty;
			Assert.IsTrue(empty.TryDecrypt(out string emptyv));
			Assert.AreEqual(string.Empty, emptyv);

			Guid expectedGuid = Guid.NewGuid();
			SecureGuid g = expectedGuid;
			Assert.IsTrue(g.TryDecrypt(out Guid gv));
			Assert.AreEqual(expectedGuid, gv);

			DateTime expectedDt = new DateTime(638000000000000000L, DateTimeKind.Utc);
			SecureDateTime dt = expectedDt;
			Assert.IsTrue(dt.TryDecrypt(out DateTime dtv));
			Assert.AreEqual(expectedDt, dtv);

			DateTimeOffset expectedDto = new DateTimeOffset(
				638000000000000000L,
				new TimeSpan(2, 0, 0)
			);
			SecureDateTimeOffset dto = expectedDto;
			Assert.IsTrue(dto.TryDecrypt(out DateTimeOffset dtov));
			Assert.AreEqual(expectedDto, dtov);

			TimeSpan expectedTs = new TimeSpan(1, 37, 42);
			SecureTimeSpan ts = expectedTs;
			Assert.IsTrue(ts.TryDecrypt(out TimeSpan tsv));
			Assert.AreEqual(expectedTs, tsv);
		}

		[Test]
		public void Success_NumericsTypes()
		{
			System.Numerics.BigInteger expectedBig = System.Numerics.BigInteger.Pow(2, 128);
			SecureValue.Numerics.SecureBigInteger big = expectedBig;
			Assert.IsTrue(big.TryDecrypt(out System.Numerics.BigInteger bigv));
			Assert.AreEqual(expectedBig, bigv);

			System.Numerics.Complex expectedComplex = new System.Numerics.Complex(1.5, -2.5);
			SecureValue.Numerics.SecureComplex complex = expectedComplex;
			Assert.IsTrue(complex.TryDecrypt(out System.Numerics.Complex complexv));
			Assert.AreEqual(expectedComplex, complexv);

			System.Numerics.Vector2 expectedV2 = new System.Numerics.Vector2(1.5f, -2.5f);
			SecureValue.Numerics.SecureVector2 v2 = expectedV2;
			Assert.IsTrue(v2.TryDecrypt(out System.Numerics.Vector2 v2v));
			Assert.AreEqual(expectedV2, v2v);

			System.Numerics.Vector3 expectedV3 = new System.Numerics.Vector3(1f, 2f, 3f);
			SecureValue.Numerics.SecureVector3 v3 = expectedV3;
			Assert.IsTrue(v3.TryDecrypt(out System.Numerics.Vector3 v3v));
			Assert.AreEqual(expectedV3, v3v);

			System.Numerics.Vector4 expectedV4 = new System.Numerics.Vector4(1f, 2f, 3f, 4f);
			SecureValue.Numerics.SecureVector4 v4 = expectedV4;
			Assert.IsTrue(v4.TryDecrypt(out System.Numerics.Vector4 v4v));
			Assert.AreEqual(expectedV4, v4v);

			System.Numerics.Quaternion expectedQ = new System.Numerics.Quaternion(
				0.5f,
				0.25f,
				0.125f,
				0.0625f
			);
			SecureValue.Numerics.SecureQuaternion q = expectedQ;
			Assert.IsTrue(q.TryDecrypt(out System.Numerics.Quaternion qv));
			Assert.AreEqual(expectedQ, qv);

			System.Numerics.Plane expectedP = new System.Numerics.Plane(1f, 2f, 3f, 4f);
			SecureValue.Numerics.SecurePlane p = expectedP;
			Assert.IsTrue(p.TryDecrypt(out System.Numerics.Plane pv));
			Assert.AreEqual(expectedP, pv);

			System.Numerics.Matrix3x2 expectedM3 = new System.Numerics.Matrix3x2(
				1f,
				2f,
				3f,
				4f,
				5f,
				6f
			);
			SecureValue.Numerics.SecureMatrix3x2 m3 = expectedM3;
			Assert.IsTrue(m3.TryDecrypt(out System.Numerics.Matrix3x2 m3v));
			Assert.AreEqual(expectedM3, m3v);

			System.Numerics.Matrix4x4 expectedM4 = System.Numerics.Matrix4x4.Identity;
			SecureValue.Numerics.SecureMatrix4x4 m4 = expectedM4;
			Assert.IsTrue(m4.TryDecrypt(out System.Numerics.Matrix4x4 m4v));
			Assert.AreEqual(expectedM4, m4v);
		}

		[Test]
		public void Success_UnityTypes()
		{
			SecureValue.Unity.SecureVector2 v2 = new Vector2(12.5f, -3.25f);
			Assert.IsTrue(v2.TryDecrypt(out Vector2 v2v));
			Assert.AreEqual(new Vector2(12.5f, -3.25f), v2v);

			SecureValue.Unity.SecureVector2Int v2i = new Vector2Int(7, -3);
			Assert.IsTrue(v2i.TryDecrypt(out Vector2Int v2iv));
			Assert.AreEqual(new Vector2Int(7, -3), v2iv);

			SecureValue.Unity.SecureVector3 v3 = new Vector3(1f, 2f, 3f);
			Assert.IsTrue(v3.TryDecrypt(out Vector3 v3v));
			Assert.AreEqual(new Vector3(1f, 2f, 3f), v3v);

			SecureValue.Unity.SecureVector3Int v3i = new Vector3Int(1, -2, 3);
			Assert.IsTrue(v3i.TryDecrypt(out Vector3Int v3iv));
			Assert.AreEqual(new Vector3Int(1, -2, 3), v3iv);

			SecureValue.Unity.SecureVector4 v4 = new Vector4(1f, 2f, 3f, 4f);
			Assert.IsTrue(v4.TryDecrypt(out Vector4 v4v));
			Assert.AreEqual(new Vector4(1f, 2f, 3f, 4f), v4v);

			SecureValue.Unity.SecureRect rect = new Rect(10f, 20f, 100f, 50f);
			Assert.IsTrue(rect.TryDecrypt(out Rect rectv));
			Assert.AreEqual(new Rect(10f, 20f, 100f, 50f), rectv);

			SecureValue.Unity.SecureRectInt rectInt = new RectInt(1, 2, 10, 20);
			Assert.IsTrue(rectInt.TryDecrypt(out RectInt rectIntv));
			Assert.AreEqual(new RectInt(1, 2, 10, 20), rectIntv);

			Bounds expectedBounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
			SecureValue.Unity.SecureBounds bounds = expectedBounds;
			Assert.IsTrue(bounds.TryDecrypt(out Bounds boundsv));
			Assert.AreEqual(expectedBounds, boundsv);

			BoundsInt expectedBoundsInt = new BoundsInt(
				new Vector3Int(1, 2, 3),
				new Vector3Int(4, 5, 6)
			);
			SecureValue.Unity.SecureBoundsInt boundsInt = expectedBoundsInt;
			Assert.IsTrue(boundsInt.TryDecrypt(out BoundsInt boundsIntv));
			Assert.AreEqual(expectedBoundsInt, boundsIntv);

			SecureValue.Unity.SecureColor color = new Color(0.5f, 0.25f, 0.125f, 1f);
			Assert.IsTrue(color.TryDecrypt(out Color colorv));
			Assert.AreEqual(new Color(0.5f, 0.25f, 0.125f, 1f), colorv);

			SecureValue.Unity.SecureColor32 color32 = new Color32(237, 129, 64, 210);
			Assert.IsTrue(color32.TryDecrypt(out Color32 color32v));
			Assert.AreEqual(new Color32(237, 129, 64, 210), color32v);

			Quaternion expectedQuat = Quaternion.Euler(15f, 30f, 45f);
			SecureValue.Unity.SecureQuaternion quat = expectedQuat;
			Assert.IsTrue(quat.TryDecrypt(out Quaternion quatv));
			Assert.AreEqual(expectedQuat, quatv);

			Matrix4x4 expectedMat = Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.Euler(15f, 30f, 45f),
				new Vector3(2.5f, 0.5f, 1.75f)
			);
			SecureValue.Unity.SecureMatrix4x4 mat = expectedMat;
			Assert.IsTrue(mat.TryDecrypt(out Matrix4x4 matv));
			Assert.AreEqual(expectedMat, matv);

			Plane expectedPlane = new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f);
			SecureValue.Unity.SecurePlane plane = expectedPlane;
			Assert.IsTrue(plane.TryDecrypt(out Plane planev));
			Assert.AreEqual(expectedPlane, planev);

			Ray expectedRay = new Ray(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 1f));
			SecureValue.Unity.SecureRay ray = expectedRay;
			Assert.IsTrue(ray.TryDecrypt(out Ray rayv));
			Assert.AreEqual(expectedRay, rayv);

			SecureValue.Unity.SecureLayerMask mask = new LayerMask() { value = 5 };
			Assert.IsTrue(mask.TryDecrypt(out LayerMask maskv));
			Assert.AreEqual(5, maskv.value);
		}

		[Test]
		public void Uninitialized_ReturnsFalseWithoutEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var i = default(SecureInt);
			Assert.IsFalse(i.TryDecrypt(out int iv));
			Assert.AreEqual(0, iv);

			var g = default(SecureGuid);
			Assert.IsFalse(g.TryDecrypt(out Guid gv));
			Assert.AreEqual(Guid.Empty, gv);

			var m = default(SecureValue.Unity.SecureMatrix4x4);
			Assert.IsFalse(m.TryDecrypt(out Matrix4x4 mv));
			Assert.AreEqual(Matrix4x4.zero, mv);

			var s = default(SecureString);
			Assert.IsFalse(s.TryDecrypt(out string sv));
			Assert.IsNull(sv);

			Assert.AreEqual(baseline, fired);
			Assert.Throws<UninitializedException>(() =>
			{
				_ = i.Decrypted;
			});
		}

		[Test]
		public void Tampered_ReturnsFalseWithOneEvent()
		{
			object ibox = Corrupt(new SecureInt(42));
			object gbox = Corrupt(new SecureGuid(Guid.NewGuid()));
			object mbox = CorruptOneCellBothCopies(
				new SecureValue.Unity.SecureMatrix4x4(Matrix4x4.identity),
				"_cellA"
			);
			object sbox = Corrupt(new SecureString("tamper me"));

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			var i = (SecureInt)ibox;
			Assert.IsFalse(i.TryDecrypt(out int iv));
			Assert.AreEqual(0, iv);
			Assert.AreEqual(1, fired);

			var g = (SecureGuid)gbox;
			Assert.IsFalse(g.TryDecrypt(out Guid gv));
			Assert.AreEqual(Guid.Empty, gv);
			Assert.AreEqual(1, fired);

			var m = (SecureValue.Unity.SecureMatrix4x4)mbox;
			Assert.IsFalse(m.TryDecrypt(out Matrix4x4 mv));
			Assert.AreEqual(Matrix4x4.zero, mv);
			Assert.AreEqual(1, fired);

			var s = (SecureString)sbox;
			Assert.IsFalse(s.TryDecrypt(out string sv));
			Assert.IsNull(sv);
			Assert.AreEqual(1, fired);

			// The throwing read is unchanged: still fail-closed after a try-read.
			Assert.Throws<TamperedException>(() =>
			{
				_ = i.Decrypted;
			});
			Assert.AreEqual(1, fired);
		}

		private static TWrapper Corrupt<TWrapper>(TWrapper wrapper)
		{
			// Box the live wrapper and flip BOTH cipher words of every backing
			// cell (Cell has a test hook, Cell128 does not), plus any raw
			// cipher/tag words on array-backed wrappers. Singly damaged cells
			// now restore, so false-returning tests must damage both copies.
			object box = wrapper;
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
				else if (f.FieldType == typeof(uint))
				{
					f.SetValue(box, (uint)f.GetValue(box) ^ 0xDEADBEEFu);
				}
				else if (f.FieldType == typeof(ulong))
				{
					f.SetValue(box, (ulong)f.GetValue(box) ^ 0xABCDEF0123456789UL);
				}
				else if (f.FieldType == typeof(ulong[]))
				{
					// Inline short strings carry null arrays (words live in
					// _w0.._w3 instead); their salts/words/tags are already
					// damaged by the branches above.
					var arr = (ulong[])f.GetValue(box);
					if (arr != null && arr.Length > 0)
					{
						arr[0] ^= 1UL;
					}
				}
			}
			return (TWrapper)box;
		}

		private static TWrapper CorruptOneCellBothCopies<TWrapper>(
			TWrapper wrapper,
			string cellField
		)
		{
			// Fully damage exactly one cell (both copies): the wrapper fails closed
			// with a single event while sibling cells stay silent. (Damaging every
			// cell would raise once per damaged cell under the backup contract.)
			object box = wrapper;
			FieldInfo cellF = box.GetType()
				.GetField(cellField, BindingFlags.NonPublic | BindingFlags.Instance);
			object cell = cellF.GetValue(box);
			Type cellType = cell.GetType();
			foreach (string name in new[] { "_cipher", "_cipherB", "_cipherLo", "_cipherLoB" })
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
			cellF.SetValue(box, cell);
			return (TWrapper)box;
		}
	}
}
#endif
