#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="Vector3Int"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecureVector3Int
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell128 _cell;

		/// <summary>Secures a Vector3Int value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureVector3Int(Vector3Int value)
		{
			_serialized = default;
			_cell = default;
			_cell.Protect(EncLo(value), EncHi(value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong EncLo(Vector3Int v) => (ulong)(uint)v.x | ((ulong)(uint)v.y << 32);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong EncHi(Vector3Int v) => (ulong)(uint)v.z;

		/// <summary>Gets the decrypted plain value (default when never assigned).</summary>
		public Vector3Int Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cell.Unprotect();
				return new Vector3Int(
					unchecked((int)(uint)lo),
					unchecked((int)(uint)(lo >> 32)),
					unchecked((int)(uint)hi)
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Vector3Int value)
		{
			if (_cell.TryUnprotect(out ulong lo, out ulong hi))
			{
				value = new Vector3Int(
					unchecked((int)(uint)lo),
					unchecked((int)(uint)(lo >> 32)),
					unchecked((int)(uint)hi)
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

		/// <summary>Converts a plain Vector3Int value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureVector3Int(Vector3Int value) =>
			new SecureVector3Int(value);

		/// <summary>Converts back to the plain Vector3Int value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector3Int(SecureVector3Int value) => value.Decrypted;

		/// <summary>Compares this value with another secured Vector3Int for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureVector3Int other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureVector3Int other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Vector3Int values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureVector3Int left, SecureVector3Int right) =>
			left.Equals(right);

		/// <summary>Tests two secured Vector3Int values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureVector3Int left, SecureVector3Int right) =>
			!left.Equals(right);

		/// <summary>Adds two secured Vector3Int values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector3Int operator +(SecureVector3Int a, SecureVector3Int b) =>
			new SecureVector3Int(a.Decrypted + b.Decrypted);

		/// <summary>Subtracts two secured Vector3Int values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureVector3Int operator -(SecureVector3Int a, SecureVector3Int b) =>
			new SecureVector3Int(a.Decrypted - b.Decrypted);
	}
}
#endif
