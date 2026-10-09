using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Cost of passing wrappers by value vs in vs ref vs ref readonly, across
	/// primitive, reference-backed, and large value-type wrappers (SecureInt,
	/// SecureGuid, SecureString, SecureBigInteger, and Numerics SecureMatrix4x4).
	/// Each benchmark loops over a NoInlining leaf with one passing
	/// mode, so the measured delta is the copy/dispatch cost, not the decrypt
	/// (identical across modes). Note: wrappers are mutable structs with
	/// non-readonly accessors, so in/ref-readonly call sites pay a defensive
	/// copy on every Decrypted access — these numbers test exactly that.
	/// Run with: --filter *ParameterPassingBenchmarks*
	/// </summary>
	[MemoryDiagnoser]
	public class ParameterPassingBenchmarks
	{
		private const int Iterations = 200;

		private SecureInt _int = 123456;
		private SecureGuid _guid = new Guid("0123456789abcdef0123456789abcdef");
		private SecureValue.Numerics.SecureMatrix4x4 _matrix =
			new SecureValue.Numerics.SecureMatrix4x4(
				new Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16)
			);
		private SecureString _string = "parameter passing benchmark secret";
		private SecureValue.Numerics.SecureBigInteger _bigInteger =
			new SecureValue.Numerics.SecureBigInteger(
				BigInteger.Parse("123456789012345678901234567890123456789")
			);

		// ---- SecureInt leaf readers (one per passing mode) ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadIntValue(SecureInt v) => v.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadIntIn(in SecureInt v) => v.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadIntRef(ref SecureInt v) => v.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadIntRefReadonly(ref readonly SecureInt v) => v.Decrypted;

		[Benchmark]
		public int Int_ByValue()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadIntValue(_int);
			}
			return acc;
		}

		[Benchmark]
		public int Int_ByIn()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadIntIn(in _int);
			}
			return acc;
		}

		[Benchmark]
		public int Int_ByRef()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadIntRef(ref _int);
			}
			return acc;
		}

		[Benchmark]
		public int Int_ByRefReadonly()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadIntRefReadonly(ref _int);
			}
			return acc;
		}

		// ---- SecureGuid leaf readers ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadGuidValue(SecureGuid v) => v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadGuidIn(in SecureGuid v) => v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadGuidRef(ref SecureGuid v) => v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadGuidRefReadonly(ref readonly SecureGuid v) =>
			v.Decrypted.GetHashCode();

		[Benchmark]
		public int Guid_ByValue()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadGuidValue(_guid);
			}
			return acc;
		}

		[Benchmark]
		public int Guid_ByIn()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadGuidIn(in _guid);
			}
			return acc;
		}

		[Benchmark]
		public int Guid_ByRef()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadGuidRef(ref _guid);
			}
			return acc;
		}

		[Benchmark]
		public int Guid_ByRefReadonly()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadGuidRefReadonly(ref _guid);
			}
			return acc;
		}

		// ---- SecureMatrix4x4 leaf readers ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadMatrixValue(SecureValue.Numerics.SecureMatrix4x4 v) =>
			(int)v.Decrypted.M11;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadMatrixIn(in SecureValue.Numerics.SecureMatrix4x4 v) =>
			(int)v.Decrypted.M11;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadMatrixRef(ref SecureValue.Numerics.SecureMatrix4x4 v) =>
			(int)v.Decrypted.M11;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadMatrixRefReadonly(
			ref readonly SecureValue.Numerics.SecureMatrix4x4 v
		) => (int)v.Decrypted.M11;

		[Benchmark]
		public int Matrix_ByValue()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadMatrixValue(_matrix);
			}
			return acc;
		}

		[Benchmark]
		public int Matrix_ByIn()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadMatrixIn(in _matrix);
			}
			return acc;
		}

		[Benchmark]
		public int Matrix_ByRef()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadMatrixRef(ref _matrix);
			}
			return acc;
		}

		[Benchmark]
		public int Matrix_ByRefReadonly()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadMatrixRefReadonly(ref _matrix);
			}
			return acc;
		}

		// ---- SecureString leaf readers ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadStringValue(SecureString v) => v.Decrypted?.GetHashCode() ?? 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadStringIn(in SecureString v) => v.Decrypted?.GetHashCode() ?? 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadStringRef(ref SecureString v) => v.Decrypted?.GetHashCode() ?? 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadStringRefReadonly(ref readonly SecureString v) =>
			v.Decrypted?.GetHashCode() ?? 0;

		[Benchmark]
		public int String_ByValue()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadStringValue(_string);
			}
			return acc;
		}

		[Benchmark]
		public int String_ByIn()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadStringIn(in _string);
			}
			return acc;
		}

		[Benchmark]
		public int String_ByRef()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadStringRef(ref _string);
			}
			return acc;
		}

		[Benchmark]
		public int String_ByRefReadonly()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadStringRefReadonly(ref _string);
			}
			return acc;
		}

		// ---- SecureBigInteger leaf readers ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadBigIntegerValue(SecureValue.Numerics.SecureBigInteger v) =>
			v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadBigIntegerIn(in SecureValue.Numerics.SecureBigInteger v) =>
			v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadBigIntegerRef(ref SecureValue.Numerics.SecureBigInteger v) =>
			v.Decrypted.GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadBigIntegerRefReadonly(
			ref readonly SecureValue.Numerics.SecureBigInteger v
		) => v.Decrypted.GetHashCode();

		[Benchmark]
		public int BigInteger_ByValue()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadBigIntegerValue(_bigInteger);
			}
			return acc;
		}

		[Benchmark]
		public int BigInteger_ByIn()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadBigIntegerIn(in _bigInteger);
			}
			return acc;
		}

		[Benchmark]
		public int BigInteger_ByRef()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadBigIntegerRef(ref _bigInteger);
			}
			return acc;
		}

		[Benchmark]
		public int BigInteger_ByRefReadonly()
		{
			int acc = 0;
			for (int i = 0; i < Iterations; i++)
			{
				acc += ReadBigIntegerRefReadonly(ref _bigInteger);
			}
			return acc;
		}
	}
}
