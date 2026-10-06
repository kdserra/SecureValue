using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>Curated deep-dive: SecureInt. Run with: --filter *IntBenchmarks*</summary>
	[MemoryDiagnoser]
	public class IntBenchmarks
	{
		private SecureInt _value = 42;

		[Benchmark]
		public SecureInt Write() => new SecureInt(123456);

		[Benchmark]
		public int Read() => _value;

		[Benchmark]
		public int Arithmetic() => _value * 2 + 1;

		[Benchmark]
		public bool Compare() => _value > 10;
	}
}
