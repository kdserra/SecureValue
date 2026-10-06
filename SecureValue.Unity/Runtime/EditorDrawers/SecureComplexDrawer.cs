#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureComplex"/>: real and imaginary fields.</summary>
	[CustomPropertyDrawer(typeof(SecureComplex))]
	internal class SecureComplexDrawer : SecureValueDrawer<SecureComplex, Complex>
	{
		protected override Complex Get(SecureComplex wrapper) => wrapper;

		protected override SecureComplex SetValue(Complex value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Complex value,
			Action<Complex> write
		)
		{
			UnityEngine.Vector2 fields = new UnityEngine.Vector2(
				(float)value.Real,
				(float)value.Imaginary
			);
			EditorGUI.BeginChangeCheck();
			UnityEngine.Vector2 next = EditorGUI.Vector2Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				write(new Complex(next.x, next.y));
			}
		}
	}
}
#endif
