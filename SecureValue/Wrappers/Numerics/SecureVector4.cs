#nullable enable
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SecureValue.Numerics
{
	/// <summary>Memory-encrypted <see cref="Vector4"/>.</summary>
	[Serializable]
	public partial struct SecureVector4
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell128 _cell;

		/// <summary>Secures a Vector4 value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureVector4(Vector4 value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			float a,
				b,
				c,
				d;
			a = value.X;
			b = value.Y;
			c = value.Z;
			d = value.W;
			_cell.Protect(
				(ulong)(uint)BitConverter.SingleToInt32Bits(a)
					| ((ulong)(uint)BitConverter.SingleToInt32Bits(b) << 32),
				(ulong)(uint)BitConverter.SingleToInt32Bits(c)
					| ((ulong)(uint)BitConverter.SingleToInt32Bits(d) << 32)
			);
		}

		/// <summary>Gets the decrypted plain value (default when never assigned).</summary>
		public Vector4 Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cell.Unprotect();
				return new Vector4(
					BitConverter.Int32BitsToSingle(unchecked((int)lo)),
					BitConverter.Int32BitsToSingle(unchecked((int)(lo >> 32))),
					BitConverter.Int32BitsToSingle(unchecked((int)hi)),
					BitConverter.Int32BitsToSingle(unchecked((int)(hi >> 32)))
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Vector4 value)
		{
			if (_cell.TryUnprotect(out ulong lo, out ulong hi))
			{
				value = new Vector4(
					BitConverter.Int32BitsToSingle(unchecked((int)lo)),
					BitConverter.Int32BitsToSingle(unchecked((int)(lo >> 32))),
					BitConverter.Int32BitsToSingle(unchecked((int)hi)),
					BitConverter.Int32BitsToSingle(unchecked((int)(hi >> 32)))
				);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>True when never assigned.</summary>
		public bool IsUnset
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _cell.IsUnset;
		}

		uint[] ISecureSerialization.SaveToSerialized()
		{
			if (IsUnset)
			{
				return Array.Empty<uint>();
			}
			Span<ulong> words = stackalloc ulong[12];
			KeySet saveKey = Vault.NewStorageKey();
			_cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(8));
			return SerializationFormat.Pack(words);
		}

		void ISecureSerialization.LoadFromSerialized(uint[]? packed) => LoadFromSerialized(packed);

		private void LoadFromSerialized(uint[]? packed)
		{
			if (packed == null || packed.Length == 0)
			{
				// Never serialized: stay unset (reads return default).
				return;
			}
			Span<ulong> words = stackalloc ulong[12];
			if (SerializationFormat.TryUnpack(packed, words))
			{
				KeySet saveKey = KeySet.FromWords(words.Slice(8));
				_cell.RestoreStorageWords(words, saveKey);
			}
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField, UnityEngine.HideInInspector]
		private uint[]? _serialized;

		void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			_serialized = ((ISecureSerialization)this).SaveToSerialized();
		}

		void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// Restores through a direct instance call: casting this to
			// ISecureSerialization would box the struct and the restored
			// cell would be lost with the box.
			LoadFromSerialized(_serialized);
		}
#endif

		/// <summary>Converts a plain Vector4 value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureVector4(Vector4 value) => new SecureVector4(value);

		/// <summary>Converts back to the plain Vector4 value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector4(SecureVector4 value) => value.Decrypted;

		/// <summary>Compares this value with another secured Vector4 for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureVector4 other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureVector4 other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Returns the decrypted value formatted with the specified format.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToString(string? format) => Decrypted.ToString(format);

		/// <summary>Returns the decrypted value formatted with the specified format and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToString(string? format, IFormatProvider? formatProvider) =>
			Decrypted.ToString(format, formatProvider);

		/// <summary>Tests two secured Vector4 values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureVector4 left, SecureVector4 right) =>
			left.Equals(right);

		/// <summary>Tests two secured Vector4 values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureVector4 left, SecureVector4 right) =>
			!left.Equals(right);

		/// <summary>Adds two secured Vector4 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector4 operator +(SecureVector4 a, SecureVector4 b) =>
			new SecureVector4(a.Decrypted + b.Decrypted);

		/// <summary>Subtracts two secured Vector4 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector4 operator -(SecureVector4 a, SecureVector4 b) =>
			new SecureVector4(a.Decrypted - b.Decrypted);

		/// <summary>Negates a secured Vector4 value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector4 operator -(SecureVector4 a) => new SecureVector4(-a.Decrypted);

		/// <summary>Multiplies two secured Vector4 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector4 operator *(SecureVector4 a, SecureVector4 b) =>
			new SecureVector4(a.Decrypted * b.Decrypted);

		/// <summary>Divides two secured Vector4 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector4 operator /(SecureVector4 a, SecureVector4 b) =>
			new SecureVector4(a.Decrypted / b.Decrypted);
	}
}
