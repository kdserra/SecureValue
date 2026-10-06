#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureRectInt"/>: a single unlabeled Vector4 row (x, y, width, height).</summary>
	[CustomPropertyDrawer(typeof(SecureRectInt))]
	internal class SecureRectIntUnityDrawer : SecureValueDrawer<SecureRectInt, RectInt>
	{
		protected override RectInt Get(SecureRectInt wrapper) => wrapper;

		protected override SecureRectInt SetValue(RectInt value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			RectInt value,
			Action<RectInt> write
		)
		{
			// One Vector4 row holds all four components; its boxes carry no sub-labels, so there
			// is nothing to double. Order is positional: x, y, width, height.
			Vector4 fields = new Vector4(value.x, value.y, value.width, value.height);
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				write(new RectInt((int)next.x, (int)next.y, (int)next.z, (int)next.w));
			}
		}
	}
}
#endif
