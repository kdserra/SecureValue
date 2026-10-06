#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecurePlane"/>: normal and distance fields.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecurePlane))]
	internal class SecurePlaneDrawer
		: SecureValueDrawer<SecureValue.Numerics.SecurePlane, System.Numerics.Plane>
	{
		protected override System.Numerics.Plane Get(SecureValue.Numerics.SecurePlane wrapper) =>
			wrapper;

		protected override SecureValue.Numerics.SecurePlane SetValue(System.Numerics.Plane value) =>
			value;

		/// <summary>Two rows: normal and distance.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(2);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			System.Numerics.Plane value,
			Action<System.Numerics.Plane> write
		)
		{
			Vector3 normal = new Vector3(value.Normal.X, value.Normal.Y, value.Normal.Z);
			EditorGUI.BeginChangeCheck();
			Vector3 nextNormal = EditorGUI.Vector3Field(RowRect(position, 0), label, normal);
			float nextDistance = EditorGUI.FloatField(RowRect(position, 1), "Distance", value.D);
			if (EditorGUI.EndChangeCheck())
			{
				write(
					new System.Numerics.Plane(
						nextNormal.x,
						nextNormal.y,
						nextNormal.z,
						nextDistance
					)
				);
			}
		}
	}
}
#endif
