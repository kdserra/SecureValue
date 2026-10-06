#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureRay"/>: label on its own line, indented origin and direction rows.</summary>
	[CustomPropertyDrawer(typeof(SecureRay))]
	internal class SecureRayUnityDrawer : SecureValueDrawer<SecureRay, Ray>
	{
		protected override Ray Get(SecureRay wrapper) => wrapper;

		protected override SecureRay SetValue(Ray value) => value;

		/// <summary>Three rows: label plus origin and direction.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(3);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Ray value,
			Action<Ray> write
		)
		{
			EditorGUI.LabelField(RowRect(position, 0), label);
			EditorGUI.indentLevel++;
			try
			{
				EditorGUI.BeginChangeCheck();
				Vector3 nextOrigin = EditorGUI.Vector3Field(
					RowRect(position, 1),
					"Origin",
					value.origin
				);
				Vector3 nextDirection = EditorGUI.Vector3Field(
					RowRect(position, 2),
					"Direction",
					value.direction
				);
				if (EditorGUI.EndChangeCheck())
				{
					// new Ray(origin, direction) normalizes the direction: editing
					// only the origin would perturb it (normalize is not idempotent,
					// so a second pass drifts by ULPs). Write the six floats directly
					// so the drawer round-trips exactly what it displays.
					Ray result = default;
					Span<float> floats = MemoryMarshal.Cast<Ray, float>(
						MemoryMarshal.CreateSpan(ref result, 1)
					);
					floats[0] = nextOrigin.x;
					floats[1] = nextOrigin.y;
					floats[2] = nextOrigin.z;
					floats[3] = nextDirection.x;
					floats[4] = nextDirection.y;
					floats[5] = nextDirection.z;
					write(result);
				}
			}
			finally
			{
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif
