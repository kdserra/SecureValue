#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace SecureValue.Unity
{
	/// <summary>
	/// Base for Secure* inspector drawers: decrypts the serialized storage
	/// words, draws a single native control for the plaintext value, and
	/// re-encrypts back into the serialized array on change.
	/// </summary>
	internal abstract class SecureValueDrawer<TWrapper, TValue> : PropertyDrawer
		where TWrapper : struct, ISecureSerialization
	{
		protected abstract TValue Get(TWrapper wrapper);
		protected abstract TWrapper SetValue(TValue value);
		protected abstract void DrawControl(
			Rect position,
			GUIContent label,
			TValue value,
			Action<TValue> write
		);

		/// <summary>
		/// Draws one native control for the decrypted value; writes re-encrypt
		/// through the live wrapper (never through the serialized words).
		/// Stale saved words display the type default instead of throwing.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UnityEngine.Object target = property.serializedObject.targetObject;
			if (target == null)
			{
				return;
			}
			var wrapper = (TWrapper)fieldInfo.GetValue(target);
			TValue value;
			try
			{
				value = wrapper.IsUnset ? default(TValue) : Get(wrapper);
			}
			catch (Exception)
			{
				// Saved words from an older layout can decode to invalid values
				// (e.g. decimal scale, UTC offset): display the default instead.
				// Runtime reads keep strict tamper-detection.
				value = default(TValue);
			}
			DrawControl(
				position,
				label,
				value,
				v =>
				{
					TWrapper updated = SetValue(v);
					Undo.RecordObject(target, "Edit Secure Value");
					fieldInfo.SetValue(target, updated);
					EditorUtility.SetDirty(target);
				}
			);
		}

		/// <summary>Single control-row height; multi-row drawers override this.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		/// <summary>Total height for a drawer spanning <paramref name="lines"/> control rows.</summary>
		protected static float HeightForLines(int lines) =>
			EditorGUIUtility.singleLineHeight * lines
			+ EditorGUIUtility.standardVerticalSpacing * (lines - 1);

		/// <summary>Row <paramref name="row"/> (0-based) of a multi-line drawer rect.</summary>
		protected static Rect RowRect(Rect position, int row) =>
			new Rect(
				position.x,
				position.y
					+ row
						* (
							EditorGUIUtility.singleLineHeight
							+ EditorGUIUtility.standardVerticalSpacing
						),
				position.width,
				EditorGUIUtility.singleLineHeight
			);
	}
}
#endif
