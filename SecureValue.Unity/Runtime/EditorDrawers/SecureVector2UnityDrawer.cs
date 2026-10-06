#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureVector2"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureVector2))]
	internal class SecureVector2UnityDrawer : SecureValueDrawer<SecureVector2, Vector2>
	{
		protected override Vector2 Get(SecureVector2 wrapper) => wrapper;

		protected override SecureVector2 SetValue(Vector2 value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector2 value,
			Action<Vector2> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector2 next = EditorGUI.Vector2Field(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
