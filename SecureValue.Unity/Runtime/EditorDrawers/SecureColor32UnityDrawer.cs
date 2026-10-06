#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureColor32"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureColor32))]
	internal class SecureColor32UnityDrawer : SecureValueDrawer<SecureColor32, Color>
	{
		protected override Color Get(SecureColor32 wrapper)
		{
			Color32 c = wrapper;
			return c;
		}

		protected override SecureColor32 SetValue(Color value) => new SecureColor32((Color32)value);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Color value,
			Action<Color> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Color next = EditorGUI.ColorField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
