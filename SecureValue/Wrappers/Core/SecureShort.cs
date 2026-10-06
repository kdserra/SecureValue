#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="short"/> value.</summary>
	[Serializable]
	public partial struct SecureShort
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures a short value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureShort(short value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell = default;
			_cell.Protect(Bits.From(value));
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public short Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Bits.ToShort(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out short value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = Bits.ToShort(plain);
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
				this = new SecureShort(default);
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

		/// <summary>Converts a plain short value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureShort(short value) => new SecureShort(value);

		/// <summary>Converts back to the plain short value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator short(SecureShort value) => value.Decrypted;

		/// <summary>Compares this value with another secured short for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureShort other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureShort other && Equals(other);

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

		/// <summary>Compares this value with another secured short.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureShort other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureShort other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureShort.", nameof(obj));

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureShort Parse(string value) => new SecureShort(short.Parse(value));

		/// <summary>Parses a string into its secured form with the specified style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureShort Parse(string value, NumberStyles style) =>
			new SecureShort(short.Parse(value, style));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureShort Parse(string value, IFormatProvider? provider) =>
			new SecureShort(short.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureShort Parse(
			string value,
			NumberStyles style,
			IFormatProvider? provider
		) => new SecureShort(short.Parse(value, style, provider));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureShort result)
		{
			if (short.TryParse(value, out short plain))
			{
				result = new SecureShort(plain);
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
			out SecureShort result
		)
		{
			if (short.TryParse(value, style, provider, out short plain))
			{
				result = new SecureShort(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
