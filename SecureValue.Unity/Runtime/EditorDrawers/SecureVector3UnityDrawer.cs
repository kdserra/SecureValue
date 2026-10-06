#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureVector3"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureVector3))]
	internal class SecureVector3UnityDrawer : SecureValueDrawer<SecureVector3, Vector3>
	{
		protected override Vector3 Get(SecureVector3 wrapper) => wrapper;

		protected override SecureVector3 SetValue(Vector3 value) => value;

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
