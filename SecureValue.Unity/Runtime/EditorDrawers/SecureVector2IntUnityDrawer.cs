#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureVector2Int"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureVector2Int))]
	internal class SecureVector2IntUnityDrawer : SecureValueDrawer<SecureVector2Int, Vector2Int>
	{
		protected override Vector2Int Get(SecureVector2Int wrapper) => wrapper;

		protected override SecureVector2Int SetValue(Vector2Int value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Vector2Int value,
			Action<Vector2Int> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector2Int next = EditorGUI.Vector2IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
