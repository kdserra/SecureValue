#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureUShort"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureUShort))]
	internal class SecureUShortDrawer : SecureValueDrawer<SecureUShort, ushort>
	{
		protected override ushort Get(SecureUShort wrapper) => wrapper;

		protected override SecureUShort SetValue(ushort value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			ushort value,
			Action<ushort> write
		)
		{
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write((ushort)Math.Clamp(next, ushort.MinValue, ushort.MaxValue));
			}
		}
	}
}
#endif
