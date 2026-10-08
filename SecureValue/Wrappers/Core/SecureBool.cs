#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Memory-encrypted <see cref="bool"/> value.</summary>
	[Serializable]
	public partial struct SecureBool
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell _cell;

		/// <summary>Secures a bool value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureBool(bool value)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cell = default;
			_cell.Protect(Bits.From(value));
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public bool Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Bits.ToBool(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out bool value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = Bits.ToBool(plain);
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
				this = new SecureBool(default);
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

		/// <summary>Converts a plain bool value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureBool(bool value) => new SecureBool(value);

		/// <summary>Converts back to the plain bool value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(SecureBool value) => value.Decrypted;

		/// <summary>Compares this value with another secured bool for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureBool other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureBool other && Equals(other);

		/// <summary>Compares this value with another secured bool.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(SecureBool other) => Decrypted.CompareTo(other.Decrypted);

		/// <summary>Compares this value with another object.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj) =>
			obj is SecureBool other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type SecureBool.", nameof(obj));

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured bool values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureBool left, SecureBool right) => left.Equals(right);

		/// <summary>Tests two secured bool values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureBool left, SecureBool right) => !left.Equals(right);

		/// <summary>Parses a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureBool Parse(string value) => new SecureBool(bool.Parse(value));

		/// <summary>Tries to parse a string into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string? value, out SecureBool result)
		{
			if (bool.TryParse(value, out bool plain))
			{
				result = new SecureBool(plain);
				return true;
			}
			result = default;
			return false;
		}
	}
}
