using System;
using System.Collections;
using System.Text;
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
		Debug.Log("[BenchmarkRunner] Starting benchmarks...");

		StringBuilder results = new StringBuilder();
		IBenchmark[] benchmarks = GetComponents<IBenchmark>();
		if (benchmarks.Length == 0)
		{
			results.Append("[BenchmarkRunner] No IBenchmark components found on this GameObject.");
			Debug.Log(results.ToString());
			yield break;
		}

		yield return Cleanup();

		foreach (IBenchmark benchmark in benchmarks)
		{
			if (results.Length > 0)
				results.AppendLine();

			yield return Run(benchmark, results);
		}

		results.AppendLine();
		results.AppendLine("[BenchmarkRunner] All benchmarks complete.");
		Debug.Log(results.ToString());
	}

	private IEnumerator Run(IBenchmark benchmark, StringBuilder results)
	{
		results.AppendLine($"[BenchmarkRunner] === {benchmark.DisplayName} ===");
		benchmark.Prepare();
		yield return Cleanup();

		// Warmup (JIT) for every phase, unmeasured.
		Measure("Warmup", benchmark, warmupIterations, results);
		yield return Cleanup();

		Measure(benchmark.DisplayName, "WriteInt", iterations, i => benchmark.SetInt(i), results);
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"ReadInt",
			iterations,
			i =>
			{
				if (benchmark.GetInt() == int.MinValue)
					throw new InvalidOperationException();
			},
			results
		);
		yield return Cleanup();
		Measure(benchmark.DisplayName, "WriteFloat", iterations, i => benchmark.SetFloat(i), results);
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"ReadFloat",
			iterations,
			i =>
			{
				if (benchmark.GetFloat() == float.MinValue)
					throw new InvalidOperationException();
			},
			results
		);
		yield return Cleanup();
		Measure(
			benchmark.DisplayName,
			"WriteString",
			stringIterations,
			i => benchmark.SetString("The quick brown fox jumps over the lazy dog"),
			results
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
			},
			results
		);
		yield return Cleanup();
	}

	private static void Measure(string prefix, IBenchmark benchmark, int count, StringBuilder results)
	{
		Measure(prefix, "Warmup", count, i => benchmark.SetInt(i), results);
	}

	private static void Measure(
		string name,
		string phase,
		int count,
		Action<int> body,
		StringBuilder results
	)
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
		results.AppendLine(
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
