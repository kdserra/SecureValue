#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureSByte"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureSByte))]
	internal class SecureSByteDrawer : SecureValueDrawer<SecureSByte, sbyte>
	{
		protected override sbyte Get(SecureSByte wrapper) => wrapper;

		protected override SecureSByte SetValue(sbyte value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			sbyte value,
			Action<sbyte> write
		)
		{
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write((sbyte)Math.Clamp(next, sbyte.MinValue, sbyte.MaxValue));
			}
		}
	}
}
#endif
