// Central wrapper inventory for SecureValue.CodeGen.
// One row per hand-written wrapper. The generator emits a sibling
// <c>SecureX.g.cs</c> partial (operators, conversions, interface boilerplate)
// next to each hand file.
namespace SecureValue.CodeGen;

/// <summary>Boilerplate category driving which emitters run for a wrapper.</summary>
internal enum WrapperKind
{
	/// <summary>8 integrals: full operator matrix, bitwise, shifts, conversions.</summary>
	Integral,

	/// <summary>float, double, decimal: arithmetic + comparison twins.</summary>
	Float,

	/// <summary>char: BCL char operator subset.</summary>
	Char,

	/// <summary>bool: equality + ordering only.</summary>
	Bool,

	/// <summary>string: concatenation + equality + ordering.</summary>
	String,

	/// <summary>DateTime, DateTimeOffset, TimeSpan, DateOnly, TimeOnly.</summary>
	DateTime,

	/// <summary>Guid.</summary>
	Guid,

	/// <summary>System.Text.Rune.</summary>
	Rune,

	/// <summary>BigInteger: full arithmetic, comparison-only twins.</summary>
	BigInteger,

	/// <summary>Complex: arithmetic on (C, double) pairs.</summary>
	Complex,

	/// <summary>System.Numerics vectors/matrices/quaternion/plane.</summary>
	Numerics,

	/// <summary>Unity engine value types (whole-file UNITY guarded).</summary>
	Unity,
}

/// <summary>One hand-written wrapper and where its generated partial lives.</summary>
internal sealed record WrapperSpec(
	string SecureName,
	string Primitive,
	string Namespace,
	string RelativeDir,
	WrapperKind Kind
)
{
	/// <summary>File name of the generated partial, e.g. SecureInt.g.cs.</summary>
	public string GeneratedFileName => SecureName + ".g.cs";
}

internal static class Spec
{
	/// <summary>All 46 wrappers. Order is emission order (stable output).</summary>
	public static readonly WrapperSpec[] All =
	{
		// Core integrals.
		new("SecureSByte", "sbyte", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureByte", "byte", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureShort", "short", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureUShort", "ushort", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureInt", "int", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureUInt", "uint", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureLong", "long", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		new("SecureULong", "ulong", "SecureValue", @"Wrappers\Core", WrapperKind.Integral),
		// Core floats.
		new("SecureFloat", "float", "SecureValue", @"Wrappers\Core", WrapperKind.Float),
		new("SecureDouble", "double", "SecureValue", @"Wrappers\Core", WrapperKind.Float),
		new("SecureDecimal", "decimal", "SecureValue", @"Wrappers\Core", WrapperKind.Float),
		// Core misc primitives.
		new("SecureChar", "char", "SecureValue", @"Wrappers\Core", WrapperKind.Char),
		new("SecureBool", "bool", "SecureValue", @"Wrappers\Core", WrapperKind.Bool),
		new("SecureString", "string", "SecureValue", @"Wrappers\Core", WrapperKind.String),
		// Core dates / times.
		new("SecureDateTime", "DateTime", "SecureValue", @"Wrappers\Core", WrapperKind.DateTime),
		new(
			"SecureDateTimeOffset",
			"DateTimeOffset",
			"SecureValue",
			@"Wrappers\Core",
			WrapperKind.DateTime
		),
		new("SecureDateOnly", "DateOnly", "SecureValue", @"Wrappers\Core", WrapperKind.DateTime),
		new("SecureTimeOnly", "TimeOnly", "SecureValue", @"Wrappers\Core", WrapperKind.DateTime),
		new("SecureTimeSpan", "TimeSpan", "SecureValue", @"Wrappers\Core", WrapperKind.DateTime),
		// Core misc structs.
		new("SecureGuid", "Guid", "SecureValue", @"Wrappers\Core", WrapperKind.Guid),
		new("SecureRune", "Rune", "SecureValue", @"Wrappers\Core", WrapperKind.Rune),
		// Numerics (namespace SecureValue.Numerics).
		new(
			"SecureBigInteger",
			"BigInteger",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.BigInteger
		),
		new(
			"SecureComplex",
			"Complex",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Complex
		),
		new(
			"SecureMatrix3x2",
			"Matrix3x2",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecureMatrix4x4",
			"Matrix4x4",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecurePlane",
			"Plane",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecureQuaternion",
			"Quaternion",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecureVector2",
			"Vector2",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecureVector3",
			"Vector3",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		new(
			"SecureVector4",
			"Vector4",
			"SecureValue.Numerics",
			@"Wrappers\Numerics",
			WrapperKind.Numerics
		),
		// Unity engine types (namespace SecureValue.Unity).
		new("SecureVector2", "Vector2", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new(
			"SecureVector2Int",
			"Vector2Int",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
		new("SecureVector3", "Vector3", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new(
			"SecureVector3Int",
			"Vector3Int",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
		new("SecureVector4", "Vector4", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new("SecureRect", "Rect", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new("SecureRectInt", "RectInt", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new("SecureBounds", "Bounds", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new(
			"SecureBoundsInt",
			"BoundsInt",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
		new("SecureColor", "Color", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new("SecureColor32", "Color32", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new(
			"SecureQuaternion",
			"Quaternion",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
		new(
			"SecureMatrix4x4",
			"Matrix4x4",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
		new("SecurePlane", "Plane", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new("SecureRay", "Ray", "SecureValue.Unity", @"Wrappers\Unity", WrapperKind.Unity),
		new(
			"SecureLayerMask",
			"LayerMask",
			"SecureValue.Unity",
			@"Wrappers\Unity",
			WrapperKind.Unity
		),
	};
}
