#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureULong"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureULong))]
	internal class SecureULongDrawer : SecureValueDrawer<SecureULong, ulong>
	{
		protected override ulong Get(SecureULong wrapper) => wrapper;

		protected override SecureULong SetValue(ulong value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			ulong value,
			Action<ulong> write
		)
		{
			// Text field (not LongField): ulong exceeds long.MaxValue, and the
			// full 0..ulong.MaxValue range must be editable.
			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(
				position,
				label,
				value.ToString(CultureInfo.InvariantCulture)
			);
			if (
				EditorGUI.EndChangeCheck()
				&& ulong.TryParse(
					next,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out ulong parsed
				)
			)
			{
				write(parsed);
			}
		}
	}
}
#endif
