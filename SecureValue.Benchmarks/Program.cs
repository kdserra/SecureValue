using BenchmarkDotNet.Running;

namespace SecureValue.Benchmarks
{
	internal static class Program
	{
		// Pick a suite with --filter, e.g.:
		//   dotnet run -c Release -- --filter *IntFloatStringBenchmarks*  curated trio: int+float+string
		//   dotnet run -c Release -- --filter *BaselineBenchmarks*        plain (non-secured) baselines
		//   dotnet run -c Release -- --filter *IntBenchmarks*             curated: int only
		//   dotnet run -c Release -- --filter *FloatBenchmarks*           curated: float only
		//   dotnet run -c Release -- --filter *StringBenchmarks*          curated: string only
		//   dotnet run -c Release -- --filter *StringSpanBenchmarks*       curated: string span API vs baseline
		//   dotnet run -c Release -- --filter *AllWrappersBenchmarks*     every supported wrapper type
		//   dotnet run -c Release -- --filter *MacBenchmarks*              MAC tag layer in isolation
		//   dotnet run -c Release -- --filter *ParameterPassingBenchmarks* by-value vs in vs ref vs ref readonly
		//   dotnet run -c Release -- --filter *Benchmarks*                everything
		// Append --job short for a quick run.
		private static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
