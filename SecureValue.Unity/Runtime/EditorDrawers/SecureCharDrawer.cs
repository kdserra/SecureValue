#if UNITY_EDITOR
using System;
using SecureValue;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for <see cref="SecureChar"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureChar))]
	internal class SecureCharDrawer : SecureValueDrawer<SecureChar, char>
	{
		protected override char Get(SecureChar wrapper) => wrapper;

		protected override SecureChar SetValue(char value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			char value,
			Action<char> write
		)
		{
			// The displayed text is rebuilt from the stored value on every repaint,
			// so a naive length check eats in-progress edits: appending makes "SX"
			// (length 2, ignored, then snapped back to "S") and deleting makes ""
			// (unwritable to a char, snapped back). Diff against the displayed text
			// instead: a 2-char text containing the old char is an append/prepend,
			// so commit the OTHER char; anything else commits the last character
			// (select-all-and-type lands here). Every typing flow converges, because
			// even a delete-then-retype passes through a committable state.
			string current = value.ToString();
			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(position, label, current);
			if (EditorGUI.EndChangeCheck() && next.Length > 0 && next != current)
			{
				char picked;
				if (current.Length == 1 && next.Length == 2)
				{
					int index = next.IndexOf(current[0]);
					picked = index < 0 ? next[next.Length - 1] : next[1 - index];
				}
				else
				{
					picked = next[next.Length - 1];
				}
				write(picked);
			}
		}
	}
}
#endif
