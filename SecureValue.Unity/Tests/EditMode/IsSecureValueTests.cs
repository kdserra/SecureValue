#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit IsSecureValueTests: ISecureValue{T}
	/// conformance across every wrapper present in the Unity build (the sweep
	/// enumerates staged types, so Unity-excluded wrappers are simply absent).
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

		[TestCaseSource(nameof(Wrappers))]
		public void Wrapper_ImplementsISecureValue(Type wrapper)
		{
			Type contracted = typeof(ISecureValue<>).MakeGenericType(
				wrapper.GetProperty("Decrypted")!.PropertyType
			);
			Assert.IsTrue(
				contracted.IsAssignableFrom(wrapper),
				$"{wrapper.Name} must implement {contracted.Name}"
			);
		}

		[TestCaseSource(nameof(Wrappers))]
		public void Wrapper_UnsetReadsDefault(Type wrapper)
		{
			object unset = Activator.CreateInstance(wrapper)!;
			Assert.IsTrue((bool)wrapper.GetProperty("IsUnset")!.GetValue(unset)!);
			Type valueType = wrapper.GetProperty("Decrypted")!.PropertyType;
			object? expected = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
			Assert.AreEqual(expected, wrapper.GetProperty("Decrypted")!.GetValue(unset));
		}

		[TestCaseSource(nameof(Wrappers))]
		public void Wrapper_UnsetTryDecryptIsFalse(Type wrapper)
		{
			object unset = Activator.CreateInstance(wrapper)!;
			object?[] args = new object?[] { null };
			// ByRef-like out args round-trip through the boxed copy; the
			// return value is what the contract guarantees.
			object? result = wrapper.GetMethod("TryDecrypt")!.Invoke(unset, args);
			Assert.IsFalse((bool)result!);
		}

		[Test]
		public void SealedValues_SatisfyContract()
		{
			ISecureValue<int> i = new SecureInt(851);
			Assert.IsFalse(i.IsUnset);
			Assert.AreEqual(851, i.Decrypted);
			Assert.IsTrue(i.TryDecrypt(out int iv) && iv == 851);

			ISecureValue<string> s = new SecureString("hi");
			Assert.IsFalse(s.IsUnset);
			Assert.AreEqual("hi", s.Decrypted);
			Assert.IsTrue(s.TryDecrypt(out string? sv) && sv == "hi");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static TValue ReadConstrained<TWrapper, TValue>(ref TWrapper wrapper)
			where TWrapper : struct, ISecureValue<TValue> => wrapper.Decrypted!;

		[Test]
		public void ConstrainedCall_DoesNotBox()
		{
			var wrapper = new SecureInt(851);
			// Warmup (JIT) outside measurement.
			int warm = ReadConstrained<SecureInt, int>(ref wrapper);
			Assert.AreEqual(851, warm);

			long before = GC.GetAllocatedBytesForCurrentThread();
			int acc = 0;
			for (int n = 0; n < 100_000; n++)
			{
				acc += ReadConstrained<SecureInt, int>(ref wrapper);
			}
			long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			Assert.AreEqual(851 * 100_000, acc);
			Assert.AreEqual(0L, allocated);
		}
	}
}
#endif
