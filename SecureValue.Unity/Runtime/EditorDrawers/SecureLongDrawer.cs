#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureLong"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureLong))]
	internal class SecureLongDrawer : SecureValueDrawer<SecureLong, long>
	{
		protected override long Get(SecureLong wrapper) => wrapper;

		protected override SecureLong SetValue(long value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			long value,
			Action<long> write
		)
		{
			EditorGUI.BeginChangeCheck();
			long next = EditorGUI.LongField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
