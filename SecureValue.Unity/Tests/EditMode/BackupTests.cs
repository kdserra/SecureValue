#nullable enable
#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit BackupTests: a singly-tampered copy heals from
	/// the good one (raising, never throwing); only doubly-tampered reads fail.
	/// Fully-qualified wrappers, no dynamic.
	/// </summary>
	public class BackupTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Test]
		public void SinglePrimaryCorrupt_RestoresValueWithoutThrowing()
		{
			object box = (SecureInt)42;
			CorruptOneWord(box, "_cell", "_cipher");
			var p = (SecureInt)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual(42, (int)p);
			Assert.AreEqual(1, fired);
			// The direct read heals the damaged copy; cap-1 suppresses any later
			// dispatches in this process run.
			Assert.AreEqual(42, (int)p);
			Assert.AreEqual(1, fired);
			p = 42;
			Assert.AreEqual(42, (int)p);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void HealOnRead_SecondDirectReadSilent()
		{
			// Direct reads persist the heal (conversions above heal a discarded
			// copy instead, so they keep reporting).
			object box = (SecureInt)42;
			CorruptOneWord(box, "_cell", "_cipher");
			var p = (SecureInt)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual(42, p.Decrypted);
			Assert.AreEqual(1, fired);
			Assert.AreEqual(42, p.Decrypted);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void SingleBackupCorrupt_RestoresValueWithoutThrowing()
		{
			object box = (SecureInt)42;
			CorruptOneWord(box, "_cell", "_cipherB");
			var p = (SecureInt)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual(42, (int)p);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void BothCorrupt_ThrowsOnce()
		{
			object box = (SecureInt)7;
			CorruptOneWord(box, "_cell", "_cipher");
			CorruptOneWord(box, "_cell", "_cipherB");
			var p = (SecureInt)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Throws<TamperedException>(() =>
			{
				_ = (int)p;
			});
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void Copies_HaveIndependentCiphertext()
		{
			object box = (SecureInt)100;
			object cell = box.GetType()
				.GetField("_cell", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(box);
			Type cellType = cell.GetType();
			ulong salt = Word(cell, cellType, "_salt");
			ulong cipher = Word(cell, cellType, "_cipher");
			ulong saltB = Word(cell, cellType, "_saltB");
			ulong cipherB = Word(cell, cellType, "_cipherB");
			Assert.AreNotEqual(salt, saltB);
			Assert.AreNotEqual(cipher, cipherB);
		}

		[Test]
		public void UnityBounds_SingleCorrupt_RestoresValue()
		{
			Bounds expected = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
			object box = (SecureValue.Unity.SecureBounds)expected;
			CorruptOneWord(box, "_cellA", "_cipherLo");
			var p = (SecureValue.Unity.SecureBounds)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual(expected, (Bounds)p);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void UnityBounds_BothCorrupt_ThrowsOnce()
		{
			Bounds expected = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
			object box = (SecureValue.Unity.SecureBounds)expected;
			CorruptOneWord(box, "_cellA", "_cipherLo");
			CorruptOneWord(box, "_cellA", "_cipherLoB");
			var p = (SecureValue.Unity.SecureBounds)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.Throws<TamperedException>(() =>
			{
				_ = (Bounds)p;
			});
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void String_SingleArrayCorrupt_Recovers()
		{
			// Heap-backed (>16 chars): the live words are the _ciphers array.
			object box = (SecureString)"backup me, this is a longer string";
			FieldInfo ciphers = typeof(SecureString).GetField(
				"_ciphers",
				BindingFlags.NonPublic | BindingFlags.Instance
			);
			((ulong[])ciphers.GetValue(box))[0] ^= 1UL;
			var p = (SecureString)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual("backup me, this is a longer string", (string?)p);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void String_SingleInlineWordCorrupt_Recovers()
		{
			// Inline (<=16 chars): no array exists; damage one live word of
			// the primary copy instead. Must still recover + raise once.
			object box = (SecureString)"backup me";
			FieldInfo word = typeof(SecureString).GetField(
				"_w0",
				BindingFlags.NonPublic | BindingFlags.Instance
			);
			word.SetValue(box, (ulong)word.GetValue(box) ^ 1UL);
			var p = (SecureString)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual("backup me", (string?)p);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void TryDecrypt_SingleCorrupt_ReturnsTrueWithEvent()
		{
			object box = (SecureInt)5;
			CorruptOneWord(box, "_cell", "_cipher");
			var p = (SecureInt)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.IsTrue(p.TryDecrypt(out int value));
			Assert.AreEqual(5, value);
			Assert.AreEqual(1, fired);
		}

		private static ulong Word(object cell, Type cellType, string name)
		{
			return (ulong)
				cellType
					.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
					.GetValue(cell);
		}

		private static void CorruptOneWord(object box, string cellField, string cipherField)
		{
			FieldInfo cellF = box.GetType()
				.GetField(cellField, BindingFlags.NonPublic | BindingFlags.Instance);
			object cell = cellF.GetValue(box);
			FieldInfo cipher = cell.GetType()
				.GetField(cipherField, BindingFlags.NonPublic | BindingFlags.Instance);
			cipher.SetValue(cell, (ulong)cipher.GetValue(cell) ^ 1UL);
			cellF.SetValue(box, cell);
		}
	}
}
#endif
