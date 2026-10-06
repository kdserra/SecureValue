#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureDateTime"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureDateTime))]
	internal class SecureDateTimeDrawer : SecureValueDrawer<SecureDateTime, DateTime>
	{
		protected override DateTime Get(SecureDateTime wrapper) => wrapper;

		protected override SecureDateTime SetValue(DateTime value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			DateTime value,
			Action<DateTime> write
		)
		{
			const float buttonWidth = 50f;
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
			string next = EditorGUI.TextField(
				fieldRect,
				label,
				value.ToString("O", CultureInfo.InvariantCulture)
			);
			if (
				EditorGUI.EndChangeCheck()
				&& DateTime.TryParse(
					next,
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out DateTime parsed
				)
			)
			{
				write(parsed);
			}

			if (GUI.Button(buttonRect, "Now"))
			{
				write(DateTime.UtcNow);
			}
		}
	}
}
#endif
