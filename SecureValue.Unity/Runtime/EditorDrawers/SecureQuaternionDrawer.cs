#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecureQuaternion"/>: x, y, z, w fields.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecureQuaternion))]
	internal class SecureQuaternionDrawer
		: SecureValueDrawer<SecureValue.Numerics.SecureQuaternion, System.Numerics.Quaternion>
	{
		protected override System.Numerics.Quaternion Get(
			SecureValue.Numerics.SecureQuaternion wrapper
		) => wrapper;

		protected override SecureValue.Numerics.SecureQuaternion SetValue(
			System.Numerics.Quaternion value
		) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			System.Numerics.Quaternion value,
			Action<System.Numerics.Quaternion> write
		)
		{
			Vector4 fields = new Vector4(value.X, value.Y, value.Z, value.W);
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				write(new System.Numerics.Quaternion(next.x, next.y, next.z, next.w));
			}
		}
	}
}
#endif
