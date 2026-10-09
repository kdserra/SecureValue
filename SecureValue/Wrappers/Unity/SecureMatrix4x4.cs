#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="Matrix4x4"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecureMatrix4x4
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell128 _cellA;
		private Cell128 _cellB;
		private Cell128 _cellC;
		private Cell128 _cellD;

		/// <summary>Secures a Matrix4x4 value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureMatrix4x4(Matrix4x4 m)
		{
			_serialized = default;
			_cellA = default;
			_cellB = default;
			_cellC = default;
			_cellD = default;
			_cellA.Protect(W(m.m00, m.m01), W(m.m02, m.m03));
			_cellB.Protect(W(m.m10, m.m11), W(m.m12, m.m13));
			_cellC.Protect(W(m.m20, m.m21), W(m.m22, m.m23));
			_cellD.Protect(W(m.m30, m.m31), W(m.m32, m.m33));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong W(float x, float y) =>
			(ulong)(uint)BitConverter.SingleToInt32Bits(x)
			| ((ulong)(uint)BitConverter.SingleToInt32Bits(y) << 32);

		/// <summary>Gets the decrypted plain value (default when never assigned).</summary>
		public Matrix4x4 Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var a = _cellA.Unprotect();
				var b = _cellB.Unprotect();
				var c = _cellC.Unprotect();
				var d = _cellD.Unprotect();
				Matrix4x4 m = default;
				m.m00 = F(a.Lo);
				m.m01 = F(a.Lo >> 32);
				m.m02 = F(a.Hi);
				m.m03 = F(a.Hi >> 32);
				m.m10 = F(b.Lo);
				m.m11 = F(b.Lo >> 32);
				m.m12 = F(b.Hi);
				m.m13 = F(b.Hi >> 32);
				m.m20 = F(c.Lo);
				m.m21 = F(c.Lo >> 32);
				m.m22 = F(c.Hi);
				m.m23 = F(c.Hi >> 32);
				m.m30 = F(d.Lo);
				m.m31 = F(d.Lo >> 32);
				m.m32 = F(d.Hi);
				m.m33 = F(d.Hi >> 32);
				return m;
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Matrix4x4 value)
		{
			bool okA = _cellA.TryUnprotect(out ulong loA, out ulong hiA);
			bool okB = _cellB.TryUnprotect(out ulong loB, out ulong hiB);
			bool okC = _cellC.TryUnprotect(out ulong loC, out ulong hiC);
			bool okD = _cellD.TryUnprotect(out ulong loD, out ulong hiD);
			if (okA && okB && okC && okD)
			{
				var a = (Lo: loA, Hi: hiA);
				var b = (Lo: loB, Hi: hiB);
				var c = (Lo: loC, Hi: hiC);
				var d = (Lo: loD, Hi: hiD);
				Matrix4x4 m = default;
				m.m00 = F(a.Lo);
				m.m01 = F(a.Lo >> 32);
				m.m02 = F(a.Hi);
				m.m03 = F(a.Hi >> 32);
				m.m10 = F(b.Lo);
				m.m11 = F(b.Lo >> 32);
				m.m12 = F(b.Hi);
				m.m13 = F(b.Hi >> 32);
				m.m20 = F(c.Lo);
				m.m21 = F(c.Lo >> 32);
				m.m22 = F(c.Hi);
				m.m23 = F(c.Hi >> 32);
				m.m30 = F(d.Lo);
				m.m31 = F(d.Lo >> 32);
				m.m32 = F(d.Hi);
				m.m33 = F(d.Hi >> 32);
				value = m;
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
			get => _cellA.IsUnset && _cellB.IsUnset && _cellC.IsUnset && _cellD.IsUnset;
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
			Span<ulong> words = stackalloc ulong[36];
			KeySet saveKey = Vault.NewStorageKey();
			_cellA.CopyStorageWords(words, saveKey);
			_cellB.CopyStorageWords(words.Slice(8), saveKey);
			_cellC.CopyStorageWords(words.Slice(16), saveKey);
			_cellD.CopyStorageWords(words.Slice(24), saveKey);
			saveKey.CopyTo(words.Slice(32));
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
			Span<ulong> words = stackalloc ulong[36];
			if (SerializationFormat.TryUnpack(packed, words))
			{
				KeySet saveKey = KeySet.FromWords(words.Slice(32));
				_cellA.RestoreStorageWords(words, saveKey);
				_cellB.RestoreStorageWords(words.Slice(8), saveKey);
				_cellC.RestoreStorageWords(words.Slice(16), saveKey);
				_cellD.RestoreStorageWords(words.Slice(24), saveKey);
			}
		}

		/// <summary>Converts a plain Matrix4x4 value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureMatrix4x4(Matrix4x4 value) =>
			new SecureMatrix4x4(value);

		/// <summary>Converts back to the plain Matrix4x4 value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Matrix4x4(SecureMatrix4x4 value) => value.Decrypted;

		/// <summary>Compares this value with another secured Matrix4x4 for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureMatrix4x4 other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureMatrix4x4 other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Matrix4x4 values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureMatrix4x4 left, SecureMatrix4x4 right) =>
			left.Equals(right);

		/// <summary>Tests two secured Matrix4x4 values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureMatrix4x4 left, SecureMatrix4x4 right) =>
			!left.Equals(right);

		/// <summary>Multiplies two secured Matrix4x4 values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecureMatrix4x4 operator *(SecureMatrix4x4 a, SecureMatrix4x4 b) =>
			new SecureMatrix4x4(a.Decrypted * b.Decrypted);
	}
}
#endif
