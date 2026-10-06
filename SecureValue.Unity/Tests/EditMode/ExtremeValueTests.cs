#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode lowest/highest/zero round-trip coverage for every wrapper type Unity can
	/// serialize (core, .NET numerics and Unity families). A decrypt failure that resolves
	/// to 0 is invisible against a 0 constant, so the demo deliberately avoids these ranges
	/// and this suite covers them explicitly instead. Bit-exactness is asserted throughout.
	/// Unity-family and Numerics-family types share simple names, so both are FULLY
	/// QUALIFIED below — never rely on usings here. No dynamic: Unity scripting
	/// (IL2CPP/AOT and test assemblies) cannot rely on the DLR, so every conversion
	/// is statically typed through the wrappers' implicit operators.
	/// </summary>
	public class ExtremeValueTests
	{
		[TestCase(false)]
		[TestCase(true)]
		public void Bool_Extremes_RoundTrip(bool value)
		{
			SecureBool p = value;
			Assert.AreEqual(value, (bool)p);
		}

		[TestCase(byte.MinValue)]
		[TestCase(byte.MaxValue)]
		public void Byte_Extremes_RoundTrip(byte value)
		{
			SecureByte p = value;
			Assert.AreEqual(value, (byte)p);
		}

		[TestCase(sbyte.MinValue)]
		[TestCase((sbyte)0)]
		[TestCase(sbyte.MaxValue)]
		public void SByte_Extremes_RoundTrip(sbyte value)
		{
			SecureSByte p = value;
			Assert.AreEqual(value, (sbyte)p);
		}

		[TestCase(char.MinValue)]
		[TestCase(char.MaxValue)]
		public void Char_Extremes_RoundTrip(char value)
		{
			SecureChar p = value;
			Assert.AreEqual(value, (char)p);
		}

		[TestCase(short.MinValue)]
		[TestCase((short)0)]
		[TestCase(short.MaxValue)]
		public void Short_Extremes_RoundTrip(short value)
		{
			SecureShort p = value;
			Assert.AreEqual(value, (short)p);
		}

		[TestCase(ushort.MinValue)]
		[TestCase(ushort.MaxValue)]
		public void UShort_Extremes_RoundTrip(ushort value)
		{
			SecureUShort p = value;
			Assert.AreEqual(value, (ushort)p);
		}

		[TestCase(int.MinValue)]
		[TestCase(0)]
		[TestCase(int.MaxValue)]
		public void Int_Extremes_RoundTrip(int value)
		{
			SecureInt p = value;
			Assert.AreEqual(value, (int)p);
		}

		[TestCase(uint.MinValue)]
		[TestCase(uint.MaxValue)]
		public void UInt_Extremes_RoundTrip(uint value)
		{
			SecureUInt p = value;
			Assert.AreEqual(value, (uint)p);
		}

		[TestCase(long.MinValue)]
		[TestCase(0L)]
		[TestCase(long.MaxValue)]
		public void Long_Extremes_RoundTrip(long value)
		{
			SecureLong p = value;
			Assert.AreEqual(value, (long)p);
		}

		[TestCase(ulong.MinValue)]
		[TestCase(ulong.MaxValue)]
		public void ULong_Extremes_RoundTrip(ulong value)
		{
			SecureULong p = value;
			Assert.AreEqual(value, (ulong)p);
		}

		[TestCase(float.MinValue)]
		[TestCase(0.0f)]
		[TestCase(float.MaxValue)]
		[TestCase(float.PositiveInfinity)]
		[TestCase(float.NegativeInfinity)]
		public void Float_Extremes_RoundTrip(float value)
		{
			SecureFloat p = value;
			Assert.AreEqual(
				BitConverter.SingleToInt32Bits(value),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[Test]
		public void Float_NegativeZero_Extremes_RoundTrip()
		{
			// -0.0f == 0.0f by value but differs in the sign bit: assert bitwise.
			SecureFloat p = -0.0f;
			Assert.AreEqual(
				BitConverter.SingleToInt32Bits(-0.0f),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[Test]
		public void Float_NaN_Extremes_RoundTrip()
		{
			SecureFloat p = float.NaN;
			Assert.AreEqual(
				BitConverter.SingleToInt32Bits(float.NaN),
				BitConverter.SingleToInt32Bits((float)p)
			);
		}

		[TestCase(double.MinValue)]
		[TestCase(0.0)]
		[TestCase(double.MaxValue)]
		[TestCase(double.PositiveInfinity)]
		[TestCase(double.NegativeInfinity)]
		public void Double_Extremes_RoundTrip(double value)
		{
			SecureDouble p = value;
			Assert.AreEqual(
				BitConverter.DoubleToInt64Bits(value),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Test]
		public void Double_NegativeZero_Extremes_RoundTrip()
		{
			// -0.0 == 0.0 by value but differs in the sign bit: assert bitwise.
			SecureDouble p = -0.0;
			Assert.AreEqual(
				BitConverter.DoubleToInt64Bits(-0.0),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Test]
		public void Double_NaN_Extremes_RoundTrip()
		{
			SecureDouble p = double.NaN;
			Assert.AreEqual(
				BitConverter.DoubleToInt64Bits(double.NaN),
				BitConverter.DoubleToInt64Bits((double)p)
			);
		}

		[Test]
		public void Decimal_Extremes_RoundTrip()
		{
			SecureDecimal lo = decimal.MinValue;
			Assert.AreEqual(decimal.MinValue, (decimal)lo);
			SecureDecimal zero = decimal.Zero;
			Assert.AreEqual(decimal.Zero, (decimal)zero);
			SecureDecimal hi = decimal.MaxValue;
			Assert.AreEqual(decimal.MaxValue, (decimal)hi);
		}

		[Test]
		public void String_Extremes_RoundTrip()
		{
			SecureString empty = "";
			Assert.AreEqual("", (string)empty);
			SecureString nil = new SecureString(null!);
			Assert.AreEqual("", (string)nil);
			string longValue = new string('z', 1000);
			SecureString p = longValue;
			Assert.AreEqual(longValue, (string)p);
		}

		[Test]
		public void Guid_Extremes_RoundTrip()
		{
			SecureGuid empty = Guid.Empty;
			Assert.AreEqual(Guid.Empty, (Guid)empty);
			var max = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
			SecureGuid hi = max;
			Assert.AreEqual(max, (Guid)hi);
		}

		[Test]
		public void DateTime_Extremes_RoundTrip()
		{
			SecureDateTime lo = DateTime.MinValue;
			Assert.AreEqual(DateTime.MinValue, (DateTime)lo);
			SecureDateTime hi = DateTime.MaxValue;
			Assert.AreEqual(DateTime.MaxValue, (DateTime)hi);
		}

		[Test]
		public void DateTimeOffset_Extremes_RoundTrip()
		{
			SecureDateTimeOffset lo = DateTimeOffset.MinValue;
			Assert.AreEqual(DateTimeOffset.MinValue, (DateTimeOffset)lo);
			SecureDateTimeOffset hi = DateTimeOffset.MaxValue;
			Assert.AreEqual(DateTimeOffset.MaxValue, (DateTimeOffset)hi);
		}

		[Test]
		public void TimeSpan_Extremes_RoundTrip()
		{
			SecureTimeSpan zero = TimeSpan.Zero;
			Assert.AreEqual(TimeSpan.Zero, (TimeSpan)zero);
			SecureTimeSpan lo = TimeSpan.MinValue;
			Assert.AreEqual(TimeSpan.MinValue, (TimeSpan)lo);
			SecureTimeSpan hi = TimeSpan.MaxValue;
			Assert.AreEqual(TimeSpan.MaxValue, (TimeSpan)hi);
		}

		[Test]
		public void Vector2_Extremes_RoundTrip()
		{
			SecureValue.Numerics.SecureVector2 zero = System.Numerics.Vector2.Zero;
			Assert.AreEqual(System.Numerics.Vector2.Zero, (System.Numerics.Vector2)zero);
			var loValue = new System.Numerics.Vector2(float.MinValue, float.MinValue);
			SecureValue.Numerics.SecureVector2 lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Vector2)lo);
			var hiValue = new System.Numerics.Vector2(float.MaxValue, float.MaxValue);
			SecureValue.Numerics.SecureVector2 hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Vector2)hi);
		}

		[Test]
		public void Vector3_Extremes_RoundTrip()
		{
			SecureValue.Numerics.SecureVector3 zero = System.Numerics.Vector3.Zero;
			Assert.AreEqual(System.Numerics.Vector3.Zero, (System.Numerics.Vector3)zero);
			var loValue = new System.Numerics.Vector3(
				float.MinValue,
				float.MinValue,
				float.MinValue
			);
			SecureValue.Numerics.SecureVector3 lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Vector3)lo);
			var hiValue = new System.Numerics.Vector3(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			SecureValue.Numerics.SecureVector3 hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Vector3)hi);
		}

		[Test]
		public void Vector4_Extremes_RoundTrip()
		{
			SecureValue.Numerics.SecureVector4 zero = System.Numerics.Vector4.Zero;
			Assert.AreEqual(System.Numerics.Vector4.Zero, (System.Numerics.Vector4)zero);
			var loValue = new System.Numerics.Vector4(
				float.MinValue,
				float.MinValue,
				float.MinValue,
				float.MinValue
			);
			SecureValue.Numerics.SecureVector4 lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Vector4)lo);
			var hiValue = new System.Numerics.Vector4(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			SecureValue.Numerics.SecureVector4 hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Vector4)hi);
		}

		[Test]
		public void Quaternion_Extremes_RoundTrip()
		{
			var zeroValue = new System.Numerics.Quaternion(0f, 0f, 0f, 0f);
			SecureValue.Numerics.SecureQuaternion zero = zeroValue;
			Assert.AreEqual(zeroValue, (System.Numerics.Quaternion)zero);
			var loValue = new System.Numerics.Quaternion(
				float.MinValue,
				float.MinValue,
				float.MinValue,
				float.MinValue
			);
			SecureValue.Numerics.SecureQuaternion lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Quaternion)lo);
			var hiValue = new System.Numerics.Quaternion(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			SecureValue.Numerics.SecureQuaternion hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Quaternion)hi);
		}

		[Test]
		public void Plane_Extremes_RoundTrip()
		{
			var zeroValue = new System.Numerics.Plane(System.Numerics.Vector3.Zero, 0f);
			SecureValue.Numerics.SecurePlane zero = zeroValue;
			Assert.AreEqual(zeroValue, (System.Numerics.Plane)zero);
			var loValue = new System.Numerics.Plane(
				new System.Numerics.Vector3(float.MinValue, float.MinValue, float.MinValue),
				float.MinValue
			);
			SecureValue.Numerics.SecurePlane lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Plane)lo);
			var hiValue = new System.Numerics.Plane(
				new System.Numerics.Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				float.MaxValue
			);
			SecureValue.Numerics.SecurePlane hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Plane)hi);
		}

		[Test]
		public void Matrix3x2_Extremes_RoundTrip()
		{
			var zeroValue = default(System.Numerics.Matrix3x2);
			SecureValue.Numerics.SecureMatrix3x2 zero = zeroValue;
			Assert.AreEqual(zeroValue, (System.Numerics.Matrix3x2)zero);
			System.Numerics.Matrix3x2 loValue = Fill3x2(float.MinValue);
			SecureValue.Numerics.SecureMatrix3x2 lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Matrix3x2)lo);
			System.Numerics.Matrix3x2 hiValue = Fill3x2(float.MaxValue);
			SecureValue.Numerics.SecureMatrix3x2 hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Matrix3x2)hi);
		}

		[Test]
		public void Matrix4x4_Extremes_RoundTrip()
		{
			var zeroValue = default(System.Numerics.Matrix4x4);
			SecureValue.Numerics.SecureMatrix4x4 zero = zeroValue;
			Assert.AreEqual(zeroValue, (System.Numerics.Matrix4x4)zero);
			System.Numerics.Matrix4x4 loValue = Fill4x4(float.MinValue);
			SecureValue.Numerics.SecureMatrix4x4 lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Matrix4x4)lo);
			System.Numerics.Matrix4x4 hiValue = Fill4x4(float.MaxValue);
			SecureValue.Numerics.SecureMatrix4x4 hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Matrix4x4)hi);
		}

		[Test]
		public void Complex_Extremes_RoundTrip()
		{
			SecureValue.Numerics.SecureComplex zero = System.Numerics.Complex.Zero;
			Assert.AreEqual(System.Numerics.Complex.Zero, (System.Numerics.Complex)zero);
			var hiValue = new System.Numerics.Complex(double.MaxValue, double.MaxValue);
			SecureValue.Numerics.SecureComplex hi = hiValue;
			Assert.AreEqual(hiValue, (System.Numerics.Complex)hi);
			var loValue = new System.Numerics.Complex(double.MinValue, double.MinValue);
			SecureValue.Numerics.SecureComplex lo = loValue;
			Assert.AreEqual(loValue, (System.Numerics.Complex)lo);
		}

		[Test]
		public void BigInteger_Extremes_RoundTrip()
		{
			SecureValue.Numerics.SecureBigInteger zero = System.Numerics.BigInteger.Zero;
			Assert.AreEqual(System.Numerics.BigInteger.Zero, (System.Numerics.BigInteger)zero);
			System.Numerics.BigInteger huge = System.Numerics.BigInteger.Pow(2, 256);
			SecureValue.Numerics.SecureBigInteger hi = huge;
			Assert.AreEqual(huge, (System.Numerics.BigInteger)hi);
			SecureValue.Numerics.SecureBigInteger lo = -huge;
			Assert.AreEqual(-huge, (System.Numerics.BigInteger)lo);
		}

		[Test]
		public void UnityVector2_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureVector2 zero = Vector2.zero;
			Assert.AreEqual(Vector2.zero, (Vector2)zero);
			var loValue = new Vector2(float.MinValue, float.MinValue);
			SecureValue.Unity.SecureVector2 lo = loValue;
			Assert.AreEqual(loValue, (Vector2)lo);
			var hiValue = new Vector2(float.MaxValue, float.MaxValue);
			SecureValue.Unity.SecureVector2 hi = hiValue;
			Assert.AreEqual(hiValue, (Vector2)hi);
		}

		[Test]
		public void UnityVector2Int_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureVector2Int zero = Vector2Int.zero;
			Assert.AreEqual(Vector2Int.zero, (Vector2Int)zero);
			var loValue = new Vector2Int(int.MinValue, int.MinValue);
			SecureValue.Unity.SecureVector2Int lo = loValue;
			Assert.AreEqual(loValue, (Vector2Int)lo);
			var hiValue = new Vector2Int(int.MaxValue, int.MaxValue);
			SecureValue.Unity.SecureVector2Int hi = hiValue;
			Assert.AreEqual(hiValue, (Vector2Int)hi);
		}

		[Test]
		public void UnityVector3_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureVector3 zero = Vector3.zero;
			Assert.AreEqual(Vector3.zero, (Vector3)zero);
			var loValue = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			SecureValue.Unity.SecureVector3 lo = loValue;
			Assert.AreEqual(loValue, (Vector3)lo);
			var hiValue = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			SecureValue.Unity.SecureVector3 hi = hiValue;
			Assert.AreEqual(hiValue, (Vector3)hi);
		}

		[Test]
		public void UnityVector3Int_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureVector3Int zero = Vector3Int.zero;
			Assert.AreEqual(Vector3Int.zero, (Vector3Int)zero);
			var loValue = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
			SecureValue.Unity.SecureVector3Int lo = loValue;
			Assert.AreEqual(loValue, (Vector3Int)lo);
			var hiValue = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			SecureValue.Unity.SecureVector3Int hi = hiValue;
			Assert.AreEqual(hiValue, (Vector3Int)hi);
		}

		[Test]
		public void UnityVector4_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureVector4 zero = Vector4.zero;
			Assert.AreEqual(Vector4.zero, (Vector4)zero);
			var loValue = new Vector4(
				float.MinValue,
				float.MinValue,
				float.MinValue,
				float.MinValue
			);
			SecureValue.Unity.SecureVector4 lo = loValue;
			Assert.AreEqual(loValue, (Vector4)lo);
			var hiValue = new Vector4(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			SecureValue.Unity.SecureVector4 hi = hiValue;
			Assert.AreEqual(hiValue, (Vector4)hi);
		}

		[Test]
		public void UnityRect_Extremes_RoundTrip()
		{
			var zeroValue = new Rect(0f, 0f, 0f, 0f);
			SecureValue.Unity.SecureRect zero = zeroValue;
			Assert.AreEqual(zeroValue, (Rect)zero);
			var hiValue = new Rect(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
			SecureValue.Unity.SecureRect hi = hiValue;
			Assert.AreEqual(hiValue, (Rect)hi);
		}

		[Test]
		public void UnityRectInt_Extremes_RoundTrip()
		{
			var zeroValue = new RectInt(0, 0, 0, 0);
			SecureValue.Unity.SecureRectInt zero = zeroValue;
			Assert.AreEqual(zeroValue, (RectInt)zero);
			var hiValue = new RectInt(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
			SecureValue.Unity.SecureRectInt hi = hiValue;
			Assert.AreEqual(hiValue, (RectInt)hi);
		}

		[Test]
		public void UnityBounds_Extremes_RoundTrip()
		{
			var zeroValue = new Bounds(Vector3.zero, Vector3.zero);
			SecureValue.Unity.SecureBounds zero = zeroValue;
			Assert.AreEqual(zeroValue, (Bounds)zero);
			var hiValue = new Bounds(
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)
			);
			SecureValue.Unity.SecureBounds hi = hiValue;
			Assert.AreEqual(hiValue, (Bounds)hi);
		}

		[Test]
		public void UnityBoundsInt_Extremes_RoundTrip()
		{
			var zeroValue = new BoundsInt(Vector3Int.zero, Vector3Int.zero);
			SecureValue.Unity.SecureBoundsInt zero = zeroValue;
			Assert.AreEqual(zeroValue, (BoundsInt)zero);
			var hiValue = new BoundsInt(
				new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue),
				new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue)
			);
			SecureValue.Unity.SecureBoundsInt hi = hiValue;
			Assert.AreEqual(hiValue, (BoundsInt)hi);
			var loValue = new BoundsInt(
				new Vector3Int(int.MinValue, int.MinValue, int.MinValue),
				new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue)
			);
			SecureValue.Unity.SecureBoundsInt lo = loValue;
			Assert.AreEqual(loValue, (BoundsInt)lo);
		}

		[Test]
		public void UnityColor_Extremes_RoundTrip()
		{
			var zeroValue = new Color(0f, 0f, 0f, 0f);
			SecureValue.Unity.SecureColor zero = zeroValue;
			Assert.AreEqual(zeroValue, (Color)zero);
			SecureValue.Unity.SecureColor white = Color.white;
			Assert.AreEqual(Color.white, (Color)white);
		}

		[Test]
		public void UnityColor32_Extremes_RoundTrip()
		{
			var zeroValue = new Color32(0, 0, 0, 0);
			SecureValue.Unity.SecureColor32 zero = zeroValue;
			Assert.AreEqual(zeroValue, (Color32)zero);
			var hiValue = new Color32(255, 255, 255, 255);
			SecureValue.Unity.SecureColor32 hi = hiValue;
			Assert.AreEqual(hiValue, (Color32)hi);
		}

		[Test]
		public void UnityQuaternion_Extremes_RoundTrip()
		{
			var zeroValue = new Quaternion(0f, 0f, 0f, 0f);
			SecureValue.Unity.SecureQuaternion zero = zeroValue;
			Assert.AreEqual(zeroValue, (Quaternion)zero);
			SecureValue.Unity.SecureQuaternion identity = Quaternion.identity;
			Assert.AreEqual(Quaternion.identity, (Quaternion)identity);
			var hiValue = new Quaternion(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			SecureValue.Unity.SecureQuaternion hi = hiValue;
			Assert.AreEqual(hiValue, (Quaternion)hi);
		}

		[Test]
		public void UnityMatrix4x4_Extremes_RoundTrip()
		{
			SecureValue.Unity.SecureMatrix4x4 zero = Matrix4x4.zero;
			Assert.AreEqual(Matrix4x4.zero, (Matrix4x4)zero);
			Vector4 column = new Vector4(
				float.MaxValue,
				float.MaxValue,
				float.MaxValue,
				float.MaxValue
			);
			var hiValue = new Matrix4x4(column, column, column, column);
			SecureValue.Unity.SecureMatrix4x4 hi = hiValue;
			Assert.AreEqual(hiValue, (Matrix4x4)hi);
		}

		[Test]
		public void UnityPlane_Extremes_RoundTrip()
		{
			var zeroValue = new Plane(Vector3.zero, 0f);
			SecureValue.Unity.SecurePlane zero = zeroValue;
			Assert.AreEqual(zeroValue, (Plane)zero);
			var hiValue = new Plane(
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				float.MaxValue
			);
			SecureValue.Unity.SecurePlane hi = hiValue;
			Assert.AreEqual(hiValue, (Plane)hi);
		}

		[Test]
		public void UnityRay_Extremes_RoundTrip()
		{
			var zeroValue = new Ray(Vector3.zero, Vector3.zero);
			SecureValue.Unity.SecureRay zero = zeroValue;
			Assert.AreEqual(zeroValue, (Ray)zero);
			var hiValue = new Ray(
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
				new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)
			);
			SecureValue.Unity.SecureRay hi = hiValue;
			Assert.AreEqual(hiValue, (Ray)hi);
		}

		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(int.MaxValue)]
		public void UnityLayerMask_Extremes_RoundTrip(int maskValue)
		{
			SecureValue.Unity.SecureLayerMask p = new LayerMask() { value = maskValue };
			Assert.AreEqual(maskValue, ((LayerMask)p).value);
		}

		private static System.Numerics.Matrix3x2 Fill3x2(float v) =>
			new System.Numerics.Matrix3x2(v, v, v, v, v, v);

		private static System.Numerics.Matrix4x4 Fill4x4(float v) =>
			new System.Numerics.Matrix4x4(v, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v);
	}
}
#endif
