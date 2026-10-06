using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Drives IBenchmark implementations placed on the same GameObject.
/// Measures write/read throughput for int, float, and string; cleans up
/// (GC collect + frame gaps) between phases; logs all results.
/// Attach to a GameObject alongside one or more IBenchmark components.
/// </summary>
public class BenchmarkRunner : MonoBehaviour
{
	[Tooltip("Iterations for int/float phases.")]
	[SerializeField]
	private int iterations = 1_000_000;

	[Tooltip("Iterations for string phases (strings allocate; keep lower).")]
	[SerializeField]
	private int stringIterations = 100_000;

	[Tooltip("Warmup operations before each measured phase.")]
	[SerializeField]
	private int warmupIterations = 1_000;

	private void Start()
	{
		StartCoroutine(RunAll());
	}

	private IEnumerator RunAll()
	{
		IBenchmark[] benchmarks = GetComponents<IBenchmark>();
		if (benchmarks.Length == 0)
		{
			Debug.Log("[BenchmarkRunner] No IBenchmark components found on this GameObject.");
			yield break;
		}

		Debug.Log(
			$"[BenchmarkRunner] {benchmarks.Length} implementation(s) | {iterations:N0} iterations ({stringIterations:N0} for strings)."
		);
		yield return Cleanup();

		foreach (IBenchmark benchmark in benchmarks)
		{
			yield return Run(benchmark);
		}

		Debug.Log("[BenchmarkRunner] All benchmarks complete.");
	}

	private IEnumerator Run(IBenchmark benchmark)
	{
		Debug.Log($"[BenchmarkRunner] === {benchmark.DisplayName} ===");
		benchmark.Prepare();
		yield return Cleanup();

		// Warmup (JIT) for every phase, unmeasured.
		Measure("Warmup", benchmark, warmupIterations);
		yield return Cleanup();

		Measure(benchmark.DisplayName, "WriteInt", iterations, i => benchmark.SetInt(i));
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"ReadInt",
			iterations,
			i =>
			{
				if (benchmark.GetInt() == int.MinValue)
					throw new InvalidOperationException();
			}
		);
		yield return Cleanup();
		Measure(benchmark.DisplayName, "WriteFloat", iterations, i => benchmark.SetFloat(i));
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"ReadFloat",
			iterations,
			i =>
			{
				if (benchmark.GetFloat() == float.MinValue)
					throw new InvalidOperationException();
			}
		);
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"WriteString",
			stringIterations,
			i => benchmark.SetString("The quick brown fox jumps over the lazy dog")
		);
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"ReadString",
			stringIterations,
			i =>
			{
				if (benchmark.GetString() == null)
					throw new InvalidOperationException();
			}
		);
		yield return Cleanup();
	}

	private static void Measure(string prefix, IBenchmark benchmark, int count)
	{
		Measure(prefix, "Warmup", count, i => benchmark.SetInt(i));
	}

	private static void Measure(string name, string phase, int count, Action<int> body)
	{
		// JIT warmup, unmeasured.
		for (int i = 0; i < 100 && i < count; i++)
		{
			body(i);
		}

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		long memoryBefore = GC.GetTotalMemory(false);
		System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
		for (int i = 0; i < count; i++)
		{
			body(i);
		}
		sw.Stop();
		long memoryDelta = GC.GetTotalMemory(false) - memoryBefore;

		double nsPerOp = sw.Elapsed.TotalMilliseconds * 1_000_000.0 / count;
		double bytesPerOp = (double)memoryDelta / count;
		Debug.Log(
			$"[{name}] {phase}: {count:N0} ops in {sw.Elapsed.TotalMilliseconds:F2} ms | {nsPerOp:F1} ns/op | ~{bytesPerOp:F1} B/op"
		);
	}

	private static IEnumerator Cleanup()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		yield return null;
		yield return null;
	}
}
