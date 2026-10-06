#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureVector4"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureVector4))]
	internal class SecureVector4UnityDrawer : SecureValueDrawer<SecureVector4, Vector4>
	{
		protected override Vector4 Get(SecureVector4 wrapper) => wrapper;

		protected override SecureVector4 SetValue(Vector4 value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector4 value,
			Action<Vector4> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
