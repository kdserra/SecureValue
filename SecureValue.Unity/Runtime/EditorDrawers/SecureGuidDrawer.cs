#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureGuid"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureGuid))]
	internal class SecureGuidDrawer : SecureValueDrawer<SecureGuid, Guid>
	{
		protected override Guid Get(SecureGuid wrapper) => wrapper;

		protected override SecureGuid SetValue(Guid value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			Guid value,
			Action<Guid> write
		)
		{
			const float buttonWidth = 70f;
			Rect fieldRect = new Rect(
				position.x,
				position.y,
				position.width - buttonWidth - 4f,
				position.height
			);
			Rect buttonRect = new Rect(
				position.xMax - buttonWidth,
				position.y,
				buttonWidth,
				position.height
			);

			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(fieldRect, label, value.ToString("D"));
			if (EditorGUI.EndChangeCheck() && Guid.TryParse(next, out Guid parsed))
			{
				write(parsed);
			}

			if (GUI.Button(buttonRect, "Generate"))
			{
				write(Guid.NewGuid());
			}
		}
	}
}
#endif
