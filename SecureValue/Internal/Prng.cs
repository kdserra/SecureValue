#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureValue
{
	/// <summary>
	/// Thread-local xoshiro256** PRNG seeded from the process key for salts
	/// (not crypto-grade; salts need per-run unpredictability only).
	/// Thread-locality keeps the hot path lock-free.
	/// </summary>
	internal static class Prng
	{
		private struct State
		{
			private ulong _s0,
				_s1,
				_s2,
				_s3;

			/// <summary>Initializes the generator state from four seed words.</summary>
			public State(ulong s0, ulong s1, ulong s2, ulong s3)
			{
				_s0 = s0;
				_s1 = s1;
				_s2 = s2;
				_s3 = s3;
			}

			/// <summary>Advances the generator and returns the next pseudorandom word.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ulong Next()
			{
				ulong result = Mixing.RotateLeft(_s1 * 5, 7) * 9;
				ulong t = _s1 << 17;
				_s2 ^= _s0;
				_s3 ^= _s1;
				_s1 ^= _s2;
				_s0 ^= _s3;
				_s2 ^= t;
				_s3 = Mixing.RotateLeft(_s3, 45);
				return result;
			}
		}

		[ThreadStatic]
		private static State t_state;

		[ThreadStatic]
		private static bool t_ready;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong Next()
		{
			if (!t_ready)
			{
				t_state = Create();
				t_ready = true;
			}
			// Mutable struct in a static field: advances in place, no copies.
			return t_state.Next();
		}

		private static State Create()
		{
			ulong z = Keys.K0 ^ Mixing.RotateLeft(Keys.K1, 32);
			z += 0x9E3779B97F4A7C15UL;
			ulong s0 = Mixing.SplitMix(z);
			z += 0x9E3779B97F4A7C15UL;
			ulong s1 = Mixing.SplitMix(z);
			z += 0x9E3779B97F4A7C15UL;
			ulong s2 = Mixing.SplitMix(z);
			z += 0x9E3779B97F4A7C15UL;
			ulong s3 = Mixing.SplitMix(z);
			return new State(s0, s1, s2, s3);
		}
	}
}
