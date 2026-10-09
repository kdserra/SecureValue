// Primitive interfaces, all safe-by-construction:
// - IEquatable<T>/IComparable<T> are implemented EXPLICITLY, so the members
//   never compete in overload resolution while consumers still get
//   interface dispatch.
// - IConvertible is explicit (16 members + GetTypeCode); cold interop path,
//   boxing there is BCL-parity (int does the same).
// - ISpanFormattable.TryFormat forwards to the decrypted value; emitted only
//   where the BCL type implements it on every TFM the wrapper exists on.
namespace SecureValue.CodeGen;

using System.Text;

internal static class Interfaces
{
	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	/// <summary>Explicit IEquatable{T} + IComparable{T} (ordering types only get the latter).</summary>
	public static void EmitEquatableComparable(
		StringBuilder sb,
		string secure,
		string primitive,
		bool comparable
	)
	{
		// Generated partials are nullable-oblivious (no '?' annotations):
		// oblivious signatures still satisfy the interfaces, and null
		// literals pass without warnings under <Nullable>disable</Nullable>.
		sb.Append(
			Doc($"Compares the decrypted value with a plain {primitive} value for equality.")
		);
		sb.Append(Inline);
		sb.Append($"\t\tbool IEquatable<{primitive}>.Equals({primitive} other) =>\n");
		if (primitive == "string")
		{
			sb.Append("\t\t\tstring.Equals(Decrypted, other, System.StringComparison.Ordinal);\n");
		}
		else
		{
			sb.Append("\t\t\tDecrypted.Equals(other);\n");
		}
		if (comparable)
		{
			sb.Append(Doc($"Compares the decrypted value with a plain {primitive} value."));
			sb.Append(Inline);
			sb.Append($"\t\tint IComparable<{primitive}>.CompareTo({primitive} other) =>\n");
			if (primitive == "string")
			{
				sb.Append("\t\t\t(Decrypted ?? string.Empty).CompareTo(other ?? string.Empty);\n");
			}
			else
			{
				sb.Append("\t\t\tDecrypted.CompareTo(other);\n");
			}
		}
	}

	/// <summary>Explicit IEquatable{T} only (non-orderable types: Complex, vectors, Unity).</summary>
	public static void EmitEquatableOnly(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(
			Doc($"Compares the decrypted value with a plain {primitive} value for equality.")
		);
		sb.Append(Inline);
		sb.Append($"\t\tbool IEquatable<{primitive}>.Equals({primitive} other) =>\n");
		sb.Append("\t\t\tDecrypted.Equals(other);\n");
	}

