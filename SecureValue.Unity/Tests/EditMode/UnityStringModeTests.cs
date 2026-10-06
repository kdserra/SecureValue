#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode coverage for the SecureString dual store (inline at or under
	/// 16 chars, heap above): serialization parity per mode, round-trips,
	/// mode-flip tamper, and capacity routing. Core SecureString runs in Unity
	/// via staged sources. Sandbox-gated like all Unity tests: nothing here
	/// compiles repo-side.
	/// </summary>
	public class UnityStringModeTests
	{
		[SetUp]
		public void ResetTamperThrottle()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

		private static string ModeOf(object box) =>
			box.GetType().GetField("_mode", Flags).GetValue(box).ToString();

		[Test]
		public void SerializationParity_InlineAndHeap()
		{
			CheckParity("short", "Inline");
			CheckParity("this string is long enough to live on the heap happily", "Heap");
		}

		[Test]
		public void Capacity_RoutesByLength()
		{
			Assert.AreEqual("Inline", ModeOf((SecureString)""));
			Assert.AreEqual("Inline", ModeOf((SecureString)"1234567890123456"));
			Assert.AreEqual("Heap", ModeOf((SecureString)"12345678901234567"));
		}

		[Test]
		public void RoundTrip_CopyToAndLength()
		{
			SecureString s = "item name";
			Assert.AreEqual(9, s.Length);
			Assert.AreEqual("item name", (string)s);
			char[] buffer = new char[s.Length];
			s.CopyTo(buffer);
			Assert.AreEqual("item name", new string(buffer));
		}

		[Test]
		public void RoundTrip_TryCopyToNeverThrows()
		{
			SecureString s = "item name";
			char[] buffer = new char[s.Length];
			Assert.IsTrue(s.TryCopyTo(buffer, out int written));
			Assert.AreEqual(9, written);
			Assert.AreEqual("item name", new string(buffer));
			Assert.IsFalse(s.TryCopyTo(new char[2], out int shortWritten));
			Assert.AreEqual(0, shortWritten);
		}

		[Test]
		public void Tamper_ModeFlip_ThrowsUninitialized()
		{
			object box = (SecureString)"small";
			Type modeType = box.GetType().GetNestedType("StorageMode", Flags);
			Assert.NotNull(modeType);
			box.GetType().GetField("_mode", Flags).SetValue(box, Enum.Parse(modeType, "Heap"));
			Assert.Throws<UninitializedException>(() =>
			{
				_ = ((SecureString)box).Decrypted;
			});
		}

		[Test]
		public void HealOnRead_InlineSingleWord_SecondReadSilent()
		{
			object box = (SecureString)"backup me";
			FieldInfo word = typeof(SecureString).GetField("_w0", Flags);
			word.SetValue(box, (ulong)word.GetValue(box) ^ 1UL);
			var p = (SecureString)box;

			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.AreEqual("backup me", p.Decrypted);
			Assert.AreEqual(1, fired);
			Assert.AreEqual("backup me", p.Decrypted);
			Assert.AreEqual(1, fired);
		}

		[Test]
		public void Tamper_TamperedPack_Throws()
		{
			object box = (SecureString)"short";
			uint[] packed = ((ISecureSerialization)box).SaveToSerialized();
			int halfUints = (packed.Length - 1) / 2;
			packed[3] ^= 0xFFFFFFFFU;
			packed[3 + halfUints] ^= 0xFFFFFFFFU;
			object fresh = default(SecureString);
			Assert.Throws<TamperedException>(() =>
			{
				((ISecureSerialization)fresh).LoadFromSerialized(packed);
			});
		}

		private static void CheckParity(string plain, string expectedMode)
		{
			object box = (SecureString)plain;
			Assert.AreEqual(expectedMode, ModeOf(box));
			((ISerializationCallbackReceiver)box).OnBeforeSerialize();
			((ISerializationCallbackReceiver)box).OnAfterDeserialize();
			SecureString restored = (SecureString)box;
			Assert.AreEqual(plain, (string)restored);
			Assert.AreEqual(expectedMode, ModeOf(restored));
		}
	}
}
#endif
