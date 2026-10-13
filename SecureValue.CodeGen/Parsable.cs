// Parse/format interface members with BCL-parity inherited types.
// IParsable<TSelf>/ISpanParsable<TSelf> (#if
// NET7_0_OR_GREATER), IUtf8SpanFormattable/IUtf8SpanParsable<TSelf> (#if
// NET8_0_OR_GREATER), IEnumerable<char> for SecureString (all TFMs).
// Only Self variants exist: IParsable<T-plain> would collide (same signature
// modulo return). All static-abstract implementations are explicit, so they
// never compete in overload resolution; bodies are plain C# 9 — only the
// `static X ISome<S>.Y` declaration lines need C# 11, and those live inside
// the TFM guards. Span/UTF-8 parse routes through the
// string forms. IUtf8SpanParsable<TSelf> is standalone (it does NOT
// derive from ISpanParsable<TSelf>), so Rune needs no string/span Parse.
namespace SecureValue.CodeGen;

using System.Text;

internal static class Parsable
{
	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	/// <summary>Emits parse/format/enumerable members matching InterfacesFor (single gating point).</summary>
	public static void EmitFor(StringBuilder sb, WrapperSpec spec)
	{
		switch (spec.Kind)
		{
			case WrapperKind.Integral:
			case WrapperKind.Float:
				EmitAllRoutes(
					sb,
					spec.SecureName,
					v => $"{spec.Primitive}.Parse({v}, provider)",
					v => $"{spec.Primitive}.TryParse({v}, provider, out {spec.Primitive} plain)",
					_ => "plain",
					utf8: true
				);
				EmitUtf8Format(sb);
				break;
			case WrapperKind.Char:
			case WrapperKind.Bool:
				EmitAllRoutes(
					sb,
					spec.SecureName,
					v => $"{spec.Primitive}.Parse({v})",
					v => $"{spec.Primitive}.TryParse({v}, out {spec.Primitive} plain)",
					_ => "plain",
					utf8: spec.Kind == WrapperKind.Char,
					directSpan: false,
					directUtf8: false
				);
				if (spec.Kind == WrapperKind.Char)
				{
					EmitUtf8Format(sb);
				}

				break;
			case WrapperKind.String:
				// All parse members live in the hand file (bespoke null→empty
				// semantics + span-native sealing); the generator keeps the
				// declarations (ParsableFor) and the enumerator only.
				EmitEnumerable(sb, spec.SecureName);
				break;
			case WrapperKind.DateTime:
				if (spec.Primitive == "TimeSpan")
				{
					EmitAllRoutes(
						sb,
						spec.SecureName,
						v => $"{spec.Primitive}.Parse({v}, provider)",
						v =>
							$"{spec.Primitive}.TryParse({v}, provider, out {spec.Primitive} plain)",
						_ => "plain",
						utf8: false
					);
				}
				else
				{
					EmitAllRoutes(
						sb,
						spec.SecureName,
						v => $"{spec.Primitive}.Parse({v}, provider)",
						v =>
							$"{spec.Primitive}.TryParse({v}, provider, System.Globalization.DateTimeStyles.None, out {spec.Primitive} plain)",
						_ => "plain",
						utf8: false
					);
				}

				EmitUtf8Format(sb);
				break;
			case WrapperKind.Guid:
				// BCL Guid exposes no public UTF-8 TryParse (explicit
				// interface only) — span is direct, UTF-8 routes via string.
				EmitAllRoutes(
					sb,
					spec.SecureName,
					v => $"Guid.Parse({v}, provider)",
					v => $"Guid.TryParse({v}, provider, out Guid plain)",
					_ => "plain",
					utf8: true,
					directSpan: true,
					directUtf8: false
				);
				EmitUtf8Format(sb);
				break;
			case WrapperKind.BigInteger:
				// Direct span, routed UTF-8 (BCL exposes no public UTF-8 TryParse).
				EmitAllRoutes(
					sb,
					spec.SecureName,
					v => $"BigInteger.Parse({v}, provider)",
					v =>
						$"BigInteger.TryParse({v}, System.Globalization.NumberStyles.Integer, provider, out BigInteger plain)",
					_ => "plain",
					utf8: true,
					directSpan: true,
					directUtf8: false
				);
				EmitUtf8Format(sb);
				break;
			case WrapperKind.Complex:
				// Direct span, routed UTF-8 (BCL exposes no public UTF-8 TryParse).
				EmitAllRoutes(
					sb,
					spec.SecureName,
					v => $"System.Numerics.Complex.Parse({v}, provider)",
					v =>
						$"System.Numerics.Complex.TryParse({v}, provider, out System.Numerics.Complex plain)",
					_ => "plain",
					utf8: true,
					directSpan: true,
					directUtf8: false
				);
				EmitUtf8Format(sb);
				break;
			case WrapperKind.Rune:
				EmitUtf8Rune(sb, spec.SecureName);
				EmitUtf8Format(sb);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Emits the string (NET7), span (NET7) and optional UTF-8 (NET8) Parse /
	/// TryParse pairs for one wrapper. Bodies are duplicated per input shape
	/// because static-abstract members cannot be invoked via the interface
	/// name — siblings cannot chain through IParsable{S}.
	/// tryTest null means infallible (SecureString). Span/UTF-8 Parse is
	/// try-or-throw; string Parse
	/// stays BCL-direct. directSpan/directUtf8 select zero-temp BCL span
	/// calls, otherwise the input routes through a temp string.
	/// </summary>
	private static void EmitAllRoutes(
		StringBuilder sb,
		string secure,
		Func<string, string> parseValue,
		Func<string, string?> tryTest,
		Func<string, string> tryResult,
		bool utf8,
		bool directSpan = true,
		bool directUtf8 = true,
		bool emitSpan = true
	)
	{
		EmitParsePair(
			sb,
			"NET7_0_OR_GREATER",
			secure,
			$"System.IParsable<{secure}>",
			"string",
			"s",
			parseValue("s"),
			tryTest("s"),
			tryResult("s")
		);
		if (emitSpan)
		{
			string spanInput = directSpan ? "s" : "s.ToString()";
			EmitParsePair(
				sb,
				"NET7_0_OR_GREATER",
				secure,
				$"System.ISpanParsable<{secure}>",
				"ReadOnlySpan<char>",
				spanInput,
				directSpan ? null : parseValue(spanInput),
				tryTest(spanInput),
				tryResult(spanInput)
			);
		}
		if (utf8)
		{
			string utf8Input = directUtf8 ? "s" : "System.Text.Encoding.UTF8.GetString(s)";
			EmitParsePair(
				sb,
				"NET8_0_OR_GREATER",
				secure,
				$"System.IUtf8SpanParsable<{secure}>",
				"ReadOnlySpan<byte>",
				utf8Input,
				directUtf8 ? null : parseValue(utf8Input),
				tryTest(utf8Input),
				tryResult(utf8Input)
			);
		}
	}

	private static void EmitParsePair(
		StringBuilder sb,
		string guard,
		string secure,
		string iface,
		string paramType,
		string paramName,
		string? parseValue,
		string? tryTest,
		string tryResult
	)
	{
		sb.Append($"#if {guard}\n");
		sb.Append(Doc($"Parses input into its secured form (explicit {secure} parsing)."));
		sb.Append(Inline);
		if (parseValue is not null)
		{
			sb.Append(
				$"\t\tstatic {secure} {iface}.Parse({paramType} s, IFormatProvider provider) =>\n"
			);
			sb.Append($"\t\t\tnew {secure}({parseValue});\n");
		}
		else
		{
			sb.Append(
				$"\t\tstatic {secure} {iface}.Parse({paramType} s, IFormatProvider provider)\n"
			);
			sb.Append("\t\t{\n");
			sb.Append($"\t\t\tif ({tryTest})\n");
			sb.Append("\t\t\t{\n");
			sb.Append($"\t\t\t\treturn new {secure}({tryResult});\n");
			sb.Append("\t\t\t}\n\n");
			sb.Append(
				"\t\t\tthrow new System.FormatException(\"Input was not in a correct format.\");\n"
			);
			sb.Append("\t\t}\n");
		}
		sb.Append(Doc($"Tries to parse input into its secured form (explicit {secure} parsing)."));
		sb.Append(Inline);
		sb.Append(
			$"\t\tstatic bool {iface}.TryParse({paramType} s, IFormatProvider provider, out {secure} result)\n"
		);
		if (tryTest is null)
		{
			sb.Append("\t\t{\n");
			sb.Append($"\t\t\tresult = new {secure}({tryResult});\n");
			sb.Append("\t\t\treturn true;\n");
			sb.Append("\t\t}\n");
		}
		else
		{
			sb.Append("\t\t{\n");
			sb.Append($"\t\t\tif ({tryTest})\n");
			sb.Append("\t\t\t{\n");
			sb.Append($"\t\t\t\tresult = new {secure}({tryResult});\n");
			sb.Append("\t\t\t\treturn true;\n");
			sb.Append("\t\t\t}\n\n");
			sb.Append("\t\t\tresult = default;\n");
			sb.Append("\t\t\treturn false;\n");
			sb.Append("\t\t}\n");
		}

		sb.Append("#endif\n");
	}

	/// <summary>Public IUtf8SpanFormattable.TryFormat, forwarded to the decrypted value.</summary>
	private static void EmitUtf8Format(StringBuilder sb)
	{
		sb.Append("#if NET8_0_OR_GREATER\n");
		sb.Append(Doc("Tries to format the decrypted value as UTF-8 bytes."));
		sb.Append(Inline);
		sb.Append(
			"\t\tpublic bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider) =>\n"
		);
		sb.Append(
			"\t\t\t((System.IUtf8SpanFormattable)Decrypted).TryFormat(utf8Destination, out bytesWritten, format, provider);\n"
		);
		sb.Append("#endif\n");
	}

	/// <summary>Rune UTF-8 forms via DecodeFromUtf8 (Rune has no string Parse surface).</summary>
	private static void EmitUtf8Rune(StringBuilder sb, string secure)
	{
		sb.Append("#if NET8_0_OR_GREATER\n");
		sb.Append(Doc($"Parses UTF-8 bytes holding exactly one rune into its secured form."));
		sb.Append(Inline);
		sb.Append(
			$"\t\tstatic {secure} System.IUtf8SpanParsable<{secure}>.Parse(ReadOnlySpan<byte> s, IFormatProvider provider)\n"
		);
		sb.Append("\t\t{\n");
		sb.Append(
			"\t\t\tif (System.Text.Rune.DecodeFromUtf8(s, out System.Text.Rune rune, out int consumed) == System.Buffers.OperationStatus.Done && consumed == s.Length)\n"
		);
		sb.Append("\t\t\t{\n");
		sb.Append($"\t\t\t\treturn new {secure}(rune);\n");
		sb.Append("\t\t\t}\n\n");
		sb.Append(
			"\t\t\tthrow new System.FormatException(\"The UTF-8 input does not contain exactly one encoded rune.\");\n"
		);
		sb.Append("\t\t}\n");
		sb.Append(
			Doc($"Tries to parse UTF-8 bytes holding exactly one rune into its secured form.")
		);
		sb.Append(Inline);
		sb.Append(
			$"\t\tstatic bool System.IUtf8SpanParsable<{secure}>.TryParse(ReadOnlySpan<byte> s, IFormatProvider provider, out {secure} result)\n"
		);
		sb.Append("\t\t{\n");
		sb.Append(
			"\t\t\tif (System.Text.Rune.DecodeFromUtf8(s, out System.Text.Rune rune, out int consumed) == System.Buffers.OperationStatus.Done && consumed == s.Length)\n"
		);
		sb.Append("\t\t\t{\n");
		sb.Append($"\t\t\t\tresult = new {secure}(rune);\n");
		sb.Append("\t\t\t\treturn true;\n");
		sb.Append("\t\t\t}\n\n");
		sb.Append("\t\t\tresult = default;\n");
		sb.Append("\t\t\treturn false;\n");
		sb.Append("\t\t}\n");
		sb.Append("#endif\n");
	}

	/// <summary>Allocation-light char enumeration over one decrypted snapshot (SecureString only).</summary>
	private static void EmitEnumerable(StringBuilder sb, string secure)
	{
		sb.Append(Doc("Returns an enumerator over the decrypted characters."));
		sb.Append(Inline);
		sb.Append(
			"\t\tpublic System.CharEnumerator GetEnumerator() => Decrypted.GetEnumerator();\n"
		);
		sb.Append(Doc("Returns an enumerator over the decrypted characters."));
		sb.Append(Inline);
		sb.Append(
			"\t\tSystem.Collections.Generic.IEnumerator<char> System.Collections.Generic.IEnumerable<char>.GetEnumerator() => GetEnumerator();\n"
		);
		sb.Append(Doc("Returns an enumerator over the decrypted characters."));
		sb.Append(Inline);
		sb.Append(
			"\t\tSystem.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();\n"
		);
	}
}
