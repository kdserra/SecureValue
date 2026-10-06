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
	/// <summary>Inspector drawer for <see cref="SecureBigInteger"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureBigInteger))]
	internal class SecureBigIntegerDrawer : SecureValueDrawer<SecureBigInteger, BigInteger>
	{
		protected override BigInteger Get(SecureBigInteger wrapper) => wrapper;

		protected override SecureBigInteger SetValue(BigInteger value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			BigInteger value,
			Action<BigInteger> write
		)
		{
			EditorGUI.BeginChangeCheck();
			string next = EditorGUI.TextField(
				position,
				label,
				value.ToString(CultureInfo.InvariantCulture)
			);
			if (
				EditorGUI.EndChangeCheck()
				&& BigInteger.TryParse(
					next,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out BigInteger parsed
				)
			)
			{
				write(parsed);
			}
		}
	}
}
#endif
