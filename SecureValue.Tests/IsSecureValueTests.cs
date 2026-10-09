using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// ISecureValue{T} conformance across every wrapper: any public struct
	/// exposing Decrypted must implement the contract, read default while
	/// unset, and fail TryDecrypt while unset. Constrained-generic calls
	/// must not box (zero-alloc proof).
	/// </summary>
	public class IsSecureValueTests
	{
		private static IEnumerable<Type> WrapperTypes() =>
			typeof(SecureInt)
				.Assembly.GetTypes()
				.Where(t => t.IsValueType && t.IsPublic && t.GetProperty("Decrypted") is not null)
				.OrderBy(t => t.FullName, StringComparer.Ordinal);

		public static IEnumerable<object[]> Wrappers() =>
			WrapperTypes().Select(t => new object[] { t });

		[Theory]
		[MemberData(nameof(Wrappers))]
		public void Wrapper_ImplementsISecureValue(Type wrapper)
		{
			Type contracted = typeof(ISecureValue<>).MakeGenericType(
				wrapper.GetProperty("Decrypted")!.PropertyType
			);
			Assert.True(
				contracted.IsAssignableFrom(wrapper),
				$"{wrapper.Name} must implement {contracted.Name}"
			);
		}

		[Theory]
		[MemberData(nameof(Wrappers))]
		public void Wrapper_UnsetReadsDefault(Type wrapper)
		{
			object unset = Activator.CreateInstance(wrapper)!;
			Assert.True((bool)wrapper.GetProperty("IsUnset")!.GetValue(unset)!);
			Type valueType = wrapper.GetProperty("Decrypted")!.PropertyType;
			object? expected = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
			Assert.Equal(expected, wrapper.GetProperty("Decrypted")!.GetValue(unset));
		}

		[Theory]
		[MemberData(nameof(Wrappers))]
		public void Wrapper_UnsetTryDecryptIsFalse(Type wrapper)
		{
			object unset = Activator.CreateInstance(wrapper)!;
			object?[] args = new object?[] { null };
			// ByRef-like out args round-trip through the boxed copy; the
			// return value is what the contract guarantees.
			object? result = wrapper.GetMethod("TryDecrypt")!.Invoke(unset, args);
			Assert.False((bool)result!);
		}

		[Fact]
		public void SealedValues_SatisfyContract()
		{
			ISecureValue<int> i = new SecureInt(851);
			Assert.False(i.IsUnset);
			Assert.Equal(851, i.Decrypted);
			Assert.True(i.TryDecrypt(out int iv) && iv == 851);

			ISecureValue<string> s = new SecureString("hi");
			Assert.False(s.IsUnset);
			Assert.Equal("hi", s.Decrypted);
			Assert.True(s.TryDecrypt(out string? sv) && sv == "hi");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static TValue ReadConstrained<TWrapper, TValue>(ref TWrapper wrapper)
			where TWrapper : struct, ISecureValue<TValue> => wrapper.Decrypted!;

		[Fact]
		public void ConstrainedCall_DoesNotBox()
		{
			var wrapper = new SecureInt(851);
			// Warmup (JIT) outside measurement.
			int warm = ReadConstrained<SecureInt, int>(ref wrapper);
			Assert.Equal(851, warm);

			long before = GC.GetAllocatedBytesForCurrentThread();
			int acc = 0;
			for (int n = 0; n < 100_000; n++)
			{
				acc += ReadConstrained<SecureInt, int>(ref wrapper);
			}
			long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			Assert.Equal(851 * 100_000, acc);
			Assert.Equal(0, allocated);
		}
	}
}
