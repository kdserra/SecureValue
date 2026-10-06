using System;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Curated trio in ONE suite: SecureInt + SecureFloat + SecureString.
	/// Run with: --filter *IntFloatStringBenchmarks*
	/// </summary>
	[MemoryDiagnoser]
	public class IntFloatStringBenchmarks
	{
		private SecureInt _int = 42;
		private SecureFloat _float = 3.14159f;
		private SecureString _stringShort = "hunter2";
		private SecureString _stringLong =
			"The quick brown fox jumps over the lazy dog, twice over!";
		private char[] _stringShortBuffer = Array.Empty<char>();
		private char[] _stringLongBuffer = Array.Empty<char>();
		private char[] _stringShortChars = Array.Empty<char>();
		private char[] _stringLongChars = Array.Empty<char>();

		[GlobalSetup]
		public void Setup()
		{
			_stringShortBuffer = new char[_stringShort.Length];
			_stringLongBuffer = new char[_stringLong.Length];
			_stringShortChars = "hunter2".ToCharArray();
			_stringLongChars =
				"The quick brown fox jumps over the lazy dog, twice over!".ToCharArray();
		}

		// ---- SecureInt ----
		[Benchmark]
		public SecureInt Int_Write() => new SecureInt(123456);

		[Benchmark]
		public int Int_Read() => _int;

		[Benchmark]
		public int Int_Arithmetic() => _int * 2 + 1;

		[Benchmark]
		public bool Int_Compare() => _int > 10;

		// ---- SecureFloat ----
		[Benchmark]
		public SecureFloat Float_Write() => new SecureFloat(2.71828f);

		[Benchmark]
		public float Float_Read() => _float;

		[Benchmark]
		public float Float_Arithmetic() => _float * 2f + 0.5f;

		[Benchmark]
		public bool Float_Compare() => _float >= 3f;

		// ---- SecureString ----
		[Benchmark]
		public SecureString String_Write_Short() => new SecureString("hunter2");

		[Benchmark]
		public SecureString String_Write_Long() =>
			new SecureString("The quick brown fox jumps over the lazy dog, twice over!");

		[Benchmark]
		public string String_Read_Short() => _stringShort.ToString();

		[Benchmark]
		public string String_Read_Long() => _stringLong.ToString();

		[Benchmark]
		public bool String_Equality_Short() => _stringShort == new SecureString("hunter2");

		// ---- SecureString span (same read workload, zero alloc) ----
		[Benchmark]
		public char String_Read_Short_Span()
		{
			_stringShort.CopyTo(_stringShortBuffer);
			return _stringShortBuffer[0];
		}

		[Benchmark]
		public char String_Read_Long_Span()
		{
			_stringLong.CopyTo(_stringLongBuffer);
			return _stringLongBuffer[0];
		}

		// ---- SecureString span writes (same workload: seal these chars) ----
		[Benchmark]
		public SecureString String_Write_Span_Short()
		{
			SecureString.TryParse(_stringShortChars, null, out SecureString result);
			return result;
		}

		[Benchmark]
		public SecureString String_Write_Span_Long()
		{
			SecureString.TryParse(_stringLongChars, null, out SecureString result);
			return result;
		}
	}
}
