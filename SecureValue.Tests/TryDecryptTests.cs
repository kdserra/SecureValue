#nullable enable
using System;
using System.Numerics;
using System.Reflection;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// TryDecrypt: success round-trips, uninitialized-false (no event),
	/// tampered-false (exactly one event), and the tampered-throwing Decrypted staying intact.
	/// Shares the isolated TamperNotifier collection so exact event counts hold.
	/// </summary>
	[Collection("TamperNotifier")]
	public class TryDecryptTests
	{
		public TryDecryptTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		// ---------- success: one representative round-trip per type ----------

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Bool_Success(bool expected)
		{
			var p = new SecureBool(expected);
			Assert.True(p.TryDecrypt(out bool actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(255)]
		public void Byte_Success(byte expected)
		{
			var p = new SecureByte(expected);
			Assert.True(p.TryDecrypt(out byte actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(-128)]
		[InlineData(127)]
		public void SByte_Success(sbyte expected)
		{
			var p = new SecureSByte(expected);
			Assert.True(p.TryDecrypt(out sbyte actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(short.MinValue)]
		[InlineData(short.MaxValue)]
		public void Short_Success(short expected)
		{
			var p = new SecureShort(expected);
			Assert.True(p.TryDecrypt(out short actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(ushort.MinValue)]
		[InlineData(ushort.MaxValue)]
		public void UShort_Success(ushort expected)
		{
			var p = new SecureUShort(expected);
			Assert.True(p.TryDecrypt(out ushort actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(int.MinValue)]
		[InlineData(int.MaxValue)]
		public void Int_Success(int expected)
		{
			var p = new SecureInt(expected);
			Assert.True(p.TryDecrypt(out int actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0u)]
		[InlineData(uint.MaxValue)]
		public void UInt_Success(uint expected)
		{
			var p = new SecureUInt(expected);
			Assert.True(p.TryDecrypt(out uint actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(long.MinValue)]
		[InlineData(long.MaxValue)]
		public void Long_Success(long expected)
		{
			var p = new SecureLong(expected);
			Assert.True(p.TryDecrypt(out long actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0ul)]
		[InlineData(ulong.MaxValue)]
		public void ULong_Success(ulong expected)
		{
			var p = new SecureULong(expected);
			Assert.True(p.TryDecrypt(out ulong actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Float_Success()
		{
			var p = new SecureFloat(1.5f);
			Assert.True(p.TryDecrypt(out float actual));
			Assert.Equal(1.5f, actual);
		}

		[Fact]
		public void Double_Success()
		{
			var p = new SecureDouble(-1e300);
			Assert.True(p.TryDecrypt(out double actual));
			Assert.Equal(-1e300, actual);
		}

		[Fact]
		public void Decimal_Success()
		{
			var p = new SecureDecimal(123456.789m);
			Assert.True(p.TryDecrypt(out decimal actual));
			Assert.Equal(123456.789m, actual);
		}

		[Theory]
		[InlineData('S')]
		[InlineData('\0')]
		public void Char_Success(char expected)
		{
			var p = new SecureChar(expected);
			Assert.True(p.TryDecrypt(out char actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Rune_Success()
		{
			var expected = new System.Text.Rune(0x10FFFF);
			var p = new SecureRune(expected);
			Assert.True(p.TryDecrypt(out System.Text.Rune actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Guid_Success()
		{
			Guid expected = Guid.NewGuid();
			var p = new SecureGuid(expected);
			Assert.True(p.TryDecrypt(out Guid actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void DateTime_Success()
		{
			var expected = new DateTime(638000000000000000L, DateTimeKind.Utc);
			var p = new SecureDateTime(expected);
			Assert.True(p.TryDecrypt(out DateTime actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void DateTimeOffset_Success()
		{
			var expected = new DateTimeOffset(638000000000000000L, new TimeSpan(2, 0, 0));
			var p = new SecureDateTimeOffset(expected);
			Assert.True(p.TryDecrypt(out DateTimeOffset actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void DateOnly_Success()
		{
			var expected = new DateOnly(2026, 9, 25);
			var p = new SecureDateOnly(expected);
			Assert.True(p.TryDecrypt(out DateOnly actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TimeOnly_Success()
		{
			var expected = new TimeOnly(13, 37, 42);
			var p = new SecureTimeOnly(expected);
			Assert.True(p.TryDecrypt(out TimeOnly actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TimeSpan_Success()
		{
			var expected = new TimeSpan(1, 37, 42);
			var p = new SecureTimeSpan(expected);
			Assert.True(p.TryDecrypt(out TimeSpan actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("")]
		[InlineData("hello")]
		public void String_Success(string expected)
		{
			var p = new SecureString(expected);
			Assert.True(p.TryDecrypt(out string actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void String_Long_Success()
		{
			string expected = new string('x', 1000);
			var p = new SecureString(expected);
			Assert.True(p.TryDecrypt(out string actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BigInteger_Success()
		{
			BigInteger expected = BigInteger.Pow(2, 256) + 1;
			var p = new SecureBigInteger(expected);
			Assert.True(p.TryDecrypt(out BigInteger actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Complex_Success()
		{
			var expected = new Complex(1.5, -2.5);
			var p = new SecureComplex(expected);
			Assert.True(p.TryDecrypt(out Complex actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Vector2_Success()
		{
			var expected = new Vector2(1.5f, -2.5f);
			var p = new SecureVector2(expected);
			Assert.True(p.TryDecrypt(out Vector2 actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Vector3_Success()
		{
			var expected = new Vector3(1f, 2f, 3f);
			var p = new SecureVector3(expected);
			Assert.True(p.TryDecrypt(out Vector3 actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Vector4_Success()
		{
			var expected = new Vector4(1f, 2f, 3f, 4f);
			var p = new SecureVector4(expected);
			Assert.True(p.TryDecrypt(out Vector4 actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Quaternion_Success()
		{
			var expected = new Quaternion(0.5f, 0.25f, 0.125f, 0.0625f);
			var p = new SecureQuaternion(expected);
			Assert.True(p.TryDecrypt(out Quaternion actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Plane_Success()
		{
			var expected = new Plane(1f, 2f, 3f, 4f);
			var p = new SecurePlane(expected);
			Assert.True(p.TryDecrypt(out Plane actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Matrix3x2_Success()
		{
			var expected = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
			var p = new SecureMatrix3x2(expected);
			Assert.True(p.TryDecrypt(out Matrix3x2 actual));
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Matrix4x4_Success()
		{
			var expected = new Matrix4x4(
				1f,
				2f,
				3f,
				4f,
				5f,
				6f,
				7f,
				8f,
				9f,
				10f,
				11f,
				12f,
				13f,
				14f,
				15f,
				16f
			);
			var p = new SecureMatrix4x4(expected);
			Assert.True(p.TryDecrypt(out Matrix4x4 actual));
			Assert.Equal(expected, actual);
		}

		// ---------- uninitialized: false, default out, NO event ----------

		[Fact]
		public void Uninitialized_Cell_NoEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var p = default(SecureInt);
			Assert.False(p.TryDecrypt(out int value));
			Assert.Equal(default, value);
			Assert.Equal(baseline, fired);
			Assert.Equal(default, p.Decrypted);
		}

		[Fact]
		public void Uninitialized_Cell128_NoEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var p = default(SecureGuid);
			Assert.False(p.TryDecrypt(out Guid value));
			Assert.Equal(default, value);
			Assert.Equal(baseline, fired);
			Assert.Equal(default, p.Decrypted);
		}

		[Fact]
		public void Uninitialized_MultiCell_NoEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var p = default(SecureMatrix4x4);
			Assert.False(p.TryDecrypt(out Matrix4x4 value));
			Assert.Equal(default, value);
			Assert.Equal(baseline, fired);
			Assert.Equal(default, p.Decrypted);
		}

		[Fact]
		public void Uninitialized_String_NoEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var p = default(SecureString);
			Assert.False(p.TryDecrypt(out string value));
			Assert.Null(value);
			Assert.Equal(baseline, fired);
			Assert.Null(p.Decrypted);
		}

		[Fact]
		public void Uninitialized_BigInteger_NoEvent()
		{
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			int baseline = fired;

			var p = default(SecureBigInteger);
			Assert.False(p.TryDecrypt(out BigInteger value));
			Assert.Equal(default, value);
			Assert.Equal(baseline, fired);
			Assert.Equal(default, p.Decrypted);
		}

		// ---------- tampered: false, default out, exactly ONE event ----------

		[Fact]
		public void Tampered_Cell_OneEvent()
		{
			SecureInt p = Corrupt(new SecureInt(42));
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out int value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
			// The throwing read is unchanged: still fail-closed after a try-read.
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
			Assert.Equal(1, fired);
		}

		[Fact]
		public void Tampered_SingleCellBlock_OneEvent()
		{
			SecureDateTime p = Corrupt(
				new SecureDateTime(new DateTime(638000000000000000L, DateTimeKind.Utc))
			);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out DateTime value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
		}

		[Fact]
		public void Tampered_Cell128_OneEvent()
		{
			SecureGuid p = Corrupt(new SecureGuid(Guid.NewGuid()));
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out Guid value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
		}

		[Fact]
		public void Tampered_MultiCell_OneEvent()
		{
			// Fully corrupt ONE cell's both copies: the wrapper fails closed with
			// exactly one event (a damaged cell raises once; untouched cells stay silent).
			SecureMatrix4x4 p = BackupTests.CorruptOneCellBothCopies(
				new SecureMatrix4x4(Matrix4x4.Identity),
				"_cellA"
			);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out Matrix4x4 value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
		}

		[Fact]
		public void Tampered_String_OneEvent()
		{
			SecureString p = Corrupt(new SecureString("tamper me"));
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out string value));
			Assert.Null(value);
			Assert.Equal(1, fired);
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
		}

		[Fact]
		public void Tampered_BigInteger_OneEvent()
		{
			SecureBigInteger p = Corrupt(new SecureBigInteger(BigInteger.Pow(2, 128)));
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;

			Assert.False(p.TryDecrypt(out BigInteger value));
			Assert.Equal(default, value);
			Assert.Equal(1, fired);
			Assert.Throws<TamperedException>(() => _ = p.Decrypted);
		}

		// ---------- helpers ----------

		private static T Corrupt<T>(T wrapper)
			where T : struct
		{
			// Box so reflection mutations stick, flip every stored word/cipher,
			// then hand back a tampered (but well-formed) value.
			object boxed = wrapper;
			Type t = boxed.GetType();
			foreach (
				FieldInfo f in t.GetFields(
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
				)
			)
			{
				if (f.FieldType == typeof(ulong))
				{
					f.SetValue(boxed, (ulong)f.GetValue(boxed)! ^ 0xABCDEF0123456789UL);
				}
				else if (f.FieldType == typeof(uint))
				{
					f.SetValue(boxed, (uint)f.GetValue(boxed)! ^ 0xDEADBEEFu);
				}
				else if (f.FieldType == typeof(ulong[]))
				{
					var arr = (ulong[]?)f.GetValue(boxed);
					if (arr is { Length: > 0 })
					{
						arr[0] ^= 1UL;
					}
				}
				else if (
					f.FieldType.IsValueType
					&& (f.FieldType == typeof(Cell) || f.FieldType == typeof(Cell128))
				)
				{
					object nested = f.GetValue(boxed)!;
					foreach (
						FieldInfo nf in nested
							.GetType()
							.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
					)
					{
						if (nf.FieldType == typeof(ulong))
						{
							nf.SetValue(nested, (ulong)nf.GetValue(nested)! ^ 0x1122334455667788UL);
						}
						else if (nf.FieldType == typeof(uint))
						{
							nf.SetValue(nested, (uint)nf.GetValue(nested)! ^ 0xCAFEBABEu);
						}
					}
					f.SetValue(boxed, nested);
				}
			}
			return (T)boxed;
		}
	}
}
