#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecureVector3"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecureVector3))]
	internal class SecureVector3Drawer
		: SecureValueDrawer<SecureValue.Numerics.SecureVector3, Vector3>
	{
		protected override Vector3 Get(SecureValue.Numerics.SecureVector3 wrapper)
		{
			System.Numerics.Vector3 v = wrapper;
			return new Vector3(v.X, v.Y, v.Z);
		}

		protected override SecureValue.Numerics.SecureVector3 SetValue(Vector3 value) =>
			new SecureValue.Numerics.SecureVector3(
				new System.Numerics.Vector3(value.x, value.y, value.z)
			);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector3 value,
			Action<Vector3> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector3 next = EditorGUI.Vector3Field(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
