#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecureVector4"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecureVector4))]
	internal class SecureVector4Drawer
		: SecureValueDrawer<SecureValue.Numerics.SecureVector4, Vector4>
	{
		protected override Vector4 Get(SecureValue.Numerics.SecureVector4 wrapper)
		{
			System.Numerics.Vector4 v = wrapper;
			return new Vector4(v.X, v.Y, v.Z, v.W);
		}

		protected override SecureValue.Numerics.SecureVector4 SetValue(Vector4 value) =>
			new SecureValue.Numerics.SecureVector4(
				new System.Numerics.Vector4(value.x, value.y, value.z, value.w)
			);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector4 value,
			Action<Vector4> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
