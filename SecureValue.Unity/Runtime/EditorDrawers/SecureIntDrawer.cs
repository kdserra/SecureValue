#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureInt"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureInt))]
	internal class SecureIntDrawer : SecureValueDrawer<SecureInt, int>
	{
		protected override int Get(SecureInt wrapper) => wrapper;

		protected override SecureInt SetValue(int value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			int value,
			Action<int> write
		)
		{
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.IntField(position, label, value);
			if (EditorGUI.EndChangeCheck())
			{
				write(next);
			}
		}
	}
}
#endif
