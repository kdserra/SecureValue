using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// Direct operation comparisons for plain values and SecureValue wrappers.
	/// The benchmark methods return the actual value instead of hashing it, so
	/// the result sink does not add unrelated hash-code work to the measurement.
	///
	/// Primitive and SecureValue prefixes identify the storage/operation family.
	/// Span, CopyTo, and StackDecrypt cases apply only to strings because those
	/// APIs do not exist for int or float.
	///
	/// Run with: --filter *PureBenchmarks*
	/// </summary>
	[MemoryDiagnoser]
	public class PureBenchmarks
	{
		private int _primitiveInt = 123456;
		private SecureInt _secureInt = 123456;
		private float _primitiveFloat = 3.14159f;
		private SecureFloat _secureFloat = 3.14159f;
		private string _primitiveString = "The quick brown fox";
		private SecureString _secureString = "The quick brown fox";
		private char[] _stringChars = Array.Empty<char>();
		private char[] _copyBuffer = Array.Empty<char>();

		[GlobalSetup]
		public void Setup()
		{
			_stringChars = _primitiveString.ToCharArray();
			_copyBuffer = new char[_secureString.Length];
		}

		// ---- int: direct read/write and parameter passing ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadPrimitiveIntRef(ref int value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadPrimitiveIntIn(in int value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadPrimitiveIntRefReadonly(ref readonly int value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadSecureValueIntRef(ref SecureInt value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadSecureValueIntIn(in SecureInt value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int ReadSecureValueIntRefReadonly(ref readonly SecureInt value) =>
			value.Decrypted;

		[Benchmark]
		public int Primitive_Int_Read() => _primitiveInt;

		[Benchmark]
		public int Primitive_Int_Write() => 123456;

		[Benchmark]
		public int Primitive_Int_Ref() => ReadPrimitiveIntRef(ref _primitiveInt);

		[Benchmark]
		public int Primitive_Int_In() => ReadPrimitiveIntIn(in _primitiveInt);

		[Benchmark]
		public int Primitive_Int_RefReadonly() => ReadPrimitiveIntRefReadonly(ref _primitiveInt);

		[Benchmark]
		public int SecureValue_Int_Read() => _secureInt.Decrypted;

		[Benchmark]
		public SecureInt SecureValue_Int_Write() => new SecureInt(123456);

		[Benchmark]
		public int SecureValue_Int_Ref() => ReadSecureValueIntRef(ref _secureInt);

		[Benchmark]
		public int SecureValue_Int_In() => ReadSecureValueIntIn(in _secureInt);

		[Benchmark]
		public int SecureValue_Int_RefReadonly() => ReadSecureValueIntRefReadonly(ref _secureInt);

		// ---- float: direct read/write and parameter passing ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadPrimitiveFloatRef(ref float value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadPrimitiveFloatIn(in float value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadPrimitiveFloatRefReadonly(ref readonly float value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadSecureValueFloatRef(ref SecureFloat value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadSecureValueFloatIn(in SecureFloat value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static float ReadSecureValueFloatRefReadonly(ref readonly SecureFloat value) =>
			value.Decrypted;

		[Benchmark]
		public float Primitive_Float_Read() => _primitiveFloat;

		[Benchmark]
		public float Primitive_Float_Write() => 3.14159f;

		[Benchmark]
		public float Primitive_Float_Ref() => ReadPrimitiveFloatRef(ref _primitiveFloat);

		[Benchmark]
		public float Primitive_Float_In() => ReadPrimitiveFloatIn(in _primitiveFloat);

		[Benchmark]
		public float Primitive_Float_RefReadonly() =>
			ReadPrimitiveFloatRefReadonly(ref _primitiveFloat);

		[Benchmark]
		public float SecureValue_Float_Read() => _secureFloat.Decrypted;

		[Benchmark]
		public SecureFloat SecureValue_Float_Write() => new SecureFloat(3.14159f);

		[Benchmark]
		public float SecureValue_Float_Ref() => ReadSecureValueFloatRef(ref _secureFloat);

		[Benchmark]
		public float SecureValue_Float_In() => ReadSecureValueFloatIn(in _secureFloat);

		[Benchmark]
		public float SecureValue_Float_RefReadonly() =>
			ReadSecureValueFloatRefReadonly(ref _secureFloat);

		// ---- string: direct read/write and parameter passing ----

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string ReadPrimitiveStringRef(ref string value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string ReadPrimitiveStringIn(in string value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string ReadPrimitiveStringRefReadonly(ref readonly string value) => value;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string? ReadSecureValueStringRef(ref SecureString value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string? ReadSecureValueStringIn(in SecureString value) => value.Decrypted;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string? ReadSecureValueStringRefReadonly(ref readonly SecureString value) =>
			value.Decrypted;

		[Benchmark]
		public string Primitive_String_Read() => _primitiveString;

		[Benchmark]
		public string Primitive_String_Write() => "The quick brown fox";

		[Benchmark]
		public string Primitive_String_Ref() => ReadPrimitiveStringRef(ref _primitiveString);

		[Benchmark]
		public string Primitive_String_In() => ReadPrimitiveStringIn(in _primitiveString);

		[Benchmark]
		public string Primitive_String_RefReadonly() =>
			ReadPrimitiveStringRefReadonly(ref _primitiveString);

		[Benchmark]
		public string? SecureValue_String_Read() => _secureString.Decrypted;

		[Benchmark]
		public SecureString SecureValue_String_Write() => new SecureString("The quick brown fox");

		[Benchmark]
		public string? SecureValue_String_Ref() => ReadSecureValueStringRef(ref _secureString);

		[Benchmark]
		public string? SecureValue_String_In() => ReadSecureValueStringIn(in _secureString);

		[Benchmark]
		public string? SecureValue_String_RefReadonly() =>
			ReadSecureValueStringRefReadonly(ref _secureString);

		// ---- string spans ----

		[Benchmark]
		public char Primitive_String_ReadSpan() => _primitiveString.AsSpan()[0];

		[Benchmark]
		public string Primitive_String_WriteSpan() => new string(_stringChars);

		[Benchmark]
		public char SecureValue_String_ReadSpan_CopyTo()
		{
			_secureString.CopyTo(_copyBuffer);
			return _copyBuffer[0];
		}

		[Benchmark]
		public SecureString SecureValue_String_WriteSpan() =>
			SecureString.Parse(_stringChars, provider: null);

		[Benchmark]
		public int SecureValue_String_ReadSpan_StackDecrypt() =>
			_secureString.StackDecrypt(static chars => chars[0]);
	}
}
