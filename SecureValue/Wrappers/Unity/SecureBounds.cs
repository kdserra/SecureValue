#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="Bounds"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecureBounds
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell128 _cellA;
		private Cell _cellB;

		/// <summary>Secures a Bounds value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureBounds(Bounds value)
		{
			_serialized = default;
			_cellA = default;
			_cellB = default;
			Vector3 c = value.center;
			Vector3 s = value.size;
			_cellA.Protect(W(c.x, c.y), W(c.z, s.x));
			_cellB.Protect(W(s.y, s.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong W(float x, float y) =>
			(ulong)(uint)BitConverter.SingleToInt32Bits(x)
			| ((ulong)(uint)BitConverter.SingleToInt32Bits(y) << 32);

		/// <summary>Gets the decrypted plain value.</summary>
		public Bounds Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cellA.Unprotect();
				ulong rest = _cellB.Unprotect();
				return new Bounds(
					new Vector3(F(lo), F(lo >> 32), F(hi)),
					new Vector3(F(hi >> 32), F(rest), F(rest >> 32))
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Bounds value)
		{
			bool okA = _cellA.TryUnprotect(out ulong lo, out ulong hi);
			bool okB = _cellB.TryUnprotect(out ulong rest);
			if (okA && okB)
			{
				value = new Bounds(
					new Vector3(F(lo), F(lo >> 32), F(hi)),
					new Vector3(F(hi >> 32), F(rest), F(rest >> 32))
				);
				return true;
			}
			value = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float F(ulong w) => BitConverter.Int32BitsToSingle(unchecked((int)(uint)w));

		/// <summary>True when never assigned.</summary>
		public bool IsUnset
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _cellA.IsUnset && _cellB.IsUnset;
		}

		/// <summary>Encrypts the type default when never initialized.</summary>
		internal void EnsureInitialized()
		{
			if (_cellA.IsUnset && _cellB.IsUnset)
			{
				this = new SecureBounds(default);
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
			Span<ulong> words = stackalloc ulong[18];
			KeySet saveKey = Vault.NewStorageKey();
			_cellA.CopyStorageWords(words, saveKey);
			_cellB.CopyStorageWords(words.Slice(8), saveKey);
			saveKey.CopyTo(words.Slice(14));
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
			Span<ulong> words = stackalloc ulong[18];
			if (SerializationFormat.TryUnpack(packed, words))
			{
				KeySet saveKey = KeySet.FromWords(words.Slice(14));
				_cellA.RestoreStorageWords(words, saveKey);
				_cellB.RestoreStorageWords(words.Slice(8), saveKey);
			}
		}

		/// <summary>Converts a plain Bounds value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureBounds(Bounds value) => new SecureBounds(value);

		/// <summary>Converts back to the plain Bounds value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Bounds(SecureBounds value) => value.Decrypted;

		/// <summary>Compares this value with another secured Bounds for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureBounds other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureBounds other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Bounds values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureBounds left, SecureBounds right) => left.Equals(right);

		/// <summary>Tests two secured Bounds values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureBounds left, SecureBounds right) =>
			!left.Equals(right);
	}
}
#endif
