#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode coverage for Unity-family API the WrapperTests mirror does not
	/// pin: mixed plain twins (P,T)/(T,P), forward-only scalar twins, the
	/// Quaternion/Matrix4x4 products, unary minus, happy-path ToString/Equals/
	/// GetHashCode/interface dispatch, tampered save-data, and PropertyDrawer
	/// wiring (one drawer per wrapper — reflection only, no IMGUI involved).
	/// Fully-qualified wrapper names throughout (Unity/Numerics families share
	/// simple names). Anything here that fails in the sandbox is a real bug:
	/// every type/member below was verified against the staged sources.
	/// </summary>
	public class UnityApiTests
	{
		[Test]
		public void MixedTwins_BindBothOrders()
		{
			SecureValue.Unity.SecureVector2 sv2 = new Vector2(1f, 2f);
			Vector2 plain2 = new Vector2(4f, 6f);
			Assert.AreEqual(new Vector2(5f, 8f), (Vector2)(sv2 + plain2));
			Assert.AreEqual(new Vector2(5f, 8f), (Vector2)(plain2 + sv2));
			Assert.AreEqual(new Vector2(-3f, -4f), (Vector2)(sv2 - plain2));
			Assert.AreEqual(new Vector2(3f, 4f), (Vector2)(plain2 - sv2));
			Assert.True(sv2 == new Vector2(1f, 2f));
			Assert.True(new Vector2(1f, 2f) == sv2);

			SecureValue.Unity.SecureVector3Int sv3i = new Vector3Int(1, 2, 3);
			Vector3Int plain3i = new Vector3Int(4, 5, 6);
			Assert.AreEqual(new Vector3Int(5, 7, 9), (Vector3Int)(sv3i + plain3i));
			Assert.AreEqual(new Vector3Int(5, 7, 9), (Vector3Int)(plain3i + sv3i));

			SecureValue.Unity.SecureColor sc = new Color(0.25f, 0.5f, 0.125f, 1f);
			Color plainC = new Color(0.5f, 0.25f, 0.625f, 0f);
			Assert.AreEqual(new Color(0.75f, 0.75f, 0.75f, 1f), (Color)(sc + plainC));
			Assert.AreEqual(new Color(0.75f, 0.75f, 0.75f, 1f), (Color)(plainC + sc));

			SecureValue.Unity.SecureRect sr = new Rect(1f, 2f, 3f, 4f);
			Rect plainR = new Rect(1f, 2f, 3f, 4f);
			Assert.True(sr == plainR);
			Assert.True(plainR == sr);
		}

		[Test]
		public void ScalarTwins_ForwardOnly()
		{
			SecureValue.Unity.SecureVector2 v2 = new Vector2(1f, 2f);
			Assert.AreEqual(new Vector2(1f, 2f) * 2f, (Vector2)(v2 * 2f));
			Assert.AreEqual(new Vector2(1f, 2f) / 2f, (Vector2)(v2 / 2f));

			SecureValue.Unity.SecureVector2Int v2i = new Vector2Int(1, 2);
			Assert.AreEqual(new Vector2Int(1, 2) * 2, (Vector2Int)(v2i * 2));
			Assert.AreEqual(new Vector2Int(1, 2) / 2, (Vector2Int)(v2i / 2));

			SecureValue.Unity.SecureColor c = new Color(0.25f, 0.5f, 0.125f, 1f);
			Assert.AreEqual(new Color(0.25f, 0.5f, 0.125f, 1f) * 0.5f, (Color)(c * 0.5f));
		}

		[Test]
		public void Products_QuaternionAndMatrix()
		{
			Quaternion r = Quaternion.Euler(10f, 20f, 30f);
			SecureValue.Unity.SecureQuaternion a = r;
			SecureValue.Unity.SecureQuaternion b = r;
			Assert.AreEqual(r * r, (Quaternion)(a * b));
			Assert.AreEqual(r * r, (Quaternion)(a * r));
			Assert.AreEqual(r * r, (Quaternion)(r * a));

			Matrix4x4 m = Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.identity,
				new Vector3(2f, 2f, 2f)
			);
			SecureValue.Unity.SecureMatrix4x4 ma = m;
			SecureValue.Unity.SecureMatrix4x4 mb = m;
			Assert.AreEqual(m * m, (Matrix4x4)(ma * mb));
			Assert.AreEqual(m * m, (Matrix4x4)(ma * m));
			Assert.AreEqual(m * m, (Matrix4x4)(m * ma));
		}

		[Test]
		public void UnaryMinus_Vectors()
		{
			Assert.AreEqual(
				-new Vector2(1f, -2f),
				(Vector2)(-(SecureValue.Unity.SecureVector2)new Vector2(1f, -2f))
			);
			Assert.AreEqual(
				-new Vector3(1f, -2f, 3f),
				(Vector3)(-(SecureValue.Unity.SecureVector3)new Vector3(1f, -2f, 3f))
			);
			Assert.AreEqual(
				-new Vector4(1f, -2f, 3f, -4f),
				(Vector4)(-(SecureValue.Unity.SecureVector4)new Vector4(1f, -2f, 3f, -4f))
			);
		}

		[Test]
		public void Members_HappyPath()
		{
			SecureValue.Unity.SecureVector3 v3 = new Vector3(1f, 2f, 3f);
			Assert.AreEqual(new Vector3(1f, 2f, 3f).ToString(), v3.ToString());
			Assert.AreEqual(new Vector3(1f, 2f, 3f).GetHashCode(), v3.GetHashCode());
			// Equals(object) is narrow library-wide (SecureX only, all 46
			// wrappers): plain-type equality goes through ==/!= and
			// IEquatable<TPlain>, pinned below.
			Assert.False(v3.Equals((object)new Vector3(1f, 2f, 3f)));
			Assert.False(v3.Equals((object)new Vector3(0f, 0f, 0f)));
			Assert.False(v3.Equals((object)"not a vector"));
			Assert.True(((IEquatable<SecureValue.Unity.SecureVector3>)v3).Equals(v3));
			Assert.True(((IEquatable<Vector3>)v3).Equals(new Vector3(1f, 2f, 3f)));

			SecureValue.Unity.SecureColor c = new Color(0.1f, 0.2f, 0.3f, 1f);
			Assert.AreEqual(new Color(0.1f, 0.2f, 0.3f, 1f).ToString(), c.ToString());

			SecureValue.Unity.SecureRay ray = new Ray(
				new Vector3(1f, 2f, 3f),
				new Vector3(0f, 0f, 1f)
			);
			Assert.AreEqual(
				new Ray(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 1f)).GetHashCode(),
				ray.GetHashCode()
			);
		}

		[Test]
		public void TamperedSaveData_ThrowsOnDeserialize()
		{
			TamperSerializedCheck(
				(SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f),
				w => (object)(Vector3)w
			);
			TamperSerializedCheck(
				(SecureValue.Unity.SecureColor32)new Color32(1, 2, 3, 4),
				w => (object)(Color32)w
			);
			TamperSerializedCheck(
				(SecureValue.Unity.SecureBounds)
					new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
				w => (object)(Bounds)w
			);
			TamperSerializedCheck(
				(SecureValue.Unity.SecureRay)
					new Ray(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 1f)),
				w => (object)(Ray)w
			);
			TamperSerializedCheck(
				(SecureValue.Unity.SecureLayerMask)new LayerMask() { value = 5 },
				w => (object)((LayerMask)w).value
			);
			TamperSerializedCheck(
				(SecureValue.Unity.SecureMatrix4x4)Matrix4x4.identity,
				w => (object)(Matrix4x4)w
			);
		}

		[Test]
		public void Drawers_ExactlyOnePerWrapper()
		{
			// Every Unity-compilable wrapper (18 Core + 9 Numerics + 16 Unity)
			// must have exactly one PropertyDrawer targeting it. Reflection
			// only — no IMGUI involved, so this runs headless in EditMode.
			// Catches dead/duplicate/mistargeted drawers (the session-20 class).
			string[] expected =
			{
				"SecureValue.SecureBool",
				"SecureValue.SecureByte",
				"SecureValue.SecureChar",
				"SecureValue.SecureDateTime",
				"SecureValue.SecureDateTimeOffset",
				"SecureValue.SecureDecimal",
				"SecureValue.SecureDouble",
				"SecureValue.SecureFloat",
				"SecureValue.SecureGuid",
				"SecureValue.SecureInt",
				"SecureValue.SecureLong",
				"SecureValue.SecureSByte",
				"SecureValue.SecureShort",
				"SecureValue.SecureString",
				"SecureValue.SecureTimeSpan",
				"SecureValue.SecureUInt",
				"SecureValue.SecureULong",
				"SecureValue.SecureUShort",
				"SecureValue.Numerics.SecureBigInteger",
				"SecureValue.Numerics.SecureComplex",
				"SecureValue.Numerics.SecureMatrix3x2",
				"SecureValue.Numerics.SecureMatrix4x4",
				"SecureValue.Numerics.SecurePlane",
				"SecureValue.Numerics.SecureQuaternion",
				"SecureValue.Numerics.SecureVector2",
				"SecureValue.Numerics.SecureVector3",
				"SecureValue.Numerics.SecureVector4",
				"SecureValue.Unity.SecureBounds",
				"SecureValue.Unity.SecureBoundsInt",
				"SecureValue.Unity.SecureColor",
				"SecureValue.Unity.SecureColor32",
				"SecureValue.Unity.SecureLayerMask",
				"SecureValue.Unity.SecureMatrix4x4",
				"SecureValue.Unity.SecurePlane",
				"SecureValue.Unity.SecureQuaternion",
				"SecureValue.Unity.SecureRay",
				"SecureValue.Unity.SecureRect",
				"SecureValue.Unity.SecureRectInt",
				"SecureValue.Unity.SecureVector2",
				"SecureValue.Unity.SecureVector2Int",
				"SecureValue.Unity.SecureVector3",
				"SecureValue.Unity.SecureVector3Int",
				"SecureValue.Unity.SecureVector4",
			};
			Array.Sort(expected);

			FieldInfo targetField = typeof(CustomPropertyDrawer).GetField(
				"m_Type",
				BindingFlags.NonPublic | BindingFlags.Instance
			);
			Assert.NotNull(targetField, "CustomPropertyDrawer.m_Type missing");

			Assembly runtime = typeof(SecureValue.Unity.SecureVector3).Assembly;
			List<string> actual = new List<string>();
			foreach (Type t in runtime.GetTypes())
			{
				object[] attrs = t.GetCustomAttributes(typeof(CustomPropertyDrawer), false);
				foreach (object attr in attrs)
				{
					Type target = (Type)targetField.GetValue(attr);
					actual.Add(target.FullName ?? target.Name);
				}
			}
			actual.Sort();
			Assert.AreEqual(string.Join(";", expected), string.Join(";", actual));
		}

		private static void TamperSerializedCheck<TWrapper>(
			TWrapper wrapper,
			Func<TWrapper, object> read
		)
		{
			// Save through the genuine callback, flip every serialized word
			// (both backup copies die, so the reload throws), and confirm the
			// tamper surfaces instead of decrypting.
			object box = wrapper;
			((ISerializationCallbackReceiver)box).OnBeforeSerialize();
			FieldInfo field = box.GetType()
				.GetField("_serialized", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(field);
			uint[] words = (uint[])field.GetValue(box);
			Assert.NotNull(words);
			for (int i = 0; i < words.Length; i++)
			{
				words[i] ^= 0xFFFFFFFFU;
			}
			Assert.Throws<TamperedException>(() =>
			{
				((ISerializationCallbackReceiver)box).OnAfterDeserialize();
				read((TWrapper)box);
			});
		}
	}
}
#endif