	/// <summary>Explicit IConvertible (cold interop path; mirrors the BCL type's own support).</summary>
	public static void EmitConvertible(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(Doc("Returns the type code of the wrapped primitive type."));
		sb.Append(Inline);
		if (primitive == "string")
		{
			sb.Append(
				"\t\tTypeCode IConvertible.GetTypeCode() => Convert.GetTypeCode(Decrypted ?? string.Empty);\n"
			);
		}
		else
		{
			sb.Append(
				"\t\tTypeCode IConvertible.GetTypeCode() => Convert.GetTypeCode(Decrypted);\n"
			);
		}
		foreach (
			string target in new[]
			{
				"bool",
				"byte",
				"sbyte",
				"short",
				"ushort",
				"int",
				"uint",
				"long",
				"ulong",
				"float",
				"double",
				"decimal",
				"DateTime",
				"char",
			}
		)
		{
			string method =
				target == "int" ? "Int32"
				: target == "short" ? "Int16"
				: target == "long" ? "Int64"
				: target == "uint" ? "UInt32"
				: target == "ushort" ? "UInt16"
				: target == "ulong" ? "UInt64"
				: target == "sbyte" ? "SByte"
				: target == "bool" ? "Boolean"
				: target == "float" ? "Single"
				: target == "char" ? "Char"
				: target == "decimal" ? "Decimal"
				: target == "double" ? "Double"
				: target == "DateTime" ? "DateTime"
				: "Byte";
			sb.Append(Doc($"Converts the decrypted value to {target} (explicit)."));
			sb.Append(Inline);
			sb.Append($"\t\t{target} IConvertible.To{method}(IFormatProvider provider) =>\n");
			if (primitive == "string")
			{
				sb.Append(
					$"\t\t\t((IConvertible)(Decrypted ?? string.Empty)).To{method}(provider);\n"
				);
			}
			else
			{
				sb.Append($"\t\t\t((IConvertible)Decrypted).To{method}(provider);\n");
			}
		}

		sb.Append(Doc("Converts the decrypted value to a string (explicit)."));
		sb.Append(Inline);
		sb.Append("\t\tstring IConvertible.ToString(IFormatProvider provider) =>\n");
		if (primitive == "string")
		{
			sb.Append("\t\t\t((IConvertible)(Decrypted ?? string.Empty)).ToString(provider);\n");
		}
		else
		{
			sb.Append("\t\t\t((IConvertible)Decrypted).ToString(provider);\n");
		}
		sb.Append(Doc("Converts the decrypted value to the specified type (explicit)."));
		sb.Append(Inline);
		sb.Append(
			"\t\tobject IConvertible.ToType(Type conversionType, IFormatProvider provider) =>\n"
		);
		if (primitive == "string")
		{
			sb.Append(
				"\t\t\t((IConvertible)(Decrypted ?? string.Empty)).ToType(conversionType, provider);\n"
			);
		}
		else
		{
			sb.Append("\t\t\t((IConvertible)Decrypted).ToType(conversionType, provider);\n");
		}
	}

	/// <summary>ISpanFormattable.TryFormat forwarded to the decrypted value.</summary>
	public static void EmitSpanFormattable(
		StringBuilder sb,
		string secure,
		string primitive,
		bool forwardProvider = true
	)
	{
		sb.Append(Doc("Tries to format the decrypted value into the destination span."));
		sb.Append(Inline);
		sb.Append(
			"\t\tpublic bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider) =>\n"
		);
		sb.Append(
			forwardProvider
				? "\t\t\tDecrypted.TryFormat(destination, out charsWritten, format, provider);\n"
				: "\t\t\tDecrypted.TryFormat(destination, out charsWritten, format);\n"
		);
	}

	/// <summary>
	/// Interface list for the generated partial declaration, ordinal-sorted so
	/// related interfaces group together (all IComparable…, then IConvertible,
	/// then all IEquatable…; ISpanFormattable trails last via GenWriter).
	/// </summary>
	public static string[] InterfacesFor(WrapperSpec spec)
	{
		List<string> list = new();
		// First-party contract: every wrapper exposes IsUnset/Decrypted/
		// TryDecrypt through ISecureValue{T}, satisfied implicitly by the
		// existing public members (no emitted members, no boxing on direct
		// or constrained-generic calls). The primitive name resolves via the
		// per-family usings GenWriter already emits.
		list.Add($"ISecureValue<{spec.Primitive}>");
		// Generator-owned: Secure-typed equality/comparison, moved from the
		// hand-written base lists (members stay hand-written; either partial
		// may declare the interface). Ordering mirrors EmitFor's split.
		switch (spec.Kind)
		{
			case WrapperKind.Integral:
			case WrapperKind.Float:
			case WrapperKind.Char:
			case WrapperKind.Bool:
			case WrapperKind.String:
			case WrapperKind.DateTime:
			case WrapperKind.Guid:
			case WrapperKind.Rune:
			case WrapperKind.BigInteger:
				list.Add($"IEquatable<{spec.SecureName}>");
				list.Add($"IComparable<{spec.SecureName}>");
				list.Add("IComparable");
				break;
			case WrapperKind.Complex:
			case WrapperKind.Numerics:
			case WrapperKind.Unity:
				list.Add($"IEquatable<{spec.SecureName}>");
				break;
		}

		switch (spec.Kind)
		{
			case WrapperKind.Integral:
			case WrapperKind.Float:
			case WrapperKind.Char:
			case WrapperKind.Bool:
			case WrapperKind.String:
			case WrapperKind.DateTime:
			case WrapperKind.Guid:
			case WrapperKind.Rune:
			case WrapperKind.BigInteger:
				list.Add($"IEquatable<{spec.Primitive}>");
				list.Add($"IComparable<{spec.Primitive}>");
				break;
			case WrapperKind.Complex:
			case WrapperKind.Numerics:
			case WrapperKind.Unity:
				list.Add($"IEquatable<{spec.Primitive}>");
				break;
		}

		if (
			spec.Kind
				is WrapperKind.Integral
					or WrapperKind.Float
					or WrapperKind.Char
					or WrapperKind.Bool
					or WrapperKind.String
			|| spec.SecureName is "SecureDateTime"
		)
		{
			list.Add("IConvertible");
		}

		if (spec.SecureName is "SecureString")
		{
			list.Add("IEnumerable<char>");
		}

		if (
			spec.Kind
				is WrapperKind.Integral
					or WrapperKind.Float
					or WrapperKind.Char
					or WrapperKind.DateTime
					or WrapperKind.Guid
					or WrapperKind.BigInteger
					or WrapperKind.Complex
			|| spec.SecureName is "SecureVector2" or "SecureVector3" or "SecureVector4"
		)
		{
			list.Add("IFormattable");
		}

		list.Sort(StringComparer.Ordinal);
		return list.ToArray();
	}

