#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureRect"/>: a single unlabeled Vector4 row (x, y, width, height).</summary>
	[CustomPropertyDrawer(typeof(SecureRect))]
	internal class SecureRectUnityDrawer : SecureValueDrawer<SecureRect, Rect>
	{
		protected override Rect Get(SecureRect wrapper) => wrapper;

		protected override SecureRect SetValue(Rect value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Rect value,
			Action<Rect> write
		)
		{
			// One Vector4 row holds all four components; its boxes carry no sub-labels, so no
			// native RectField behavior (which overpainted the following row) remains.
			Vector4 fields = new Vector4(value.x, value.y, value.width, value.height);
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				write(new Rect(next.x, next.y, next.z, next.w));
			}
		}
	}
}
#endif
