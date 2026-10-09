#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="int"/> value.</summary>
	[Serializable]
	public partial struct SecureInt
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures an int value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureInt(int value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell.Protect(Bits.From(value));
		}

		/// <summary>Gets the decrypted plain value (default when never assigned).</summary>
		public int Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Bits.ToInt(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out int value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = Bits.ToInt(plain);
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
			Span<ulong> words = stackalloc ulong[10];
			KeySet saveKey = Vault.NewStorageKey();
			_cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(6));
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

		/// <summary>Converts a plain int value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureInt(int value) => new SecureInt(value);

		/// <summary>Converts back to the plain int value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(SecureInt value) => value.Decrypted;

		/// <summary>Compares this value with another secured int for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureInt other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureInt other && Equals(other);

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

		/// <summary>Compares this value with another secured int.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureInt other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureInt other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureInt.", nameof(obj));

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureInt Parse(string value) => new SecureInt(int.Parse(value));

		/// <summary>Parses a string into its secured form with the specified style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureInt Parse(string value, NumberStyles style) =>
			new SecureInt(int.Parse(value, style));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureInt Parse(string value, IFormatProvider? provider) =>
			new SecureInt(int.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureInt Parse(
			string value,
			NumberStyles style,
			IFormatProvider? provider
		) => new SecureInt(int.Parse(value, style, provider));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureInt result)
		{
			if (int.TryParse(value, out int plain))
			{
				result = new SecureInt(plain);
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
			out SecureInt result
		)
		{
			if (int.TryParse(value, style, provider, out int plain))
			{
				result = new SecureInt(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