	/// <summary>Whether the generated partial carries a NET7-guarded IFormattable (Rune only).</summary>
	public static bool WantsConditionalFormattable(WrapperSpec spec) =>
		spec.SecureName is "SecureRune";

	/// <summary>
	/// Parse interfaces (#if NET7_0_OR_GREATER in GenWriter). Every listed underlying type implements both.
	/// IParsable&lt;T&gt; (not T-plain: same signature modulo return would
	/// collide) — only the Self variants exist, by construction.
	/// </summary>
	public static string[] ParsableFor(WrapperSpec spec) =>
		spec.Kind
			is WrapperKind.Integral
				or WrapperKind.Float
				or WrapperKind.Char
				or WrapperKind.Bool
				or WrapperKind.String
				or WrapperKind.DateTime
				or WrapperKind.Guid
				or WrapperKind.BigInteger
				or WrapperKind.Complex
			? new[] { $"IParsable<{spec.SecureName}>", $"ISpanParsable<{spec.SecureName}>" }
			: Array.Empty<string>();

	/// <summary>
	/// UTF-8 interfaces (#if NET8_0_OR_GREATER in GenWriter). Bool/string have neither; dates format only; Rune parses/formats but
	/// has no string/span Parse surface at all.
	/// </summary>
	public static string[] Utf8For(WrapperSpec spec)
	{
		List<string> list = new();
		bool formats =
			spec.Kind
				is WrapperKind.Integral
					or WrapperKind.Float
					or WrapperKind.Char
					or WrapperKind.DateTime
					or WrapperKind.Guid
					or WrapperKind.BigInteger
					or WrapperKind.Complex
			|| spec.SecureName is "SecureRune";
		bool parses =
			spec.Kind
				is WrapperKind.Integral
					or WrapperKind.Float
					or WrapperKind.Char
					or WrapperKind.Guid
					or WrapperKind.BigInteger
					or WrapperKind.Complex
			|| spec.SecureName is "SecureRune";
		if (formats)
		{
			list.Add("IUtf8SpanFormattable");
		}

		if (parses)
		{
			list.Add($"IUtf8SpanParsable<{spec.SecureName}>");
		}

		return list.ToArray();
	}

