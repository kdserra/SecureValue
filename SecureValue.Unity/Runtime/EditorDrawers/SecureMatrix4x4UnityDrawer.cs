#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureMatrix4x4"/>: label on its own line, four indented rows of four fields.</summary>
	[CustomPropertyDrawer(typeof(SecureMatrix4x4))]
	internal class SecureMatrix4x4UnityDrawer : SecureValueDrawer<SecureMatrix4x4, Matrix4x4>
	{
		protected override Matrix4x4 Get(SecureMatrix4x4 wrapper) => wrapper;

		protected override SecureMatrix4x4 SetValue(Matrix4x4 value) => value;

		/// <summary>Five rows: label plus four rows of four fields.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(5);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Matrix4x4 value,
			Action<Matrix4x4> write
		)
		{
			EditorGUI.LabelField(RowRect(position, 0), label);
			Vector4 row0 = new Vector4(value.m00, value.m01, value.m02, value.m03);
			Vector4 row1 = new Vector4(value.m10, value.m11, value.m12, value.m13);
			Vector4 row2 = new Vector4(value.m20, value.m21, value.m22, value.m23);
			Vector4 row3 = new Vector4(value.m30, value.m31, value.m32, value.m33);
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
					Matrix4x4 m = default;
					m.m00 = nextRow0.x;
					m.m01 = nextRow0.y;
					m.m02 = nextRow0.z;
					m.m03 = nextRow0.w;
					m.m10 = nextRow1.x;
					m.m11 = nextRow1.y;
					m.m12 = nextRow1.z;
					m.m13 = nextRow1.w;
					m.m20 = nextRow2.x;
					m.m21 = nextRow2.y;
					m.m22 = nextRow2.z;
					m.m23 = nextRow2.w;
					m.m30 = nextRow3.x;
					m.m31 = nextRow3.y;
					m.m32 = nextRow3.z;
					m.m33 = nextRow3.w;
					write(m);
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
