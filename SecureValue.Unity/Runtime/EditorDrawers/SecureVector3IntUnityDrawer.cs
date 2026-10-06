#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureVector3Int"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureVector3Int))]
	internal class SecureVector3IntUnityDrawer : SecureValueDrawer<SecureVector3Int, Vector3Int>
	{
		protected override Vector3Int Get(SecureVector3Int wrapper) => wrapper;

		protected override SecureVector3Int SetValue(Vector3Int value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector3Int value,
			Action<Vector3Int> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector3Int next = EditorGUI.Vector3IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
