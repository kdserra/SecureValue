#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="Plane"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecurePlane
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell128 _cell;

		/// <summary>Secures a Plane value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecurePlane(Plane value)
		{
			_serialized = default;
			_cell = default;
			Vector3 n = value.normal;
			_cell.Protect(
				(ulong)(uint)BitConverter.SingleToInt32Bits(n.x)
					| ((ulong)(uint)BitConverter.SingleToInt32Bits(n.y) << 32),
				(ulong)(uint)BitConverter.SingleToInt32Bits(n.z)
					| ((ulong)(uint)BitConverter.SingleToInt32Bits(value.distance) << 32)
			);
		}

		/// <summary>Gets the decrypted plain value.</summary>
		public Plane Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cell.Unprotect();
				// Unity's Plane constructors normalize the normal; the property setters
				// do not, so restore through them to keep the round trip bit-exact (same
				// latent issue as SecureRay, visible there only because Ray lacks
				// Vector3's approximate == to mask the drift).
				Plane result = default;
				result.normal = new Vector3(
					BitConverter.Int32BitsToSingle(unchecked((int)lo)),
					BitConverter.Int32BitsToSingle(unchecked((int)(lo >> 32))),
					BitConverter.Int32BitsToSingle(unchecked((int)hi))
				);
				result.distance = BitConverter.Int32BitsToSingle(unchecked((int)(hi >> 32)));
				return result;
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Plane value)
		{
			if (_cell.TryUnprotect(out ulong lo, out ulong hi))
			{
				// Unity's Plane constructors normalize the normal; the property setters
				// do not, so restore through them to keep the round trip bit-exact (same
				// latent issue as SecureRay, visible there only because Ray lacks
				// Vector3's approximate == to mask the drift).
				Plane result = default;
				result.normal = new Vector3(
					BitConverter.Int32BitsToSingle(unchecked((int)lo)),
					BitConverter.Int32BitsToSingle(unchecked((int)(lo >> 32))),
					BitConverter.Int32BitsToSingle(unchecked((int)hi))
				);
				result.distance = BitConverter.Int32BitsToSingle(unchecked((int)(hi >> 32)));
				value = result;
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
				this = new SecurePlane(default);
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

		/// <summary>Converts a plain Plane value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecurePlane(Plane value) => new SecurePlane(value);

		/// <summary>Converts back to the plain Plane value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Plane(SecurePlane value) => value.Decrypted;

		/// <summary>Compares this value with another secured Plane for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecurePlane other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecurePlane other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Plane values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecurePlane left, SecurePlane right) => left.Equals(right);

		/// <summary>Tests two secured Plane values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecurePlane left, SecurePlane right) => !left.Equals(right);
	}
}
#endif
