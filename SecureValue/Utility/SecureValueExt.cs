#nullable enable
using System;
using System.Collections.Generic;

namespace SecureValue
{
	/// <summary>
	/// Bulk conversion helpers between plain sequences and their memory-encrypted
	/// <c>Secure*</c> counterparts. The per-type <c>ToSecure</c>/<c>ToDecrypted</c>
	/// overloads cover every supported wrapper. All conversions preserve sequence order and
	/// are lazily enumerated. Ambiguous math types are supported in both variants:
	/// <c>SecureValue.Numerics</c> for <c>System.Numerics</c> and
	/// <c>SecureValue.Unity</c> for <c>UnityEngine</c> (Unity builds only).
	/// </summary>
	public static class SecureValueExt
	{
		/// <summary>Secures each <see cref="bool"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureBool> ToSecure(this IEnumerable<bool> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (bool v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="byte"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureByte> ToSecure(this IEnumerable<byte> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (byte v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="sbyte"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureSByte> ToSecure(this IEnumerable<sbyte> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (sbyte v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="short"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureShort> ToSecure(this IEnumerable<short> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (short v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="ushort"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureUShort> ToSecure(this IEnumerable<ushort> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (ushort v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="int"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureInt> ToSecure(this IEnumerable<int> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (int v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="uint"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureUInt> ToSecure(this IEnumerable<uint> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (uint v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="long"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureLong> ToSecure(this IEnumerable<long> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (long v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="ulong"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureULong> ToSecure(this IEnumerable<ulong> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (ulong v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="float"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureFloat> ToSecure(this IEnumerable<float> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (float v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="double"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureDouble> ToSecure(this IEnumerable<double> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (double v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="decimal"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureDecimal> ToSecure(this IEnumerable<decimal> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (decimal v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="char"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureChar> ToSecure(this IEnumerable<char> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (char v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="string"/> in the sequence, preserving order. Null elements are rejected.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null, or an element is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureString> ToSecure(this IEnumerable<string> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (string v in values)
			{
				yield return new SecureString(v);
			}
		}

		/// <summary>Secures each <see cref="Guid"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureGuid> ToSecure(this IEnumerable<Guid> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Guid v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="DateTime"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureDateTime> ToSecure(this IEnumerable<DateTime> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (DateTime v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="DateTimeOffset"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureDateTimeOffset> ToSecure(
			this IEnumerable<DateTimeOffset> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (DateTimeOffset v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="TimeSpan"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureTimeSpan> ToSecure(this IEnumerable<TimeSpan> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (TimeSpan v in values)
			{
				yield return v;
			}
		}

#if NET
		/// <summary>Secures each <see cref="System.Text.Rune"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureRune> ToSecure(this IEnumerable<System.Text.Rune> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Text.Rune v in values)
			{
				yield return v;
			}
		}
#endif

#if NET6_0_OR_GREATER
		/// <summary>Secures each <see cref="DateOnly"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureDateOnly> ToSecure(this IEnumerable<DateOnly> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (DateOnly v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="TimeOnly"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<SecureTimeOnly> ToSecure(this IEnumerable<TimeOnly> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (TimeOnly v in values)
			{
				yield return v;
			}
		}
#endif

		/// <summary>Secures each <see cref="System.Numerics.BigInteger"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureBigInteger> ToSecure(
			this IEnumerable<System.Numerics.BigInteger> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.BigInteger v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Complex"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureComplex> ToSecure(
			this IEnumerable<System.Numerics.Complex> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Complex v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Matrix3x2"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureMatrix3x2> ToSecure(
			this IEnumerable<System.Numerics.Matrix3x2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Matrix3x2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Matrix4x4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureMatrix4x4> ToSecure(
			this IEnumerable<System.Numerics.Matrix4x4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Matrix4x4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Plane"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecurePlane> ToSecure(
			this IEnumerable<System.Numerics.Plane> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Plane v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Quaternion"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureQuaternion> ToSecure(
			this IEnumerable<System.Numerics.Quaternion> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Quaternion v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Vector2"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureVector2> ToSecure(
			this IEnumerable<System.Numerics.Vector2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Vector2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Vector3"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureVector3> ToSecure(
			this IEnumerable<System.Numerics.Vector3> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Vector3 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each <see cref="System.Numerics.Vector4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Numerics.SecureVector4> ToSecure(
			this IEnumerable<System.Numerics.Vector4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (System.Numerics.Vector4 v in values)
			{
				yield return v;
			}
		}

