using System;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Curated deep-dive: SecureString span API (Length + CopyTo) against the
	/// allocating string baseline. Each pair runs the same logical workload
	/// both ways so the Mean/Allocated columns compare directly.
	/// Run with: --filter *StringSpanBenchmarks*</summary>
	[MemoryDiagnoser]
	public class StringSpanBenchmarks
	{
		private SecureString _short = "hunter2";
		private SecureString _long = "The quick brown fox jumps over the lazy dog, twice over!";
		private SecureString _short2 = "hunter2";
		private SecureString _long2 = "The quick brown fox jumps over the lazy dog, twice over!";
		private char[] _shortBuffer = Array.Empty<char>();
		private char[] _longBuffer = Array.Empty<char>();
		private char[] _longBuffer2 = Array.Empty<char>();
		private char[] _shortChars = Array.Empty<char>();
		private char[] _longChars = Array.Empty<char>();

		[GlobalSetup]
		public void Setup()
		{
			_shortBuffer = new char[_short.Length];
			_longBuffer = new char[_long.Length];
			_longBuffer2 = new char[_long2.Length];
			_shortChars = "hunter2".ToCharArray();
			_longChars = "The quick brown fox jumps over the lazy dog, twice over!".ToCharArray();
		}

		// ---- Length ----
		[Benchmark]
		public int Span_Length_Short() => _short.Length;

		[Benchmark]
		public int Span_Length_Long() => _long.Length;

		// ---- Read: allocating baseline vs span (same workload: all chars out) ----
		[Benchmark]
		public string Read_ToString_Short() => _short.ToString();

		[Benchmark]
		public char Read_CopyTo_Short()
		{
			_short.CopyTo(_shortBuffer);
			return _shortBuffer[0];
		}

		[Benchmark]
		public char Read_CopyTo_Short_Stackalloc()
		{
			// "hunter2" is 7 chars.
			Span<char> destination = stackalloc char[7];
			_short.CopyTo(destination);
			return destination[0];
		}

		[Benchmark]
		public string Read_ToString_Long() => _long.ToString();

		[Benchmark]
		public char Read_CopyTo_Long()
		{
			_long.CopyTo(_longBuffer);
			return _longBuffer[0];
		}

		// ---- Equality: allocating baseline vs span (same workload: full compare) ----
		[Benchmark]
		public bool Equality_Allocating_Short() => _short == _short2;

		[Benchmark]
		public bool Equality_Span_Short()
		{
			// "hunter2" is 7 chars.
			Span<char> a = stackalloc char[7];
			Span<char> b = stackalloc char[7];
			_short.CopyTo(a);
			_short2.CopyTo(b);
			return a.SequenceEqual(b);
		}

		[Benchmark]
		public bool Equality_Allocating_Long() => _long == _long2;

		[Benchmark]
		public bool Equality_Span_Long()
		{
			_long.CopyTo(_longBuffer);
			_long2.CopyTo(_longBuffer2);
			return _longBuffer.AsSpan().SequenceEqual(_longBuffer2);
		}

		// ---- Write: string baseline vs span (same workload: seal these chars) ----
		[Benchmark]
		public SecureString Write_Ctor_Short() => new SecureString("hunter2");

		[Benchmark]
		public SecureString Write_Ctor_Long() =>
			new SecureString("The quick brown fox jumps over the lazy dog, twice over!");

		[Benchmark]
		public SecureString Write_TryParse_Span_Short()
		{
			SecureString.TryParse(_shortChars, null, out SecureString result);
			return result;
		}

		[Benchmark]
		public SecureString Write_TryParse_Span_Long()
		{
			SecureString.TryParse(_longChars, null, out SecureString result);
			return result;
		}
	}
}
