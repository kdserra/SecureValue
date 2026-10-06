#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecurePlane"/>: normal and distance fields.</summary>
	[CustomPropertyDrawer(typeof(SecurePlane))]
	internal class SecurePlaneUnityDrawer : SecureValueDrawer<SecurePlane, Plane>
	{
		protected override Plane Get(SecurePlane wrapper) => wrapper;

		protected override SecurePlane SetValue(Plane value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Plane value,
			Action<Plane> write
		)
		{
			Vector4 fields = new Vector4(
				value.normal.x,
				value.normal.y,
				value.normal.z,
				value.distance
			);
			EditorGUI.BeginChangeCheck();
			Vector4 next = EditorGUI.Vector4Field(position, label, fields);
			if (EditorGUI.EndChangeCheck())
			{
				// new Plane(normal, distance) normalizes the normal: editing any
				// single field (e.g. distance) would rescale the others. Restore
				// through the setters (which do not normalize) so the drawer
				// round-trips exactly what it displays, like the wrapper itself.
				Plane result = default;
				result.normal = new Vector3(next.x, next.y, next.z);
				result.distance = next.w;
				write(result);
			}
		}
	}
}
#endif