	/// <summary>Emits interface members matching <see cref="InterfacesFor"/> (single gating point).</summary>
	public static void EmitFor(StringBuilder sb, WrapperSpec spec)
	{
		switch (spec.Kind)
		{
			case WrapperKind.Integral:
			case WrapperKind.Float:
			case WrapperKind.Char:
			case WrapperKind.Bool:
			case WrapperKind.String:
			case WrapperKind.DateTime:
			case WrapperKind.Guid:
			case WrapperKind.Rune:
			case WrapperKind.BigInteger:
				EmitEquatableComparable(sb, spec.SecureName, spec.Primitive, comparable: true);
				break;
			case WrapperKind.Complex:
			case WrapperKind.Numerics:
			case WrapperKind.Unity:
				EmitEquatableOnly(sb, spec.SecureName, spec.Primitive);
				break;
		}

		// TimeSpan's IConvertible is absent from netstandard2.1 and converts
		// nothing usefully at runtime anywhere (ToInt32 etc. throw), so only
		// SecureDateTime carries IConvertible among the date kinds.
		if (
			spec.Kind
				is WrapperKind.Integral
					or WrapperKind.Float
					or WrapperKind.Char
					or WrapperKind.Bool
					or WrapperKind.String
			|| spec.SecureName is "SecureDateTime"
		)
		{
			EmitConvertible(sb, spec.SecureName, spec.Primitive);
		}

		if (
			spec.Kind is WrapperKind.Integral or WrapperKind.Float or WrapperKind.Guid
			|| spec.SecureName
				is "SecureDateTime"
					or "SecureTimeSpan"
					or "SecureDateOnly"
					or "SecureTimeOnly"
					or "SecureComplex"
					or "SecureBigInteger"
		)
		{
			sb.Append($"#if {SpanFormatGuard(spec)}\n");
			EmitSpanFormattable(
				sb,
				spec.SecureName,
				spec.Primitive,
				spec.SecureName != "SecureGuid"
			);
			sb.Append("#endif\n");
		}

		// Char/Rune/DateTimeOffset forward through the interface: BCL may
		// implement TryFormat explicitly, and a direct call would not bind.
		// Char also gains IFormattable itself here (BCL char has no format
		// overloads to forward, so there is no hand member to mirror).
		if (spec.SecureName is "SecureChar")
		{
			sb.Append(
				Doc(
					"Formats the decrypted value (explicit IFormattable; format is ignored for char)."
				)
			);
			sb.Append(Inline);
			sb.Append(
				"\t\tstring IFormattable.ToString(string format, IFormatProvider provider) => Decrypted.ToString();\n"
			);
		}
		if (spec.SecureName is "SecureChar" or "SecureRune" or "SecureDateTimeOffset")
		{
			sb.Append($"#if {SpanFormatGuard(spec)}\n");
			sb.Append(Doc("Tries to format the decrypted value into the destination span."));
			sb.Append(Inline);
			sb.Append(
				"\t\tpublic bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider) =>\n"
			);
			sb.Append(
				"\t\t\t((System.ISpanFormattable)Decrypted).TryFormat(destination, out charsWritten, format, provider);\n"
			);
			sb.Append("#endif\n");
		}
	}

	/// <summary>Whether the generated partial carries ISpanFormattable (net6+ only).</summary>
	public static bool WantsSpanFormat(WrapperSpec spec) =>
		spec.Kind is WrapperKind.Integral or WrapperKind.Float or WrapperKind.Guid
		|| spec.SecureName
			is "SecureDateTime"
				or "SecureTimeSpan"
				or "SecureDateOnly"
				or "SecureTimeOnly"
				or "SecureComplex"
				or "SecureBigInteger"
				or "SecureChar"
				or "SecureRune"
				or "SecureDateTimeOffset";

	/// <summary>
	/// TFM guard for ISpanFormattable. Complex grew its public TryFormat in
	/// .NET 7 and Rune carries its IFormattable at NET7 (build-oracle
	/// verified); everything else carries it from .NET 6.
	/// </summary>
	public static string SpanFormatGuard(WrapperSpec spec) =>
		spec.SecureName is "SecureComplex" or "SecureRune"
			? "NET7_0_OR_GREATER"
			: "NET6_0_OR_GREATER";
}
