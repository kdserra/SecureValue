using System;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Cost of the MAC tag layer in isolation (single-word, pair, and span
	/// tags, plus the true read path of tag-key derivation + tag). Method
	/// names are single tokens so the GitHub report needs no quoting.
	/// Run with: --filter *MacBenchmarks*
	/// </summary>
	[MemoryDiagnoser]
	public class MacBenchmarks
	{
		private ulong _cipher;
		private ulong _cipherB;
		private ulong[] _span4 = Array.Empty<ulong>();
		private ulong[] _span16 = Array.Empty<ulong>();
		private ulong _salt;
		private ulong _tagKey;
		private KeySet _keys;

		[GlobalSetup]
		public void Setup()
		{
			var rng = new Random(42);
			_cipher = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
			_cipherB = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
			_span4 = new ulong[4];
			_span16 = new ulong[16];
			for (int i = 0; i < _span4.Length; i++)
			{
				_span4[i] = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
			}
			for (int i = 0; i < _span16.Length; i++)
			{
				_span16[i] = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
			}
			_salt = ((ulong)(uint)rng.Next() << 32) | (uint)rng.Next();
			_keys = Keys.Derive(_salt, Keys.ProcessSet);
			_tagKey = Vault.DeriveProcessTagKey(_salt);
		}

		[Benchmark]
		public uint TagSingle() => Mac.ComputeTag(_cipher, _tagKey);

		[Benchmark]
		public uint TagPair() => Mac.ComputeTag(_cipher, _cipherB, _tagKey);

		[Benchmark]
		public uint TagSpan4() => Mac.ComputeTag(_span4, _keys);

		[Benchmark]
		public uint TagSpan16() => Mac.ComputeTag(_span16, _keys);

		[Benchmark]
		public uint TagReadPath() => Mac.ComputeTag(_cipher, Vault.DeriveProcessTagKey(_salt));
	}
}
