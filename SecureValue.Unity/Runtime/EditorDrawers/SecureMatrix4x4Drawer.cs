#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using SecureValue.Numerics;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the System.Numerics <see cref="SecureValue.Numerics.SecureMatrix4x4"/>: label on its own line, four indented rows of four fields.</summary>
	[CustomPropertyDrawer(typeof(SecureValue.Numerics.SecureMatrix4x4))]
	internal class SecureMatrix4x4Drawer
		: SecureValueDrawer<SecureValue.Numerics.SecureMatrix4x4, System.Numerics.Matrix4x4>
	{
		protected override System.Numerics.Matrix4x4 Get(
			SecureValue.Numerics.SecureMatrix4x4 wrapper
		) => wrapper;

		protected override SecureValue.Numerics.SecureMatrix4x4 SetValue(
			System.Numerics.Matrix4x4 value
		) => value;

		/// <summary>Five rows: label plus four rows of four fields.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(5);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			System.Numerics.Matrix4x4 value,
			Action<System.Numerics.Matrix4x4> write
		)
		{
			EditorGUI.LabelField(RowRect(position, 0), label);
			Vector4 row0 = new Vector4(value.M11, value.M12, value.M13, value.M14);
			Vector4 row1 = new Vector4(value.M21, value.M22, value.M23, value.M24);
			Vector4 row2 = new Vector4(value.M31, value.M32, value.M33, value.M34);
			Vector4 row3 = new Vector4(value.M41, value.M42, value.M43, value.M44);
			EditorGUI.indentLevel++;
			try
			{
				EditorGUI.BeginChangeCheck();
				Vector4 nextRow0 = EditorGUI.Vector4Field(
					RowRect(position, 1),
					GUIContent.none,
					row0
				);
				Vector4 nextRow1 = EditorGUI.Vector4Field(
					RowRect(position, 2),
					GUIContent.none,
					row1
				);
				Vector4 nextRow2 = EditorGUI.Vector4Field(
					RowRect(position, 3),
					GUIContent.none,
					row2
				);
				Vector4 nextRow3 = EditorGUI.Vector4Field(
					RowRect(position, 4),
					GUIContent.none,
					row3
				);
				if (EditorGUI.EndChangeCheck())
				{
					write(
						new System.Numerics.Matrix4x4(
							nextRow0.x,
							nextRow0.y,
							nextRow0.z,
							nextRow0.w,
							nextRow1.x,
							nextRow1.y,
							nextRow1.z,
							nextRow1.w,
							nextRow2.x,
							nextRow2.y,
							nextRow2.z,
							nextRow2.w,
							nextRow3.x,
							nextRow3.y,
							nextRow3.z,
							nextRow3.w
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
