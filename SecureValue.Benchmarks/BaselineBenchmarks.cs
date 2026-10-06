using System;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Baselines: the SAME operations performed on plain (non-secured) types,
	/// so the secured suites' overhead can be compared directly.
	/// Run with: --filter *BaselineBenchmarks*
	/// Results are kept in fields and returned so the JIT cannot eliminate them.
	/// </summary>
	[MemoryDiagnoser]
	public class BaselineBenchmarks
	{
		private int _int = 42;
		private float _float = 3.14159f;
		private string _stringShort = "hunter2";
		private string _stringLong = "The quick brown fox jumps over the lazy dog, twice over!";

		[Benchmark]
		public int Int_Write()
		{
			int v = 123456;
			return v;
		}

		[Benchmark]
		public int Int_Read() => _int;

		[Benchmark]
		public int Int_Arithmetic() => _int * 2 + 1;

		[Benchmark]
		public bool Int_Compare() => _int > 10;

		[Benchmark]
		public float Float_Write()
		{
			float v = 2.71828f;
			return v;
		}

		[Benchmark]
		public float Float_Read() => _float;

		[Benchmark]
		public float Float_Arithmetic() => _float * 2f + 0.5f;

		[Benchmark]
		public bool Float_Compare() => _float >= 3f;

		[Benchmark]
		public string String_Write_Short()
		{
			string v = "hunter2";
			return v;
		}

		[Benchmark]
		public string String_Write_Long()
		{
			string v = "The quick brown fox jumps over the lazy dog, twice over!";
			return v;
		}

		[Benchmark]
		public string String_Read_Short() => _stringShort;

		[Benchmark]
		public string String_Read_Long() => _stringLong;

		[Benchmark]
		public bool String_Equality_Short() => _stringShort == "hunter2";
	}
}
