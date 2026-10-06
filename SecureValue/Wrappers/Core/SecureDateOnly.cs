#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SecureValue
{
#if NET6_0_OR_GREATER
	/// <summary>Memory-encrypted <see cref="DateOnly"/> value.</summary>
	[Serializable]
	public partial struct SecureDateOnly : ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures a DateOnly value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureDateOnly(DateOnly value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell = default;
			_cell.Protect((ulong)value.DayNumber);
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public DateOnly Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DateOnly.FromDayNumber((int)_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out DateOnly value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = DateOnly.FromDayNumber((int)plain);
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
				this = new SecureDateOnly(default);
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

		/// <summary>Converts a plain DateOnly value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureDateOnly(DateOnly value) => new SecureDateOnly(value);

		/// <summary>Converts back to the plain DateOnly value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DateOnly(SecureDateOnly value) => value.Decrypted;

		/// <summary>Compares this value with another secured DateOnly for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureDateOnly other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureDateOnly other && Equals(other);

		/// <summary>Compares this value with another secured DateOnly.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureDateOnly other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureDateOnly other
				? CompareTo(other)
				: throw new ArgumentException(
					"Object must be of type SecureDateOnly.",
					nameof(obj)
				);

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

		/// <summary>Tests two secured DateOnly values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureDateOnly left, SecureDateOnly right) =>
			left.Equals(right);

		/// <summary>Tests two secured DateOnly values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureDateOnly left, SecureDateOnly right) =>
			!left.Equals(right);

		/// <summary>Compares two secured DateOnly values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(SecureDateOnly left, SecureDateOnly right) =>
			left.Decrypted < right.Decrypted;

		/// <summary>Compares two secured DateOnly values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(SecureDateOnly left, SecureDateOnly right) =>
			left.Decrypted <= right.Decrypted;

		/// <summary>Compares two secured DateOnly values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(SecureDateOnly left, SecureDateOnly right) =>
			left.Decrypted > right.Decrypted;

		/// <summary>Compares two secured DateOnly values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(SecureDateOnly left, SecureDateOnly right) =>
			left.Decrypted >= right.Decrypted;

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDateOnly Parse(string value) =>
			new SecureDateOnly(DateOnly.Parse(value));

		/// <summary>Parses a string into its secured form with the specified provider.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDateOnly Parse(string value, IFormatProvider? provider) =>
			new SecureDateOnly(DateOnly.Parse(value, provider));

		/// <summary>Parses a string into its secured form with the specified provider and style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureDateOnly Parse(
			string value,
			IFormatProvider? provider,
			DateTimeStyles style
		) => new SecureDateOnly(DateOnly.Parse(value, provider, style));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureDateOnly result)
		{
			if (DateOnly.TryParse(value, out DateOnly plain))
			{
				result = new SecureDateOnly(plain);
				return true;
			}
			result = default;
			return false;
		}

		/// <summary>Tries to parse a string into its secured form with the specified provider and style.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(
			string? value,
			IFormatProvider? provider,
			DateTimeStyles style,
			out SecureDateOnly result
		)
		{
			if (DateOnly.TryParse(value, provider, style, out DateOnly plain))
			{
				result = new SecureDateOnly(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
#endif
}
