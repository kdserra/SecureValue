#nullable enable
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SecureValue.Numerics
{
	/// <summary>Memory-encrypted <see cref="Matrix3x2"/>.</summary>
	[Serializable]
	public partial struct SecureMatrix3x2
		: ISecureSerialization
#if UNITY_5_3_OR_NEWER
			,
			UnityEngine.ISerializationCallbackReceiver
#endif
	{
		private Cell128 _cellA;
		private Cell _cellB;

		/// <summary>Secures a Matrix3x2 value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureMatrix3x2(Matrix3x2 m)
		{
#if UNITY_5_3_OR_NEWER
			_serialized = default;
#endif
			_cellA = default;
			_cellB = default;
			_cellB = default;
			_cellA.Protect(W(m.M11, m.M12), W(m.M21, m.M22));
			_cellB.Protect(W(m.M31, m.M32));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong W(float x, float y) =>
			(ulong)(uint)BitConverter.SingleToInt32Bits(x)
			| ((ulong)(uint)BitConverter.SingleToInt32Bits(y) << 32);

		/// <summary>Gets the decrypted plain value.</summary>
		public Matrix3x2 Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cellA.Unprotect();
				ulong rest = _cellB.Unprotect();
				return new Matrix3x2(
					F(lo),
					F(lo >> 32),
					F(hi),
					F(hi >> 32),
					F(rest),
					F(rest >> 32)
				);
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Matrix3x2 value)
		{
			bool okA = _cellA.TryUnprotect(out ulong lo, out ulong hi);
			bool okB = _cellB.TryUnprotect(out ulong rest);
			if (okA && okB)
			{
				value = new Matrix3x2(
					F(lo),
					F(lo >> 32),
					F(hi),
					F(hi >> 32),
					F(rest),
					F(rest >> 32)
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
				this = new SecureMatrix3x2(default);
			}
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

		/// <summary>Converts a plain Matrix3x2 value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureMatrix3x2(Matrix3x2 value) =>
			new SecureMatrix3x2(value);

		/// <summary>Converts back to the plain Matrix3x2 value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Matrix3x2(SecureMatrix3x2 value) => value.Decrypted;

		/// <summary>Compares this value with another secured Matrix3x2 for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureMatrix3x2 other) => Decrypted == other.Decrypted;

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureMatrix3x2 other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Matrix3x2 values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureMatrix3x2 left, SecureMatrix3x2 right) =>
			left.Equals(right);

		/// <summary>Tests two secured Matrix3x2 values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureMatrix3x2 left, SecureMatrix3x2 right) =>
			!left.Equals(right);

		/// <summary>Adds two secured Matrix3x2 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureMatrix3x2 operator +(SecureMatrix3x2 a, SecureMatrix3x2 b) =>
			new SecureMatrix3x2(a.Decrypted + b.Decrypted);

		/// <summary>Subtracts two secured Matrix3x2 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureMatrix3x2 operator -(SecureMatrix3x2 a, SecureMatrix3x2 b) =>
			new SecureMatrix3x2(a.Decrypted - b.Decrypted);

		/// <summary>Negates a secured Matrix3x2 value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureMatrix3x2 operator -(SecureMatrix3x2 a) =>
			new SecureMatrix3x2(-a.Decrypted);

		/// <summary>Multiplies two secured Matrix3x2 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureMatrix3x2 operator *(SecureMatrix3x2 a, SecureMatrix3x2 b) =>
			new SecureMatrix3x2(a.Decrypted * b.Decrypted);
	}
}
