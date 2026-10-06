#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureShort"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureShort))]
	internal class SecureShortDrawer : SecureValueDrawer<SecureShort, short>
	{
		protected override short Get(SecureShort wrapper) => wrapper;

		protected override SecureShort SetValue(short value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			short value,
			Action<short> write
		)
		{
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write((short)Math.Clamp(next, short.MinValue, short.MaxValue));
			}
		}
	}
}
#endif
