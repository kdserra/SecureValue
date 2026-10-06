using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>Curated deep-dive: SecureString (the only allocating wrapper).
	/// Run with: --filter *StringBenchmarks*</summary>
	[MemoryDiagnoser]
	public class StringBenchmarks
	{
		private SecureString _short = "hunter2";
		private SecureString _long = "The quick brown fox jumps over the lazy dog, twice over!";

		[Benchmark]
		public SecureString Write_Short() => new SecureString("hunter2");

		[Benchmark]
		public SecureString Write_Long() =>
			new SecureString("The quick brown fox jumps over the lazy dog, twice over!");

		[Benchmark]
		public string Read_Short() => _short.ToString();

		[Benchmark]
		public string Read_Long() => _long.ToString();

		[Benchmark]
		public bool Equality_Short() => _short == new SecureString("hunter2");
	}
}
