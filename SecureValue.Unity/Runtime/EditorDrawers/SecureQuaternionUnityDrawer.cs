#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureQuaternion"/>: x, y, z, w fields.</summary>
	[CustomPropertyDrawer(typeof(SecureQuaternion))]
	internal class SecureQuaternionUnityDrawer : SecureValueDrawer<SecureQuaternion, Quaternion>
	{
		protected override Quaternion Get(SecureQuaternion wrapper) => wrapper;

		protected override SecureQuaternion SetValue(Quaternion value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Quaternion value,
			Action<Quaternion> write
		)
		{
			Vector4 fields = new Vector4(value.x, value.y, value.z, value.w);
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				write(new Quaternion(next.x, next.y, next.z, next.w));
			}
		}
	}
}
#endif
