#nullable enable
using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="decimal"/> value (128-bit payload).</summary>
	[Serializable]
	public partial struct SecureDecimal
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell128 _cell;

		/// <summary>Secures a decimal value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureDecimal(decimal value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			// decimal.GetBits allocates: reinterpret the 16 bytes in place instead.
			// Runtime memory order is NOT GetBits order — the CLR lays decimal
			// out as flags,hi,lo,mid, so explicit offsets reproduce GetBits
			// exactly: lo=bytes[8..12], mid=bytes[12..16], hi=bytes[4..8],
			// flags=bytes[0..4]. Little-endian fast path; big-endian falls
			// back to GetBits for correctness.
			if (BitConverter.IsLittleEndian)
			{
				Span<decimal> one = stackalloc decimal[1];
				one[0] = value;
				ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(one);
				uint lo32 = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(8));
				uint mid32 = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(12));
				uint hi32 = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4));
				uint flags32 = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
				_cell.Protect(
					(ulong)lo32 | ((ulong)mid32 << 32),
					(ulong)hi32 | ((ulong)flags32 << 32)
				);
			}
			else
			{
				int[] bits = decimal.GetBits(value);
				_cell.Protect(
					(ulong)(uint)bits[0] | ((ulong)(uint)bits[1] << 32),
					(ulong)(uint)bits[2] | ((ulong)(uint)bits[3] << 32)
				);
			}
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public decimal Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cell.Unprotect();
				return new decimal(
					unchecked((int)(uint)lo),
					unchecked((int)(uint)(lo >> 32)),
					unchecked((int)(uint)hi),
					(hi & 0x8000000000000000UL) != 0UL,
					(byte)((hi >> 48) & 0xFFUL)
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out decimal value)
		{
			if (_cell.TryUnprotect(out ulong lo, out ulong hi))
			{
				value = new decimal(
					unchecked((int)(uint)lo),
					unchecked((int)(uint)(lo >> 32)),
					unchecked((int)(uint)hi),
					(hi & 0x8000000000000000UL) != 0UL,
					(byte)((hi >> 48) & 0xFFUL)
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

		/// <summary>Encrypts the type default when never initialized.</summary>
		internal void EnsureInitialized()
		{
			if (_cell.IsUnset)
			{
				this = new SecureDecimal(default);
			}
		}

		uint[] ISecureSerialization.SaveToSerialized()
		{
			Span<ulong> words = stackalloc ulong[12];
			KeySet saveKey = Vault.NewStorageKey();
			_cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(8));
			return SerializationFormat.Pack(words);
		}

		void ISecureSerialization.LoadFromSerialized(uint[]? packed) => LoadFromSerialized(packed);

		private void LoadFromSerialized(uint[]? packed)
		{
			if (packed == null)
			{
				// Never serialized: materialize a default (bad lengths fail closed on read).
				EnsureInitialized();
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
			EnsureInitialized();
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

		/// <summary>Converts a plain decimal value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureDecimal(decimal value) => new SecureDecimal(value);

		/// <summary>Converts back to the plain decimal value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator decimal(SecureDecimal value) => value.Decrypted;

		/// <summary>Compares this value with another secured decimal for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureDecimal other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureDecimal other && Equals(other);

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

		/// <summary>Compares this value with another secured decimal.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureDecimal other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureDecimal other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureDecimal.", nameof(obj));

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDecimal Parse(string value) => new SecureDecimal(decimal.Parse(value));

		/// <summary>Parses a string into its secured form with the specified style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDecimal Parse(string value, NumberStyles style) =>
			new SecureDecimal(decimal.Parse(value, style));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDecimal Parse(string value, IFormatProvider? provider) =>
			new SecureDecimal(decimal.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDecimal Parse(
			string value,
			NumberStyles style,
			IFormatProvider? provider
		) => new SecureDecimal(decimal.Parse(value, style, provider));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureDecimal result)
		{
			if (decimal.TryParse(value, out decimal plain))
			{
				result = new SecureDecimal(plain);
				return true;
			}
			result = default;
			return false;
		}

		/// <summary>Tries to parse a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(
			string? value,
			NumberStyles style,
			IFormatProvider? provider,
			out SecureDecimal result
		)
		{
			if (decimal.TryParse(value, style, provider, out decimal plain))
			{
				result = new SecureDecimal(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
