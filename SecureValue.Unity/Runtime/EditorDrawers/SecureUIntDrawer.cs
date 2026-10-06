#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureUInt"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureUInt))]
	internal class SecureUIntDrawer : SecureValueDrawer<SecureUInt, uint>
	{
		protected override uint Get(SecureUInt wrapper) => wrapper;

		protected override SecureUInt SetValue(uint value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			uint value,
			Action<uint> write
		)
		{
			EditorGUI.BeginChangeCheck();
			long next = EditorGUI.LongField(position, label, (long)value);
			if (EditorGUI.EndChangeCheck())
			{
				write((uint)Math.Clamp(next, 0L, uint.MaxValue));
			}
		}
	}
}
#endif
