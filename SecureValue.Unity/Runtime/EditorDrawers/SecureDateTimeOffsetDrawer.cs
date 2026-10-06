#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureDateTimeOffset"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureDateTimeOffset))]
	internal class SecureDateTimeOffsetDrawer
		: SecureValueDrawer<SecureDateTimeOffset, DateTimeOffset>
	{
		protected override DateTimeOffset Get(SecureDateTimeOffset wrapper) => wrapper;

		protected override SecureDateTimeOffset SetValue(DateTimeOffset value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			DateTimeOffset value,
			Action<DateTimeOffset> write
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
				&& DateTimeOffset.TryParse(
					next,
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out DateTimeOffset parsed
				)
			)
			{
				write(parsed);
			}

			if (GUI.Button(buttonRect, "Now"))
			{
				write(DateTimeOffset.UtcNow);
			}
		}
	}
}
#endif
