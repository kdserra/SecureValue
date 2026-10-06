#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureTimeSpan"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureTimeSpan))]
	internal class SecureTimeSpanDrawer : SecureValueDrawer<SecureTimeSpan, TimeSpan>
	{
		protected override TimeSpan Get(SecureTimeSpan wrapper) => wrapper;

		protected override SecureTimeSpan SetValue(TimeSpan value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			TimeSpan value,
			Action<TimeSpan> write
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
				value.ToString("c", CultureInfo.InvariantCulture)
			);
			if (
				EditorGUI.EndChangeCheck()
				&& TimeSpan.TryParse(next, CultureInfo.InvariantCulture, out TimeSpan parsed)
			)
			{
				write(parsed);
			}

			if (GUI.Button(buttonRect, "Now"))
			{
				// "Now" for a duration: the current UTC time-of-day.
				write(DateTime.UtcNow.TimeOfDay);
			}
		}
	}
}
#endif
