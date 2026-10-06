#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecureVector2"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecureVector2))]
	internal class SecureVector2Drawer
		: SecureValueDrawer<SecureValue.Numerics.SecureVector2, Vector2>
	{
		protected override Vector2 Get(SecureValue.Numerics.SecureVector2 wrapper)
		{
			System.Numerics.Vector2 v = wrapper;
			return new Vector2(v.X, v.Y);
		}

		protected override SecureValue.Numerics.SecureVector2 SetValue(Vector2 value) =>
			new SecureValue.Numerics.SecureVector2(new System.Numerics.Vector2(value.x, value.y));

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector2 value,
			Action<Vector2> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector2 next = EditorGUI.Vector2Field(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
