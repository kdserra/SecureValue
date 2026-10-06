#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SecureValue;
using System.Globalization;

namespace SecureValue.Unity
{
	/// <summary>Inspector drawer for the Unity <see cref="SecureLayerMask"/>: a single editable field.</summary>
	[CustomPropertyDrawer(typeof(SecureLayerMask))]
	internal class SecureLayerMaskUnityDrawer : SecureValueDrawer<SecureLayerMask, LayerMask>
	{
		protected override LayerMask Get(SecureLayerMask wrapper) => wrapper;

		protected override SecureLayerMask SetValue(LayerMask value) => value;

		protected override void DrawControl(
			Rect position,
			GUIContent label,
			LayerMask value,
			Action<LayerMask> write
		)
		{
			// MaskField works on concatenated layer-list bits, NOT LayerMask.value
			// bits: with gaps in the layer list the two differ, so round-trip
			// through InternalEditorUtility's converters on both sides.
			EditorGUI.BeginChangeCheck();
			int next = EditorGUI.MaskField(
				position,
				label,
				UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(value),
				UnityEditorInternal.InternalEditorUtility.layers
			);
			if (EditorGUI.EndChangeCheck())
			{
				write(
					UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(
						next
					)
				);
			}
		}
	}
}
#endif
