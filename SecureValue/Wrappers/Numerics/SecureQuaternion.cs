#nullable enable
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SecureValue.Numerics
{
	/// <summary>Memory-encrypted <see cref="Quaternion"/>.</summary>
	[Serializable]
	public partial struct SecureQuaternion
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell128 _cell;

		/// <summary>Secures a Quaternion value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureQuaternion(Quaternion value)
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
		public Quaternion Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cell.Unprotect();
				return new Quaternion(
					BitConverter.Int32BitsToSingle(unchecked((int)lo)),
					BitConverter.Int32BitsToSingle(unchecked((int)(lo >> 32))),
					BitConverter.Int32BitsToSingle(unchecked((int)hi)),
					BitConverter.Int32BitsToSingle(unchecked((int)(hi >> 32)))
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Quaternion value)
		{
			if (_cell.TryUnprotect(out ulong lo, out ulong hi))
			{
				value = new Quaternion(
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

		/// <summary>Converts a plain Quaternion value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureQuaternion(Quaternion value) =>
			new SecureQuaternion(value);

		/// <summary>Converts back to the plain Quaternion value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Quaternion(SecureQuaternion value) => value.Decrypted;

		/// <summary>Compares this value with another secured Quaternion for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureQuaternion other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureQuaternion other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Quaternion values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureQuaternion left, SecureQuaternion right) =>
			left.Equals(right);

		/// <summary>Tests two secured Quaternion values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureQuaternion left, SecureQuaternion right) =>
			!left.Equals(right);

		/// <summary>Adds two secured Quaternion values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureQuaternion operator +(SecureQuaternion a, SecureQuaternion b) =>
			new SecureQuaternion(a.Decrypted + b.Decrypted);

		/// <summary>Subtracts two secured Quaternion values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureQuaternion operator -(SecureQuaternion a, SecureQuaternion b) =>
			new SecureQuaternion(a.Decrypted - b.Decrypted);

		/// <summary>Negates a secured Quaternion value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureQuaternion operator -(SecureQuaternion a) =>
			new SecureQuaternion(-a.Decrypted);

		/// <summary>Multiplies two secured Quaternion values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureQuaternion operator *(SecureQuaternion a, SecureQuaternion b) =>
			new SecureQuaternion(a.Decrypted * b.Decrypted);

		/// <summary>Divides two secured Quaternion values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureQuaternion operator /(SecureQuaternion a, SecureQuaternion b) =>
			new SecureQuaternion(a.Decrypted / b.Decrypted);
	}
}
