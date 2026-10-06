#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="LayerMask"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecureLayerMask
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell _cell;

		/// <summary>Secures a LayerMask value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureLayerMask(LayerMask value)
		{
			_serialized = default;
			_cell = default;
			_cell.Protect(Bits.From(value.value));
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public LayerMask Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (LayerMask)Bits.ToInt(_cell.Unprotect());
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out LayerMask value)
		{
			if (_cell.TryUnprotect(out ulong plain))
			{
				value = (LayerMask)Bits.ToInt(plain);
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
				this = new SecureLayerMask(default);
			}
		}

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

		/// <summary>Converts a plain LayerMask value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureLayerMask(LayerMask value) =>
			new SecureLayerMask(value);

		/// <summary>Converts back to the plain LayerMask value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator LayerMask(SecureLayerMask value) => value.Decrypted;

		/// <summary>Compares this value with another secured LayerMask for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureLayerMask other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureLayerMask other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured LayerMask values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureLayerMask left, SecureLayerMask right) =>
			left.Equals(right);

		/// <summary>Tests two secured LayerMask values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureLayerMask left, SecureLayerMask right) =>
			!left.Equals(right);
	}
}
#endif
