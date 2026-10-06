#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureBool"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureBool))]
	internal class SecureBoolDrawer : SecureValueDrawer<SecureBool, bool>
	{
		protected override bool Get(SecureBool wrapper) => wrapper;

		protected override SecureBool SetValue(bool value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			bool value,
			Action<bool> write
		)
		{
			EditorGUI.BeginChangeCheck();
			bool next = EditorGUI.Toggle(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
