#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureDouble"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureDouble))]
	internal class SecureDoubleDrawer : SecureValueDrawer<SecureDouble, double>
	{
		protected override double Get(SecureDouble wrapper) => wrapper;

		protected override SecureDouble SetValue(double value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			double value,
			Action<double> write
		)
		{
			EditorGUI.BeginChangeCheck();
			double next = EditorGUI.DoubleField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
