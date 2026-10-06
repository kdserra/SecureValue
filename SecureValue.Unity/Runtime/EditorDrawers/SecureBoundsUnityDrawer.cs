#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureBounds"/>: center and extents rows.</summary>
	[CustomPropertyDrawer(typeof(SecureBounds))]
	internal class SecureBoundsUnityDrawer : SecureValueDrawer<SecureBounds, Bounds>
	{
		protected override Bounds Get(SecureBounds wrapper) => wrapper;

		protected override SecureBounds SetValue(Bounds value) => value;

		/// <summary>Two rows: center and extents.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(2);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Bounds value,
			Action<Bounds> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector3 nextCenter = EditorGUI.Vector3Field(RowRect(position, 0), label, value.center);
			Vector3 nextExtents = EditorGUI.Vector3Field(
				RowRect(position, 1),
				"Extents",
				value.extents
			);
			if (EditorGUI.EndChangeCheck())
			{
				write(new Bounds(nextCenter, nextExtents * 2));
			}
		}
	}
}
#endif
