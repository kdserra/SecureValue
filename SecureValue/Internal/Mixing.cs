#nullable enable
using System.Runtime.CompilerServices;

namespace SecureValue
{
	/// <summary>Bit-mixing primitives shared by the RNG, cipher, and MAC layers.</summary>
	internal static class Mixing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong SplitMix(ulong state)
		{
			state += 0x9E3779B97F4A7C15UL;
			ulong z = state;
			z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
			z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
			return z ^ (z >> 31);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong RotateLeft(ulong x, int r) => (x << r) | (x >> (64 - r));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong RotateRight(ulong x, int r) => (x >> r) | (x << (64 - r));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint RotateLeft(uint x, int r) => (x << r) | (x >> (32 - r));
	}
}
