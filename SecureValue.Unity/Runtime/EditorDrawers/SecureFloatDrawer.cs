#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureFloat"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureFloat))]
	internal class SecureFloatDrawer : SecureValueDrawer<SecureFloat, float>
	{
		protected override float Get(SecureFloat wrapper) => wrapper;

		protected override SecureFloat SetValue(float value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			float value,
			Action<float> write
		)
		{
			EditorGUI.BeginChangeCheck();
			float next = EditorGUI.FloatField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
