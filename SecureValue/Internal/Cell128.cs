#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>
	/// A pair of secured words covering a 128-bit payload (decimal, Guid, ...),
	/// kept as TWO independent copies so a tampered copy restores from the good
	/// one instead of throwing.
	/// </summary>
	internal struct Cell128
	{
		private ulong _salt;
		private ulong _cipherLo;
		private ulong _cipherHi;
		private uint _tag;
		private ulong _saltB;
		private ulong _cipherLoB;
		private ulong _cipherHiB;
		private uint _tagB;

		/// <summary>
		/// Encrypts a 128-bit plaintext pair into both copies with independent fresh salts.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Protect(ulong lo, ulong hi)
		{
			// One shared salt links each pair; the tag covers both halves.
			// The two copies get independent salts (see Cell: shared salt is a
			// single point of failure).
			_salt = Vault.RandomSalt();
			(_cipherLo, _cipherHi, _tag) = Vault.Seal128(lo, hi, _salt);
			_saltB = Vault.RandomSalt();
			(_cipherLoB, _cipherHiB, _tagB) = Vault.Seal128(lo, hi, _saltB);
		}

		/// <summary>
		/// Verifies both copies' pair tags and decrypts. A singly-tampered value restores
		/// from the good copy (raising the tamper event) and re-seals the damaged
		/// copy from the recovered plaintext; a doubly-tampered value throws
		/// <see cref="TamperedException"/>, a never-assigned one
		/// <see cref="UninitializedException"/>. The fast path never mutates.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (ulong Lo, ulong Hi) Unprotect()
		{
			bool okA = PairTagMatches(_salt, _cipherLo, _cipherHi, _tag);
			bool okB = PairTagMatches(_saltB, _cipherLoB, _cipherHiB, _tagB);
			if (okA && okB)
			{
				// Fast path: both tags verified, only the primary copy decrypted.
				return DecryptPair(_salt, _cipherLo, _cipherHi);
			}
			if (!okA && !okB && IsUnset)
			{
				Vault.ThrowUninitialized();
			}
			if (okA)
			{
				(ulong lo, ulong hi) = DecryptPair(_salt, _cipherLo, _cipherHi);
				ResealBackup(lo, hi, backup: true);
				TamperingNotifier.Raise();
				return (lo, hi);
			}
			if (okB)
			{
				(ulong lo, ulong hi) = DecryptPair(_saltB, _cipherLoB, _cipherHiB);
				ResealBackup(lo, hi, backup: false);
				TamperingNotifier.Raise();
				return (lo, hi);
			}
			Vault.ThrowTampered();
			return (0UL, 0UL);
		}

		/// <summary>
		/// Tries to verify and decrypt without throwing. True with the good copy's
		/// value when at least one copy verifies (a singly-damaged copy is re-sealed
		/// from the recovered plaintext, exactly like <see cref="Unprotect"/>).
		/// Tampering still raises
		/// <see cref="TamperingNotifier.TamperingDetected"/>, once per damaged read.
		/// False with zeroed values when both copies fail or were never assigned.
		/// Reassignment heals regardless.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryUnprotect(out ulong lo, out ulong hi)
		{
			bool okA = PairTagMatches(_salt, _cipherLo, _cipherHi, _tag);
			bool okB = PairTagMatches(_saltB, _cipherLoB, _cipherHiB, _tagB);
			if (okA && okB)
			{
				(lo, hi) = DecryptPair(_salt, _cipherLo, _cipherHi);
				return true;
			}
			if (!okA && !okB && IsUnset)
			{
				lo = 0UL;
				hi = 0UL;
				return false;
			}
			if (okA || okB)
			{
				(lo, hi) = okA
					? DecryptPair(_salt, _cipherLo, _cipherHi)
					: DecryptPair(_saltB, _cipherLoB, _cipherHiB);
				ResealBackup(lo, hi, backup: okA);
				TamperingNotifier.Raise();
				return true;
			}
			TamperingNotifier.Raise();
			lo = 0UL;
			hi = 0UL;
			return false;
		}

		/// <summary>
		/// Re-seals one copy from recovered plaintext with a fresh salt, healing a
		/// singly-tampered read. The surviving copy is untouched.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResealBackup(ulong lo, ulong hi, bool backup)
		{
			ulong salt = Vault.RandomSalt();
			(ulong cLo, ulong cHi, uint tag) = Vault.Seal128(lo, hi, salt);
			if (backup)
			{
				_saltB = salt;
				_cipherLoB = cLo;
				_cipherHiB = cHi;
				_tagB = tag;
			}
			else
			{
				_salt = salt;
				_cipherLo = cLo;
				_cipherHi = cHi;
				_tag = tag;
			}
		}

		/// <summary>
		/// Verifies one pair's tag without decrypting. All-zero words read as absent,
		/// never valid.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PairTagMatches(ulong salt, ulong cipherLo, ulong cipherHi, uint tag)
		{
			if (salt == 0UL && cipherLo == 0UL && cipherHi == 0UL && tag == 0U)
			{
				return false;
			}
			return Mac.ComputeTag(cipherLo, cipherHi, Vault.DeriveProcessTagKey(salt)) == tag;
		}

		/// <summary>Decrypts one verified pair (chained hi-word keying mirrors the seal order).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static (ulong Lo, ulong Hi) DecryptPair(ulong salt, ulong cipherLo, ulong cipherHi)
		{
			KeySet rk = Vault.DeriveProcessKeys(salt);
			ulong lo = BlockCipher.DecryptCore(cipherLo, rk);
			KeySet rkHi = Vault.DeriveChainedKeys(salt, cipherLo, Keys.ProcessSet);
			return (lo, BlockCipher.DecryptCore(cipherHi, rkHi));
		}

		/// <summary>Internal test hook: exposes the primary lo cipher word.</summary>
		internal ulong PeekCipherLo() => _cipherLo;

		internal const int WordCount = 8;

		/// <summary>True when never assigned.</summary>
		internal bool IsUnset
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get =>
				_salt == 0UL
				&& _cipherLo == 0UL
				&& _cipherHi == 0UL
				&& _tag == 0U
				&& _saltB == 0UL
				&& _cipherLoB == 0UL
				&& _cipherHiB == 0UL
				&& _tagB == 0U;
		}

		/// <summary>Exports the raw cell words (salt, cipherLo, cipherHi, tag, backup quad).</summary>
		internal void CopyWords(Span<ulong> dst)
		{
			dst[0] = _salt;
			dst[1] = _cipherLo;
			dst[2] = _cipherHi;
			dst[3] = _tag;
			dst[4] = _saltB;
			dst[5] = _cipherLoB;
			dst[6] = _cipherHiB;
			dst[7] = _tagB;
		}

		/// <summary>Restores raw cell words previously produced by <see cref="CopyWords"/>.</summary>
		internal void RestoreWords(ReadOnlySpan<ulong> src)
		{
			_salt = src[0];
			_cipherLo = src[1];
			_cipherHi = src[2];
			_tag = (uint)src[3];
			_saltB = src[4];
			_cipherLoB = src[5];
			_cipherHiB = src[6];
			_tagB = (uint)src[7];
		}

		/// <summary>
		/// Re-encrypts both copies with a caller-supplied per-save storage key, so saves
		/// load in any process. Layout per copy: [salt, storageCipherLo, storageCipherHi,
		/// storageTag]; hi chains on the lo ciphertext, mirroring the in-memory seal
		/// order. The caller appends the key words after the data words.
		/// </summary>
		internal void CopyStorageWords(Span<ulong> dst, in KeySet ks)
		{
			CopyOneStorageCopy(_salt, _cipherLo, _cipherHi, dst, 0, ks);
			CopyOneStorageCopy(_saltB, _cipherLoB, _cipherHiB, dst, 4, ks);
		}

		private static void CopyOneStorageCopy(
			ulong salt,
			ulong cipherLo,
			ulong cipherHi,
			Span<ulong> dst,
			int offset,
			in KeySet ks
		)
		{
			KeySet rkP = Vault.DeriveProcessKeys(salt);
			KeySet rkPHi = Vault.DeriveChainedKeys(salt, cipherLo, Keys.ProcessSet);
			(ulong lo, ulong hi) = (Vault.OpenWord(cipherLo, rkP), Vault.OpenWord(cipherHi, rkPHi));
			KeySet rkS = Keys.Derive(salt, ks);
			ulong cLo = Vault.SealWord(lo, rkS);
			KeySet rkSHi = Vault.DeriveChainedKeys(salt, cLo, ks);
			ulong cHi = Vault.SealWord(hi, rkSHi);
			dst[offset] = salt;
			dst[offset + 1] = cLo;
			dst[offset + 2] = cHi;
			// The tag covers both ciphers AND the key words (short Mac inputs mix
			// rk.K3 only, so a cipher-only tag would leave ks.K0..K2 flips silent).
			Span<ulong> auth = stackalloc ulong[2 + KeySet.WordCount];
			auth[0] = cLo;
			auth[1] = cHi;
			ks.CopyTo(auth.Slice(2));
			dst[offset + 3] = Vault.ComputeTag(auth, rkS);
		}

		/// <summary>
		/// Restores storage-form words with the key they were saved with.
		/// A singly-tampered save recovers from the good copy (raising the tamper
		/// event); a doubly-tampered save throws.
		/// </summary>
		internal void RestoreStorageWords(ReadOnlySpan<ulong> src, in KeySet ks)
		{
			bool okA = TryStorageCopy(src, 0, ks, out ulong aLo, out ulong aHi);
			bool okB = TryStorageCopy(src, 4, ks, out ulong bLo, out ulong bHi);
			if (okA && okB)
			{
				SealBoth(aLo, aHi, src[0], src[4]);
				return;
			}
			if (okA)
			{
				SealBoth(aLo, aHi, src[0], src[4]);
				TamperingNotifier.Raise();
				return;
			}
			if (okB)
			{
				SealBoth(bLo, bHi, src[0], src[4]);
				TamperingNotifier.Raise();
				return;
			}
			Vault.ThrowTampered();
		}

		private static bool TryStorageCopy(
			ReadOnlySpan<ulong> src,
			int offset,
			in KeySet ks,
			out ulong lo,
			out ulong hi
		)
		{
			ulong salt = src[offset];
			ulong cLo = src[offset + 1];
			ulong cHi = src[offset + 2];
			uint tag = (uint)src[offset + 3];
			if (salt == 0UL && cLo == 0UL && cHi == 0UL && tag == 0U)
			{
				lo = 0UL;
				hi = 0UL;
				return false;
			}
			KeySet rkS = Keys.Derive(salt, ks);
			Span<ulong> auth = stackalloc ulong[2 + KeySet.WordCount];
			auth[0] = cLo;
			auth[1] = cHi;
			ks.CopyTo(auth.Slice(2));
			if (Vault.ComputeTag(auth, rkS) != tag)
			{
				lo = 0UL;
				hi = 0UL;
				return false;
			}
			KeySet rkSHi = Vault.DeriveChainedKeys(salt, cLo, ks);
			lo = Vault.OpenWord(cLo, rkS);
			hi = Vault.OpenWord(cHi, rkSHi);
			return true;
		}

		/// <summary>Seals both live copies from recovered plaintext, keeping each copy's salt.</summary>
		private void SealBoth(ulong lo, ulong hi, ulong saltA, ulong saltB)
		{
			_salt = saltA;
			(_cipherLo, _cipherHi, _tag) = Vault.Seal128(lo, hi, _salt);
			_saltB = saltB;
			(_cipherLoB, _cipherHiB, _tagB) = Vault.Seal128(lo, hi, _saltB);
		}
	}
}
