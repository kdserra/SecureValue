#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;

namespace SecureValue.Tests
{
	/// <summary>
	/// Bulk sequence conversions (<see cref="SecureValueExt"/>): per-type
	/// <c>ToSecure</c>/<c>ToDecrypted</c> round-trips for every core and
	/// <c>System.Numerics</c> wrapper, order preservation, empty sequences,
	/// lazy enumeration, null-sequence/element rejection, and unset reads.
	/// UnityEngine variants are covered by the Unity EditMode mirror.
	/// </summary>
	public class SecureValueExtTests
	{
		private static void RoundTrip<T>(IEnumerable<T> plain, Func<IEnumerable<T>, IEnumerable<T>> cycle)
		{
			T[] expected = plain.ToArray();
			T[] actual = cycle(expected).ToArray();
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Core_RoundTripsPreservingOrder()
		{
			RoundTrip(new[] { true, false, true }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new byte[] { 0, 1, 255 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new sbyte[] { -128, 0, 127 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new short[] { -32768, 0, 32767 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new ushort[] { 0, 1, 65535 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 3, 1, 2, -5 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new uint[] { 0, 1, uint.MaxValue }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 3L, 1L, -2L }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new ulong[] { 0, 1, ulong.MaxValue }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 3.5f, 1.25f, -0.5f }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 3.5, 1.25, -0.5 }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 3.5m, 1.25m, -0.5m }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { 'c', 'a', 'b' }, v => v.ToSecure().ToDecrypted());
			RoundTrip(
				new[] { "crown_01", string.Empty, "a much longer string that lives on the heap for sure" },
				v => v.ToSecure().ToDecrypted()
			);
			Guid g = Guid.NewGuid();
			RoundTrip(new[] { g, Guid.Empty }, v => v.ToSecure().ToDecrypted());
			DateTime dt = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
			RoundTrip(new[] { dt, DateTime.MinValue }, v => v.ToSecure().ToDecrypted());
			DateTimeOffset dto = new(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2));
			RoundTrip(new[] { dto, DateTimeOffset.MinValue }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { TimeSpan.FromDays(1), TimeSpan.Zero }, v => v.ToSecure().ToDecrypted());
		}

#if NET
		[Fact]
		public void Core_RuneRoundTrip()
		{
			RoundTrip(
				new[] { new System.Text.Rune('A'), new System.Text.Rune(0x1F600) },
				v => v.ToSecure().ToDecrypted()
			);
		}
#endif

#if NET6_0_OR_GREATER
		[Fact]
		public void Core_DateOnlyTimeOnlyRoundTrips()
		{
			RoundTrip(new[] { new DateOnly(2026, 1, 2), DateOnly.MinValue }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { new TimeOnly(3, 4, 5), TimeOnly.MinValue }, v => v.ToSecure().ToDecrypted());
		}
#endif

		[Fact]
		public void Numerics_RoundTrips()
		{
			RoundTrip(
				new[] { BigInteger.Pow(2, 128), BigInteger.MinusOne, BigInteger.Zero },
				v => v.ToSecure().ToDecrypted()
			);
			RoundTrip(new[] { new Complex(3, 4), Complex.Zero }, v => v.ToSecure().ToDecrypted());
			RoundTrip(
				new[] { new Matrix3x2(1, 2, 3, 4, 5, 6), default(Matrix3x2) },
				v => v.ToSecure().ToDecrypted()
			);
			RoundTrip(new[] { Matrix4x4.Identity, default(Matrix4x4) }, v => v.ToSecure().ToDecrypted());
			RoundTrip(
				new[] { new Plane(new Vector3(0, 1, 0), 5), default(Plane) },
				v => v.ToSecure().ToDecrypted()
			);
			RoundTrip(
				new[] { Quaternion.Identity, new Quaternion(1, 2, 3, 4) },
				v => v.ToSecure().ToDecrypted()
			);
			RoundTrip(new[] { new Vector2(1, 2), Vector2.Zero }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { new Vector3(1, 2, 3), Vector3.Zero }, v => v.ToSecure().ToDecrypted());
			RoundTrip(new[] { new Vector4(1, 2, 3, 4), Vector4.Zero }, v => v.ToSecure().ToDecrypted());
		}

		[Fact]
		public void EmptySequences_RoundTripToEmpty()
		{
			Assert.Empty(Array.Empty<int>().ToSecure());
			Assert.Empty(Array.Empty<SecureInt>().ToDecrypted());
			Assert.Empty(Array.Empty<string>().ToSecure());
			Assert.Empty(Array.Empty<Vector3>().ToSecure());
		}

		[Fact]
		public void NullSequence_ThrowsOnEnumeration()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = ((IEnumerable<int>)null!).ToSecure().ToArray();
			});
			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = ((IEnumerable<SecureInt>)null!).ToDecrypted().ToArray();
			});
			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = ((IEnumerable<Vector3>)null!).ToSecure().ToArray();
			});
		}

		[Fact]
		public void NullStringElement_ThrowsOnEnumeration()
		{
			string?[] values = new[] { "ok", null };
			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = values!.ToSecure().ToArray();
			});
		}

		[Fact]
		public void Conversions_AreLazy()
		{
			static IEnumerable<int> Throwing()
			{
				throw new InvalidOperationException("Must not enumerate yet.");
#pragma warning disable CS0162
				yield break;
#pragma warning restore CS0162
			}
			// No enumeration happens at call time.
			IEnumerable<SecureInt> secured = Throwing().ToSecure();
			Assert.Throws<InvalidOperationException>(() => secured.ToArray());
		}

		[Fact]
		public void UnsetElements_DecryptToDefault()
		{
			int[] back = new[] { default(SecureInt) }.ToDecrypted().ToArray();
			Assert.Equal(new[] { 0 }, back);
			string[] strings = new[] { default(SecureString) }.ToDecrypted().ToArray();
			Assert.Equal(new[] { string.Empty }, strings);
		}
	}
}
