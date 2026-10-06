#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureMatrix3x2"/>: label on its own line, two indented rows of three fields.</summary>
	[CustomPropertyDrawer(typeof(SecureMatrix3x2))]
	internal class SecureMatrix3x2Drawer : SecureValueDrawer<SecureMatrix3x2, Matrix3x2>
	{
		protected override Matrix3x2 Get(SecureMatrix3x2 wrapper) => wrapper;

		protected override SecureMatrix3x2 SetValue(Matrix3x2 value) => value;

		/// <summary>Three rows: label plus two rows of three fields.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(3);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Matrix3x2 value,
			Action<Matrix3x2> write
		)
		{
			EditorGUI.LabelField(RowRect(position, 0), label);
			UnityEngine.Vector3 row0 = new UnityEngine.Vector3(value.M11, value.M12, value.M21);
			UnityEngine.Vector3 row1 = new UnityEngine.Vector3(value.M22, value.M31, value.M32);
			EditorGUI.indentLevel++;
			try
			{
				EditorGUI.BeginChangeCheck();
				UnityEngine.Vector3 nextRow0 = EditorGUI.Vector3Field(
					RowRect(position, 1),
					GUIContent.none,
					row0
				);
				UnityEngine.Vector3 nextRow1 = EditorGUI.Vector3Field(
					RowRect(position, 2),
					GUIContent.none,
					row1
				);
				if (EditorGUI.EndChangeCheck())
				{
					write(
						new Matrix3x2(
							nextRow0.x,
							nextRow0.y,
							nextRow0.z,
							nextRow1.x,
							nextRow1.y,
							nextRow1.z
						)
					);
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
