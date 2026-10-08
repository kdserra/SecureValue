#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="double"/> value.</summary>
	[Serializable]
	public partial struct SecureDouble
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures a double value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureDouble(double value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell.Protect(Bits.From(value));
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public double Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Bits.ToDouble(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out double value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = Bits.ToDouble(plain);
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
				this = new SecureDouble(default);
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

		/// <summary>Converts a plain double value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureDouble(double value) => new SecureDouble(value);

		/// <summary>Converts back to the plain double value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator double(SecureDouble value) => value.Decrypted;

		/// <summary>Compares this value with another secured double for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureDouble other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureDouble other && Equals(other);

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

		/// <summary>Compares this value with another secured double.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureDouble other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureDouble other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureDouble.", nameof(obj));

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDouble Parse(string value) => new SecureDouble(double.Parse(value));

		/// <summary>Parses a string into its secured form with the specified style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDouble Parse(string value, NumberStyles style) =>
			new SecureDouble(double.Parse(value, style));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDouble Parse(string value, IFormatProvider? provider) =>
			new SecureDouble(double.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified style and provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDouble Parse(
			string value,
			NumberStyles style,
			IFormatProvider? provider
		) => new SecureDouble(double.Parse(value, style, provider));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureDouble result)
		{
			if (double.TryParse(value, out double plain))
			{
				result = new SecureDouble(plain);
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
			out SecureDouble result
		)
		{
			if (double.TryParse(value, style, provider, out double plain))
			{
				result = new SecureDouble(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
