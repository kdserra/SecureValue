#nullable enable
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>
	/// 64-bit block cipher: 4-round key-dependent Feistel network plus an
	/// avalanche finalizer built only from trivially invertible steps.
	/// Allocation-free, value primitives only; subkeys derived once per call.
	/// </summary>
	internal static class BlockCipher
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint Round(uint x, ulong key)
		{
			uint c = (uint)key | 1u;
			return Mixing.RotateLeft((x ^ (uint)key) * c, 11) ^ (uint)(key >> 41);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FeistelEncrypt(ulong v, ulong k0, ulong k1, ulong k2, ulong k3)
		{
			v ^= k2;
			uint l = (uint)(v >> 32);
			uint r = (uint)v;
			uint t1 = l ^ Round(r, k0);
			uint t2 = r ^ Round(t1, k1);
			uint t3 = t1 ^ Round(t2, k2);
			uint t4 = t2 ^ Round(t3, k3);
			return (((ulong)t3 << 32) | t4) ^ k2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FeistelDecrypt(ulong v, ulong k0, ulong k1, ulong k2, ulong k3)
		{
			v ^= k2;
			uint l = (uint)(v >> 32);
			uint r = (uint)v;
			uint t2 = r ^ Round(l, k3);
			uint t1 = l ^ Round(t2, k2);
			uint rr = t2 ^ Round(t1, k1);
			uint ll = t1 ^ Round(rr, k0);
			return (((ulong)ll << 32) | rr) ^ k2;
		}

		// Avalanche finalizer using only trivially invertible steps, so decrypt
		// mirrors it with subtract/rotate-back (no modular inverses).
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FinalMix(ulong z, ulong sk0, ulong sk1, ulong sk2, ulong sk3)
		{
			z ^= sk2;
			z += sk0;
			z = Mixing.RotateLeft(z, 29);
			z ^= z >> 27;
			z ^= sk1;
			z += sk3;
			z = Mixing.RotateLeft(z, 17);
			z ^= z >> 31;
			return z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FinalUnmix(ulong z, ulong sk0, ulong sk1, ulong sk2, ulong sk3)
		{
			z ^= z >> 31 ^ z >> 62;
			z = Mixing.RotateRight(z, 17);
			z -= sk3;
			z ^= sk1;
			z ^= z >> 27 ^ z >> 54;
			z = Mixing.RotateRight(z, 29);
			z -= sk0;
			z ^= sk2;
			return z;
		}

		/// <summary>Encrypts one word with already-derived subkeys (no derivation).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong EncryptCore(ulong plain, in KeySet rk) =>
			FinalMix(FeistelEncrypt(plain, rk.K0, rk.K1, rk.K2, rk.K3), rk.K0, rk.K1, rk.K2, rk.K3);

		/// <summary>Decrypts one word with already-derived subkeys (no derivation, no tag check).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong DecryptCore(ulong cipher, in KeySet rk) =>
			FeistelDecrypt(
				FinalUnmix(cipher, rk.K0, rk.K1, rk.K2, rk.K3),
				rk.K0,
				rk.K1,
				rk.K2,
				rk.K3
			);

		/// <summary>Seals one word: single derivation, encrypt plus MAC tag.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (ulong Cipher, uint Tag) Seal(ulong plain, ulong salt, in KeySet ks)
		{
			KeySet rk = Keys.Derive(salt, ks);
			ulong cipher = EncryptCore(plain, rk);
			return (cipher, Mac.ComputeTag(cipher, rk));
		}

		/// <summary>
		/// Opens one word: verifies the MAC tag, then decrypts. False on mismatch.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool TryOpen(
			ulong cipher,
			uint tag,
			ulong salt,
			in KeySet ks,
			out ulong plain
		)
		{
			KeySet rk = Keys.Derive(salt, ks);
			if (Mac.ComputeTag(cipher, rk) != tag)
			{
				plain = 0UL;
				return false;
			}
			plain = DecryptCore(cipher, rk);
			return true;
		}

		/// <summary>
		/// 128-bit seal with cross-word chaining (hi keying derives from the lo
		/// ciphertext); one tag covers both halves.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (ulong Lo, ulong Hi, uint Tag) Seal128(
			ulong lo,
			ulong hi,
			ulong salt,
			in KeySet ks
		)
		{
			KeySet rk = Keys.Derive(salt, ks);
			ulong cLo = EncryptCore(lo, rk);
			KeySet rkHi = Keys.Derive(salt ^ Mixing.SplitMix(cLo ^ ks.K1), ks);
			ulong cHi = EncryptCore(hi, rkHi);
			return (cLo, cHi, Mac.ComputeTag(cLo, cHi, rk));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool TryOpen128(
			ulong cLo,
			ulong cHi,
			uint tag,
			ulong salt,
			in KeySet ks,
			out ulong lo,
			out ulong hi
		)
		{
			KeySet rk = Keys.Derive(salt, ks);
			if (Mac.ComputeTag(cLo, cHi, rk) != tag)
			{
				lo = 0UL;
				hi = 0UL;
				return false;
			}
			lo = DecryptCore(cLo, rk);
			KeySet rkHi = Keys.Derive(salt ^ Mixing.SplitMix(cLo ^ ks.K1), ks);
			hi = DecryptCore(cHi, rkHi);
			return true;
		}
	}
}
