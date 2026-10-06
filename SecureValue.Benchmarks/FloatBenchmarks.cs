using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>Curated deep-dive: SecureFloat. Run with: --filter *FloatBenchmarks*</summary>
	[MemoryDiagnoser]
	public class FloatBenchmarks
	{
		private SecureFloat _value = 3.14159f;

		[Benchmark]
		public SecureFloat Write() => new SecureFloat(2.71828f);

		[Benchmark]
		public float Read() => _value;

		[Benchmark]
		public float Arithmetic() => _value * 2f + 0.5f;

		[Benchmark]
		public bool Compare() => _value >= 3f;
	}
}
