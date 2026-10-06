#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit SecurityTests, executed under Unity's CLR to prove
	/// the cipher, diffusion and tamper machinery behave identically there: avalanche
	/// diffusion through the internal seal path, cross-word chaining, the tamper event
	/// contract, fail-closed zeroed cells, and primitive interop. Fully-qualified wrapper
	/// names throughout. System.Numerics is deliberately NOT imported (its Vector types
	/// collide with UnityEngine's); PopCount is reached by full qualification.
	/// </summary>
	public class SecurityTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		// System.Numerics.BitOperations is not in Unity's profile: self-contained PopCount.
		private static int Hamming(ulong a, ulong b)
		{
			ulong v = a ^ b;
			v -= (v >> 1) & 0x5555555555555555UL;
			v = ((v >> 2) & 0x3333333333333333UL) + (v & 0x3333333333333333UL);
			v = (v + (v >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
			return (int)((v * 0x0101010101010101UL) >> 56);
		}

		[Test]
		public void Avalanche_OneBitPlaintextChange_ChangesAboutHalfTheCipherBits()
		{
			const int samples = 2000;
			long totalFlips = 0;
			var rng = new System.Random(12345);
			byte[] buf = new byte[16];
			for (int i = 0; i < samples; i++)
			{
				rng.NextBytes(buf);
				ulong plain = BitConverter.ToUInt64(buf, 0) * BitConverter.ToUInt64(buf, 8);
				rng.NextBytes(buf);
				ulong salt = BitConverter.ToUInt64(buf, 0);
				ulong flipped = plain ^ (1UL << rng.Next(64));
				totalFlips += Hamming(
					Vault.Seal(plain, salt).Cipher,
					Vault.Seal(flipped, salt).Cipher
				);
			}
			double average = totalFlips / (double)samples;
			Assert.GreaterOrEqual(average, 24.0);
			Assert.LessOrEqual(average, 40.0);
		}

		[Test]
		public void Avalanche_CipherBearsNoResemblanceToPlaintext()
		{
			const int samples = 1000;
			long totalDistance = 0;
			var rng = new System.Random(6789);
			byte[] buf = new byte[16];
			for (int i = 0; i < samples; i++)
			{
				rng.NextBytes(buf);
				ulong plain = BitConverter.ToUInt64(buf, 0) * BitConverter.ToUInt64(buf, 8);
				rng.NextBytes(buf);
				ulong c1 = Vault.Seal(plain, BitConverter.ToUInt64(buf, 0)).Cipher;
				rng.NextBytes(buf);
				ulong c2 = Vault.Seal(plain, BitConverter.ToUInt64(buf, 8)).Cipher;
				totalDistance += Hamming(c1, plain);
				totalDistance += Hamming(c1, c2);
			}
			double averageDistance = totalDistance / (double)(samples * 2);
			Assert.GreaterOrEqual(averageDistance, 24.0);
			Assert.LessOrEqual(averageDistance, 40.0);
		}

		[Test]
		public void Cell128_LoChange_DiffusesThroughBothWords()
		{
			// Cross-word chaining is lo->hi one-directional: flipping the lo word must
			// move BOTH output words (same salt isolates the plaintext change).
			ulong salt = Vault.NextRandom();
			var first = Vault.Seal128(0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, salt);
			var second = Vault.Seal128(0x0123456789ABCDEEUL, 0xFEDCBA9876543210UL, salt);
			Assert.AreNotEqual(first.Lo, second.Lo);
			Assert.AreNotEqual(first.Hi, second.Hi);
		}

		[Test]
		public void TamperingDetected_EventFiresBeforeThrow()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			object box = (SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f);
			CorruptCells(box);
			Assert.Throws<TamperedException>(() =>
			{
				Vector3 x = (SecureValue.Unity.SecureVector3)box;
			});
			Assert.AreEqual(baseline + 1, fired);

			object secondBox = (SecureValue.Unity.SecureVector3)new Vector3(4f, 5f, 6f);
			CorruptCells(secondBox);
			Assert.Throws<TamperedException>(() =>
			{
				Vector3 x = (SecureValue.Unity.SecureVector3)secondBox;
			});
			Assert.AreEqual(baseline + 1, fired);
		}

		[Test]
		public void TamperingDetected_HandlersCanBeRemoved()
		{
			// Listeners can be unsubscribed (test teardown, domain reload).
			int fired = 0;
			Action handler = () => fired++;
			TamperingNotifier.TamperingDetected += handler;
			TamperingNotifier.TamperingDetected -= handler;

			object box = (SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f);
			CorruptCells(box);
			Assert.Throws<TamperedException>(() =>
			{
				Vector3 x = (SecureValue.Unity.SecureVector3)box;
			});
			Assert.AreEqual(0, fired);
		}

		[Test]
		public void HasDetectedTampering_FalseBeforeTampering()
		{
			Assert.IsFalse(TamperingNotifier.HasDetectedTampering);
		}

		[Test]
		public void HasDetectedTampering_TrueAfterTampering()
		{
			object box = (SecureValue.Unity.SecureVector3)new Vector3(1f, 2f, 3f);
			CorruptCells(box);
			Assert.Throws<TamperedException>(() =>
			{
				Vector3 x = (SecureValue.Unity.SecureVector3)box;
			});
			Assert.IsTrue(TamperingNotifier.HasDetectedTampering);
		}

		[Test]
		public void HasDetectedTampering_StaysTrueWhenEventCapped()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			// The first tamper read dispatches; the second is suppressed,
			// but HasDetectedTampering stays true.
			for (int i = 0; i < 2; i++)
			{
				object box = (SecureValue.Unity.SecureVector3)new Vector3(i + 1f, 2f, 3f);
				CorruptCells(box);
				Assert.Throws<TamperedException>(() =>
				{
					Vector3 x = (SecureValue.Unity.SecureVector3)box;
				});
			}
			Assert.AreEqual(baseline + 1, fired);
			Assert.IsTrue(TamperingNotifier.HasDetectedTampering);
		}

		[Test]
		public void ZeroedLiveCell_ThrowsUninitializedWithoutNotifying()
		{
			// Zeroing every backing field of a live value fails closed as
			// uninitialized — never a tamper event (nothing was forged into).
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			object box = (SecureValue.Unity.SecureBounds)
				new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
			Type t = box.GetType();
			foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (f.FieldType != typeof(Cell) && f.FieldType != typeof(Cell128))
				{
					continue;
				}
				object cell = f.GetValue(box);
				Type cellType = cell.GetType();
				foreach (
					FieldInfo word in cellType.GetFields(
						BindingFlags.NonPublic | BindingFlags.Instance
					)
				)
				{
					word.SetValue(cell, word.FieldType == typeof(uint) ? (object)0U : (object)0UL);
				}
				f.SetValue(box, cell);
			}
			SecureValue.Unity.SecureBounds zeroed = (SecureValue.Unity.SecureBounds)box;
			Assert.Throws<UninitializedException>(() =>
			{
				Bounds x = zeroed;
			});
			Assert.AreEqual(0, fired);
		}

		[Test]
		public void Wrappers_InteroperateWithUnderlyingPrimitives()
		{
			SecureValue.Unity.SecureVector2Int p = new Vector2Int(10, 20);
			Assert.AreEqual(
				new Vector2Int(15, 25),
				(Vector2Int)(p + (SecureValue.Unity.SecureVector2Int)new Vector2Int(5, 5))
			);
			Assert.AreEqual(
				new Vector2Int(15, 25),
				(Vector2Int)((SecureValue.Unity.SecureVector2Int)new Vector2Int(5, 5) + p)
			);
			Assert.AreEqual(
				new Vector2Int(5, 15),
				(Vector2Int)(p - (SecureValue.Unity.SecureVector2Int)new Vector2Int(5, 5))
			);
			Assert.True(p == (SecureValue.Unity.SecureVector2Int)new Vector2Int(10, 20));
			Assert.True(p != (SecureValue.Unity.SecureVector2Int)new Vector2Int(0, 0));

			SecureValue.Unity.SecureColor c = new Color(0.5f, 0.5f, 0.5f, 1f);
			Assert.AreEqual(
				new Color(1f, 1f, 1f, 1f),
				(Color)(c + (SecureValue.Unity.SecureColor)new Color(0.5f, 0.5f, 0.5f, 0f))
			);
			Assert.True(c == (SecureValue.Unity.SecureColor)new Color(0.5f, 0.5f, 0.5f, 1f));
		}

		[Test]
		public void SamePlaintext_DifferentCiphertext()
		{
			// No reusable ciphertext pattern for a value: identical plaintexts in
			// different fields must differ in storage.
			SecureValue.Unity.SecureVector3 a = new Vector3(100f, 200f, 300f);
			SecureValue.Unity.SecureVector3 b = new Vector3(100f, 200f, 300f);
			Assert.AreEqual((Vector3)a, (Vector3)b);
			Assert.AreNotEqual(FirstWord(a), FirstWord(b));
		}

		private static ulong FirstWord<TWrapper>(TWrapper wrapper)
		{
			object box = wrapper;
			FieldInfo cellField = null;
			foreach (
				FieldInfo f in box.GetType()
					.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			)
			{
				if (f.FieldType == typeof(Cell) || f.FieldType == typeof(Cell128))
				{
					cellField = f;
					break;
				}
			}
			Assert.NotNull(cellField);
			object cell = cellField.GetValue(box);
			FieldInfo cipher =
				cell.GetType().GetField("_cipher", BindingFlags.NonPublic | BindingFlags.Instance)
				?? cell.GetType()
					.GetField("_cipherLo", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(cipher);
			return (ulong)cipher.GetValue(cell);
		}

		private static void CorruptCells(object box)
		{
			// Same uniform path as WrapperTests: flip BOTH cipher words directly
			// (Cell128 has no CorruptForTesting hook). Singly damaged cells now
			// restore instead of throwing, so throw-tests must damage both copies.
			Type t = box.GetType();
			foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (f.FieldType == typeof(Cell) || f.FieldType == typeof(Cell128))
				{
					object cell = f.GetValue(box);
					Type cellType = cell.GetType();
					foreach (
						string name in new[] { "_cipher", "_cipherB", "_cipherLo", "_cipherLoB" }
					)
					{
						FieldInfo cipher = cellType.GetField(
							name,
							BindingFlags.NonPublic | BindingFlags.Instance
						);
						if (cipher != null)
						{
							cipher.SetValue(cell, (ulong)cipher.GetValue(cell) ^ 1UL);
						}
					}
					f.SetValue(box, cell);
				}
			}
		}
	}
}
#endif
