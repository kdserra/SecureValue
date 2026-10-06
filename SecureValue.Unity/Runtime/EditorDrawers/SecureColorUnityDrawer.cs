#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureColor"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureColor))]
	internal class SecureColorUnityDrawer : SecureValueDrawer<SecureColor, Color>
	{
		protected override Color Get(SecureColor wrapper) => wrapper;

		protected override SecureColor SetValue(Color value) => value;

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
