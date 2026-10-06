#nullable enable
using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// IsUnset: never-assigned reads true; any sealed value — including the
	/// wrapped type's own default and the empty string — reads false.
	/// </summary>
	public class IsUnsetTests
	{
		[Fact]
		public void DefaultStructs_AreDefault()
		{
			Assert.True(default(SecureInt).IsUnset);
			Assert.True(default(SecureBool).IsUnset);
			Assert.True(default(SecureGuid).IsUnset);
			Assert.True(default(SecureDecimal).IsUnset);
			Assert.True(default(SecureMatrix3x2).IsUnset);
			Assert.True(default(SecureMatrix4x4).IsUnset);
			Assert.True(default(SecureString).IsUnset);
			Assert.True(default(SecureBigInteger).IsUnset);
		}

		[Fact]
		public void SealedTypeDefaults_AreNotDefault()
		{
			Assert.False(new SecureInt(0).IsUnset);
			Assert.False(new SecureBool(false).IsUnset);
			Assert.False(new SecureGuid(Guid.Empty).IsUnset);
			Assert.False(new SecureDecimal(0m).IsUnset);
			Assert.False(new SecureString(string.Empty).IsUnset);
			Assert.False(new SecureBigInteger(BigInteger.Zero).IsUnset);
			Assert.False(new SecureMatrix3x2(new Matrix3x2()).IsUnset);
			Assert.False(new SecureMatrix4x4(new Matrix4x4()).IsUnset);
		}

		[Fact]
		public void SealedValues_AreNotDefault()
		{
			Assert.False(new SecureInt(851).IsUnset);
			Assert.False(new SecureString("hi").IsUnset);
			Assert.False(new SecureBigInteger(BigInteger.Pow(2, 256)).IsUnset);
		}

		[Fact]
		public void Assignment_ClearsDefault()
		{
			SecureInt p = default;
			Assert.True(p.IsUnset);
			p = 5;
			Assert.False(p.IsUnset);
		}
	}
}
