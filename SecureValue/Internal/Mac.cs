#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>
	/// Keyed MAC tag over ciphertext words, stored inline and checked on every
	/// read. Subkeys arrive derived; the tag derives nothing itself.
	/// </summary>
	internal static class Mac
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong TagMix(ulong cipher, ulong key)
		{
			ulong z = cipher ^ key;
			z = (z ^ (z >> 33)) * 0xFF51AFD7ED558CCDUL;
			z = (z ^ (z >> 33)) * 0xC4CEB9FE1A85EC53UL;
			return z ^ (z >> 33);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ulong cipher, ulong k3) => (uint)(TagMix(cipher, k3) >> 24);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ulong cipher, in KeySet rk) => ComputeTag(cipher, rk.K3);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ulong c0, ulong c1, ulong k3) =>
			(uint)(TagMix(TagMix(c0, k3) ^ c1, k3) >> 24);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ulong c0, ulong c1, in KeySet rk) =>
			ComputeTag(c0, c1, rk.K3);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ReadOnlySpan<ulong> ciphers, in KeySet rk)
		{
			ulong acc = rk.K0;
			for (int i = 0; i < ciphers.Length; i++)
			{
				acc = TagMix(acc ^ ciphers[i] ^ ((ulong)i * 0x9E3779B97F4A7C15UL), rk.K3);
			}
			return (uint)(TagMix(acc, rk.K3) >> 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ComputeTag(ulong prefix, ReadOnlySpan<ulong> ciphers, in KeySet rk)
		{
			ulong acc = TagMix(rk.K0 ^ prefix, rk.K3);
			for (int i = 0; i < ciphers.Length; i++)
			{
				acc = TagMix(acc ^ ciphers[i] ^ ((ulong)i * 0x9E3779B97F4A7C15UL), rk.K3);
			}
			return (uint)(TagMix(acc, rk.K3) >> 24);
		}
	}
}
