#nullable enable
#if UNITY_EDITOR
using System;
using NUnit.Framework;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit SecureStringSpanConversionTests: span to
	/// SecureString implicit conversions (both Span and ReadOnlySpan sources),
	/// string/null source binding, and inline-length allocation freedom.
	/// </summary>
	public class SecureStringSpanConversionTests
	{
		[TestCase("")]
		[TestCase("a")]
		[TestCase("crown_01")]
		[TestCase("abcdefghijklmnop")]
		[TestCase("hello world, this is a longer string for testing spans!")]
		[TestCase("héllo wörld🌍")]
		public void ReadOnlySpanAssignment_RoundTrips(string plain)
		{
			ReadOnlySpan<char> span = plain.AsSpan();
			SecureString s = span;
			Assert.AreEqual(plain.Length, s.Length);
			Assert.AreEqual(plain, s.Decrypted);
			Assert.IsTrue(s.SequenceEqual(plain.AsSpan()));
		}

		[TestCase("")]
		[TestCase("a")]
		[TestCase("crown_01")]
		[TestCase("abcdefghijklmnop")]
		[TestCase("hello world, this is a longer string for testing spans!")]
		public void SpanAssignment_RoundTrips(string plain)
		{
			Span<char> span = plain.ToCharArray().AsSpan();
			SecureString s = span;
			Assert.AreEqual(plain.Length, s.Length);
			Assert.AreEqual(plain, s.Decrypted);
		}

		[Test]
		public void InlineStackallocAssignment_RoundTrips()
		{
			Span<char> buffer = stackalloc char[16];
			"wpn_excalibur_01".AsSpan().CopyTo(buffer);
			SecureString s = buffer;
			Assert.AreEqual("wpn_excalibur_01", s.Decrypted);
		}

		[Test]
		public void EmptySpanAssignment_IsEmpty()
		{
			SecureString s = ReadOnlySpan<char>.Empty;
			Assert.AreEqual(0, s.Length);
			Assert.AreEqual(string.Empty, s.Decrypted);
		}

		[Test]
		public void StringSources_StillBindStringConversion()
		{
			SecureString fromLiteral = "crown_01";
			Assert.AreEqual("crown_01", fromLiteral.Decrypted);

			string value = "crown_01";
			SecureString fromVar = value;
			Assert.AreEqual("crown_01", fromVar.Decrypted);

			SecureString fromNull = (string)null!;
			Assert.AreEqual(string.Empty, fromNull.Decrypted);
		}

		[Test]
		public void SpanAssignment_InlineLengthIsAllocationFree()
		{
			ReadOnlySpan<char> span = "allocation probe".AsSpan();
			Consume(span);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			Consume(span);
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.AreEqual(before, after);
		}

		private static void Consume(ReadOnlySpan<char> span)
		{
			for (int i = 0; i < 1000; i++)
			{
				SecureString s = span;
				if (s.Length != span.Length)
				{
					throw new InvalidOperationException("Corrupted span conversion.");
				}
			}
		}
	}
}
#endif
