#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureBoundsInt"/>: position and size rows.</summary>
	[CustomPropertyDrawer(typeof(SecureBoundsInt))]
	internal class SecureBoundsIntUnityDrawer : SecureValueDrawer<SecureBoundsInt, BoundsInt>
	{
		protected override BoundsInt Get(SecureBoundsInt wrapper) => wrapper;

		protected override SecureBoundsInt SetValue(BoundsInt value) => value;

		/// <summary>Two rows: position and size.</summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			HeightForLines(2);

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			BoundsInt value,
			Action<BoundsInt> write
		)
		{
			EditorGUI.BeginChangeCheck();
			Vector3Int nextPosition = EditorGUI.Vector3IntField(
				RowRect(position, 0),
				label,
				value.position
			);
			Vector3Int nextSize = EditorGUI.Vector3IntField(
				RowRect(position, 1),
				"Size",
				value.size
			);
			if (EditorGUI.EndChangeCheck())
			{
				write(new BoundsInt(nextPosition, nextSize));
			}
		}
	}
}
#endif
