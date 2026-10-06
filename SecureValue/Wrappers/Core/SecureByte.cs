#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="byte"/> value.</summary>
	[Serializable]
	public partial struct SecureByte
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures a byte value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureByte(byte value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell = default;
			_cell.Protect(Bits.From(value));
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public byte Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Bits.ToByte(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out byte value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = Bits.ToByte(plain);
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
				this = new SecureByte(default);
			}
		}

		uint[] ISecureSerialization.SaveToSerialized()
		{
			Span<ulong> words = stackalloc ulong[10];
			KeySet saveKey = Vault.NewStorageKey();
			_cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(6));
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
			Span<ulong> words = stackalloc ulong[10];
			if (SerializationFormat.TryUnpack(packed, words))
			{
				KeySet saveKey = KeySet.FromWords(words.Slice(6));
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

		/// <summary>Converts a plain byte value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureByte(byte value) => new SecureByte(value);

		/// <summary>Converts back to the plain byte value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator byte(SecureByte value) => value.Decrypted;

		/// <summary>Compares this value with another secured byte for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureByte other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureByte other && Equals(other);

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

		/// <summary>Compares this value with another secured byte.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureByte other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureByte other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureByte.", nameof(obj));

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureByte Parse(string value) => new SecureByte(byte.Parse(value));

		/// <summary>Parses a string into its secured form with the specified style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureByte Parse(string value, NumberStyles style) =>
			new SecureByte(byte.Parse(value, style));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureByte Parse(string value, IFormatProvider? provider) =>
			new SecureByte(byte.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureByte Parse(
			string value,
			NumberStyles style,
			IFormatProvider? provider
		) => new SecureByte(byte.Parse(value, style, provider));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureByte result)
		{
			if (byte.TryParse(value, out byte plain))
			{
				result = new SecureByte(plain);
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
			out SecureByte result
		)
		{
			if (byte.TryParse(value, style, provider, out byte plain))
			{
				result = new SecureByte(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
