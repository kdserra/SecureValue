#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureDecimal"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureDecimal))]
	internal class SecureDecimalDrawer : SecureValueDrawer<SecureDecimal, decimal>
	{
		protected override decimal Get(SecureDecimal wrapper) => wrapper;

		protected override SecureDecimal SetValue(decimal value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			decimal value,
			Action<decimal> write
		)
		{
			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(
				position,
				label,
				value.ToString(CultureInfo.InvariantCulture)
			);
			if (
				EditorGUI.EndChangeCheck()
				&& decimal.TryParse(
					next,
					NumberStyles.Number,
					CultureInfo.InvariantCulture,
					out decimal parsed
				)
			)
			{
				write(parsed);
			}
		}
	}
}
#endif