#if UNITY_5_3_OR_NEWER
		/// <summary>Secures each UnityEngine <c>Vector2</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureVector2> ToSecure(
			this IEnumerable<UnityEngine.Vector2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Vector2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Vector2Int</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureVector2Int> ToSecure(
			this IEnumerable<UnityEngine.Vector2Int> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Vector2Int v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Vector3</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureVector3> ToSecure(
			this IEnumerable<UnityEngine.Vector3> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Vector3 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Vector3Int</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureVector3Int> ToSecure(
			this IEnumerable<UnityEngine.Vector3Int> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Vector3Int v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Vector4</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureVector4> ToSecure(
			this IEnumerable<UnityEngine.Vector4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Vector4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Rect</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureRect> ToSecure(
			this IEnumerable<UnityEngine.Rect> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Rect v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>RectInt</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureRectInt> ToSecure(
			this IEnumerable<UnityEngine.RectInt> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.RectInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Bounds</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureBounds> ToSecure(
			this IEnumerable<UnityEngine.Bounds> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Bounds v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>BoundsInt</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureBoundsInt> ToSecure(
			this IEnumerable<UnityEngine.BoundsInt> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.BoundsInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Color</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureColor> ToSecure(
			this IEnumerable<UnityEngine.Color> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Color v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Color32</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureColor32> ToSecure(
			this IEnumerable<UnityEngine.Color32> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Color32 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Quaternion</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureQuaternion> ToSecure(
			this IEnumerable<UnityEngine.Quaternion> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Quaternion v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Matrix4x4</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureMatrix4x4> ToSecure(
			this IEnumerable<UnityEngine.Matrix4x4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Matrix4x4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Plane</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecurePlane> ToSecure(
			this IEnumerable<UnityEngine.Plane> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Plane v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>Ray</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureRay> ToSecure(
			this IEnumerable<UnityEngine.Ray> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.Ray v in values)
			{
				yield return v;
			}
		}

		/// <summary>Secures each UnityEngine <c>LayerMask</c> in the sequence, preserving order.</summary>
		/// <param name="values">The plain values to secure.</param>
		/// <returns>The secured values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Unity.SecureLayerMask> ToSecure(
			this IEnumerable<UnityEngine.LayerMask> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (UnityEngine.LayerMask v in values)
			{
				yield return v;
			}
		}
#endif

		/// <summary>Decrypts each <see cref="SecureBool"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<bool> ToDecrypted(this IEnumerable<SecureBool> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureBool v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureByte"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<byte> ToDecrypted(this IEnumerable<SecureByte> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureByte v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureSByte"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<sbyte> ToDecrypted(this IEnumerable<SecureSByte> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureSByte v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureShort"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<short> ToDecrypted(this IEnumerable<SecureShort> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureShort v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureUShort"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<ushort> ToDecrypted(this IEnumerable<SecureUShort> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureUShort v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureInt"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<int> ToDecrypted(this IEnumerable<SecureInt> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureUInt"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<uint> ToDecrypted(this IEnumerable<SecureUInt> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureUInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureLong"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<long> ToDecrypted(this IEnumerable<SecureLong> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureLong v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureULong"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<ulong> ToDecrypted(this IEnumerable<SecureULong> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureULong v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureFloat"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<float> ToDecrypted(this IEnumerable<SecureFloat> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureFloat v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureDouble"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<double> ToDecrypted(this IEnumerable<SecureDouble> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureDouble v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureDecimal"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<decimal> ToDecrypted(this IEnumerable<SecureDecimal> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureDecimal v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureChar"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<char> ToDecrypted(this IEnumerable<SecureChar> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureChar v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureString"/> in the sequence, preserving order. Never yields null.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<string> ToDecrypted(this IEnumerable<SecureString> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureString v in values)
			{
				yield return v.Decrypted;
			}
		}

		/// <summary>Decrypts each <see cref="SecureGuid"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<Guid> ToDecrypted(this IEnumerable<SecureGuid> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureGuid v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureDateTime"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<DateTime> ToDecrypted(this IEnumerable<SecureDateTime> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureDateTime v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureDateTimeOffset"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<DateTimeOffset> ToDecrypted(
			this IEnumerable<SecureDateTimeOffset> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureDateTimeOffset v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureTimeSpan"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<TimeSpan> ToDecrypted(this IEnumerable<SecureTimeSpan> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureTimeSpan v in values)
			{
				yield return v;
			}
		}

#if NET
		/// <summary>Decrypts each <see cref="SecureRune"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Text.Rune> ToDecrypted(this IEnumerable<SecureRune> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureRune v in values)
			{
				yield return v;
			}
		}
#endif

#if NET6_0_OR_GREATER
		/// <summary>Decrypts each <see cref="SecureDateOnly"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<DateOnly> ToDecrypted(this IEnumerable<SecureDateOnly> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureDateOnly v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="SecureTimeOnly"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<TimeOnly> ToDecrypted(this IEnumerable<SecureTimeOnly> values)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (SecureTimeOnly v in values)
			{
				yield return v;
			}
		}
#endif

		/// <summary>Decrypts each <see cref="Numerics.SecureBigInteger"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.BigInteger> ToDecrypted(
			this IEnumerable<Numerics.SecureBigInteger> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureBigInteger v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureComplex"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Complex> ToDecrypted(
			this IEnumerable<Numerics.SecureComplex> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureComplex v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureMatrix3x2"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Matrix3x2> ToDecrypted(
			this IEnumerable<Numerics.SecureMatrix3x2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureMatrix3x2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureMatrix4x4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Matrix4x4> ToDecrypted(
			this IEnumerable<Numerics.SecureMatrix4x4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureMatrix4x4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecurePlane"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Plane> ToDecrypted(
			this IEnumerable<Numerics.SecurePlane> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecurePlane v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureQuaternion"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Quaternion> ToDecrypted(
			this IEnumerable<Numerics.SecureQuaternion> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureQuaternion v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureVector2"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Vector2> ToDecrypted(
			this IEnumerable<Numerics.SecureVector2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureVector2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureVector3"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Vector3> ToDecrypted(
			this IEnumerable<Numerics.SecureVector3> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureVector3 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each <see cref="Numerics.SecureVector4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<System.Numerics.Vector4> ToDecrypted(
			this IEnumerable<Numerics.SecureVector4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Numerics.SecureVector4 v in values)
			{
				yield return v;
			}
		}

#if UNITY_5_3_OR_NEWER
		/// <summary>Decrypts each Unity <see cref="Unity.SecureVector2"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Vector2> ToDecrypted(
			this IEnumerable<Unity.SecureVector2> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureVector2 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureVector2Int"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Vector2Int> ToDecrypted(
			this IEnumerable<Unity.SecureVector2Int> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureVector2Int v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureVector3"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Vector3> ToDecrypted(
			this IEnumerable<Unity.SecureVector3> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureVector3 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureVector3Int"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Vector3Int> ToDecrypted(
			this IEnumerable<Unity.SecureVector3Int> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureVector3Int v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureVector4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Vector4> ToDecrypted(
			this IEnumerable<Unity.SecureVector4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureVector4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureRect"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Rect> ToDecrypted(
			this IEnumerable<Unity.SecureRect> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureRect v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureRectInt"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.RectInt> ToDecrypted(
			this IEnumerable<Unity.SecureRectInt> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureRectInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureBounds"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Bounds> ToDecrypted(
			this IEnumerable<Unity.SecureBounds> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureBounds v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureBoundsInt"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.BoundsInt> ToDecrypted(
			this IEnumerable<Unity.SecureBoundsInt> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureBoundsInt v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureColor"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Color> ToDecrypted(
			this IEnumerable<Unity.SecureColor> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureColor v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureColor32"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Color32> ToDecrypted(
			this IEnumerable<Unity.SecureColor32> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureColor32 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureQuaternion"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Quaternion> ToDecrypted(
			this IEnumerable<Unity.SecureQuaternion> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureQuaternion v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureMatrix4x4"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Matrix4x4> ToDecrypted(
			this IEnumerable<Unity.SecureMatrix4x4> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureMatrix4x4 v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecurePlane"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Plane> ToDecrypted(
			this IEnumerable<Unity.SecurePlane> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecurePlane v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureRay"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.Ray> ToDecrypted(
			this IEnumerable<Unity.SecureRay> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureRay v in values)
			{
				yield return v;
			}
		}

		/// <summary>Decrypts each Unity <see cref="Unity.SecureLayerMask"/> in the sequence, preserving order.</summary>
		/// <param name="values">The secured values to decrypt.</param>
		/// <returns>The decrypted values, in the same order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is null (thrown on enumeration).</exception>
		public static IEnumerable<UnityEngine.LayerMask> ToDecrypted(
			this IEnumerable<Unity.SecureLayerMask> values
		)
		{
			if (values is null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			foreach (Unity.SecureLayerMask v in values)
			{
				yield return v;
			}
		}
#endif
	}
}
