#nullable enable
#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using SecureValue;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SecureValue.Unity
{
	/// <summary>Memory-encrypted <see cref="Ray"/> (UnityEngine).</summary>
	[Serializable]
	public partial struct SecureRay
		: ISecureSerialization,
			UnityEngine.ISerializationCallbackReceiver
	{
		private Cell128 _cellA;
		private Cell _cellB;

		/// <summary>Secures a Ray value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SecureRay(Ray value)
		{
			_serialized = default;
			_cellA = default;
			_cellB = default;
			Vector3 o = value.origin;
			Vector3 d = value.direction;
			_cellA.Protect(W(o.x, o.y), W(o.z, d.x));
			_cellB.Protect(W(d.y, d.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong W(float x, float y) =>
			(ulong)(uint)BitConverter.SingleToInt32Bits(x)
			| ((ulong)(uint)BitConverter.SingleToInt32Bits(y) << 32);

		/// <summary>Gets the decrypted plain value (default when never assigned).</summary>
		public Ray Decrypted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(ulong lo, ulong hi) = _cellA.Unprotect();
				ulong rest = _cellB.Unprotect();
				// Unity's Ray constructor AND direction setter both normalize, so neither
				// can restore the stored bits: a second normalization drifts by ULPs and
				// destroys bit-exactness (caught by the thoroughness suites). Write the six
				// floats directly instead: Ray is a sequential 6-float struct, origin
				// followed by direction (verified against UnityCsReference).
				Ray result = default;
				Span<float> floats = MemoryMarshal.Cast<Ray, float>(
					MemoryMarshal.CreateSpan(ref result, 1)
				);
				floats[0] = F(lo);
				floats[1] = F(lo >> 32);
				floats[2] = F(hi);
				floats[3] = F(hi >> 32);
				floats[4] = F(rest);
				floats[5] = F(rest >> 32);
				return result;
			}
		}

		/// <summary>Tries to decrypt without throwing. Returns false when never assigned or tampered; tampering still raises <see cref="TamperingNotifier.TamperingDetected"/>.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDecrypt(out Ray value)
		{
			bool okA = _cellA.TryUnprotect(out ulong lo, out ulong hi);
			bool okB = _cellB.TryUnprotect(out ulong rest);
			if (okA && okB)
			{
				// Unity's Ray constructor AND direction setter both normalize, so neither
				// can restore the stored bits: a second normalization drifts by ULPs and
				// destroys bit-exactness (caught by the thoroughness suites). Write the six
				// floats directly instead: Ray is a sequential 6-float struct, origin
				// followed by direction (verified against UnityCsReference).
				Ray result = default;
				Span<float> floats = MemoryMarshal.Cast<Ray, float>(
					MemoryMarshal.CreateSpan(ref result, 1)
				);
				floats[0] = F(lo);
				floats[1] = F(lo >> 32);
				floats[2] = F(hi);
				floats[3] = F(hi >> 32);
				floats[4] = F(rest);
				floats[5] = F(rest >> 32);
				value = result;
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
			if (packed == null || packed.Length == 0)
			{
				// Never serialized: stay unset (reads return default).
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

		/// <summary>Converts a plain Ray value into its secured form.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SecureRay(Ray value) => new SecureRay(value);

		/// <summary>Converts back to the plain Ray value (decrypts on read).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Ray(SecureRay value) => value.Decrypted;

		/// <summary>Compares this value with another secured Ray for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SecureRay other) => Decrypted.Equals(other.Decrypted);

		/// <summary>Compares this value with another object for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) => obj is SecureRay other && Equals(other);

		/// <summary>Returns the hash code of the decrypted value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Decrypted.GetHashCode();

		/// <summary>Returns the decrypted value as a string.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Decrypted.ToString();

		/// <summary>Tests two secured Ray values for equality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SecureRay left, SecureRay right) => left.Equals(right);

		/// <summary>Tests two secured Ray values for inequality.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SecureRay left, SecureRay right) => !left.Equals(right);
	}
}
#endif
