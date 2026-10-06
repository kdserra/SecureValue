#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureString"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureString))]
	internal class SecureStringDrawer : SecureValueDrawer<SecureString, string>
	{
		protected override string Get(SecureString wrapper) => wrapper;

		protected override SecureString SetValue(string value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			string value,
			Action<string> write
		)
		{
			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(position, label, value ?? string.Empty);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
