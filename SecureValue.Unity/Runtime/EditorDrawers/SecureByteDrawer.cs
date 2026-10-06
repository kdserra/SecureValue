#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureByte"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureByte))]
	internal class SecureByteDrawer : SecureValueDrawer<SecureByte, byte>
	{
		protected override byte Get(SecureByte wrapper) => wrapper;

		protected override SecureByte SetValue(byte value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			byte value,
			Action<byte> write
		)
		{
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write((byte)Math.Clamp(next, byte.MinValue, byte.MaxValue));
			}
		}
	}
}
#endif
