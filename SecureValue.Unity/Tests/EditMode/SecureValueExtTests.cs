#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SecureValue;
using UnityEngine;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit SecureValueExtTests, extended with the UnityEngine
	/// variants: bulk <c>ToSecure</c>/<c>ToDecrypted</c> round-trips for core and Unity
	/// wrappers, order preservation, empty sequences, lazy enumeration, and
	/// null-sequence/element rejection.
	/// </summary>
	public class SecureValueExtTests
	{
		[Test]
		public void Core_RoundTripsPreservingOrder()
		{
			CollectionAssert.AreEqual(
				new[] { 3, 1, 2, -5 },
				new[] { 3, 1, 2, -5 }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[]
				{
					"crown_01",
					string.Empty,
					"a much longer string that lives on the heap for sure",
				},
				new[]
				{
					"crown_01",
					string.Empty,
					"a much longer string that lives on the heap for sure",
				}
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { 3.5, 1.25, -0.5 },
				new[] { 3.5, 1.25, -0.5 }.ToSecure().ToDecrypted().ToArray()
			);
			Guid g = Guid.NewGuid();
			CollectionAssert.AreEqual(
				new[] { g, Guid.Empty },
				new[] { g, Guid.Empty }.ToSecure().ToDecrypted().ToArray()
			);
		}

		[Test]
		public void Unity_RoundTrips()
		{
			CollectionAssert.AreEqual(
				new[] { new Vector2(1, 2), Vector2.zero },
				new[] { new Vector2(1, 2), Vector2.zero }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Vector2Int(1, 2), Vector2Int.zero },
				new[] { new Vector2Int(1, 2), Vector2Int.zero }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Vector3(1, 2, 3), Vector3.zero },
				new[] { new Vector3(1, 2, 3), Vector3.zero }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Vector3Int(1, 2, 3), Vector3Int.zero },
				new[] { new Vector3Int(1, 2, 3), Vector3Int.zero }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Vector4(1, 2, 3, 4), Vector4.zero },
				new[] { new Vector4(1, 2, 3, 4), Vector4.zero }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Rect(1, 2, 3, 4), new Rect() },
				new[] { new Rect(1, 2, 3, 4), new Rect() }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new RectInt(1, 2, 3, 4), new RectInt() },
				new[] { new RectInt(1, 2, 3, 4), new RectInt() }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Bounds(new Vector3(1, 2, 3), new Vector3(4, 5, 6)), new Bounds() },
				new[] { new Bounds(new Vector3(1, 2, 3), new Vector3(4, 5, 6)), new Bounds() }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[]
				{
					new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
					new BoundsInt(),
				},
				new[]
				{
					new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
					new BoundsInt(),
				}
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Color(0.1f, 0.2f, 0.3f, 0.4f), new Color() },
				new[] { new Color(0.1f, 0.2f, 0.3f, 0.4f), new Color() }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Color32(1, 2, 3, 4), new Color32() },
				new[] { new Color32(1, 2, 3, 4), new Color32() }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { Quaternion.identity, new Quaternion(1, 2, 3, 4) },
				new[] { Quaternion.identity, new Quaternion(1, 2, 3, 4) }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { Matrix4x4.identity, new Matrix4x4() },
				new[] { Matrix4x4.identity, new Matrix4x4() }.ToSecure().ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Plane(new Vector3(0, 1, 0), 5), new Plane() },
				new[] { new Plane(new Vector3(0, 1, 0), 5), new Plane() }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { new Ray(new Vector3(1, 2, 3), new Vector3(0, 1, 0)), new Ray() },
				new[] { new Ray(new Vector3(1, 2, 3), new Vector3(0, 1, 0)), new Ray() }
					.ToSecure()
					.ToDecrypted()
					.ToArray()
			);
			LayerMask[] masks = new[]
			{
				new LayerMask() { value = 7 },
				new LayerMask(),
			};
			int[] back = masks.ToSecure().ToDecrypted().ToArray().Select(m => m.value).ToArray();
			CollectionAssert.AreEqual(new[] { 7, 0 }, back);
		}

		[Test]
		public void EmptySequences_RoundTripToEmpty()
		{
			Assert.IsEmpty(new int[0].ToSecure());
			Assert.IsEmpty(new SecureInt[0].ToDecrypted());
			Assert.IsEmpty(new Vector3[0].ToSecure());
			Assert.IsEmpty(new SecureValue.Unity.SecureVector3[0].ToDecrypted());
		}

		[Test]
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

		[Test]
		public void NullStringElement_ThrowsOnEnumeration()
		{
			string?[] values = new[] { "ok", null };
			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = values!.ToSecure().ToArray();
			});
		}

		[Test]
		public void Conversions_AreLazy()
		{
			IEnumerable<SecureInt> secured = Throwing().ToSecure();
			Assert.Throws<InvalidOperationException>(() =>
			{
				_ = secured.ToArray();
			});
		}

		[Test]
		public void UnsetElements_DecryptToDefault()
		{
			CollectionAssert.AreEqual(
				new[] { 0 },
				new[] { default(SecureInt) }.ToDecrypted().ToArray()
			);
			CollectionAssert.AreEqual(
				new[] { string.Empty },
				new[] { default(SecureString) }.ToDecrypted().ToArray()
			);
		}

		private static IEnumerable<int> Throwing()
		{
			throw new InvalidOperationException("Must not enumerate yet.");
#pragma warning disable CS0162
			yield break;
#pragma warning restore CS0162
		}
	}
}
#endif
