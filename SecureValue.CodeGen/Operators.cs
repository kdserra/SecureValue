// Minimal operator set for integral wrappers: every wrapper
// owns exactly O(P,P) + O(P,T_own) + O(T_own,P), plus the int-literal interop
// twins O(P,int) + O(int,P) (unsuffixed literals are int, and sbyte/byte/
// short/ushort have no suffix). No cross-type matrix, no (P1,P2) pairs:
// cross-width mixes are compile errors unless the int bridge resolves them
// value-safely, and cross-Secure mixes need an explicit cast.
// Returns mirror the BCL via Promote, except (P,P) which narrows into its own
// wrapper exactly like the hand-written originals this replaces. Pairs the
// BCL itself rejects (ulong x int) stay errors here too.
namespace SecureValue.CodeGen;

using System.Text;

internal static class Operators
{
	/// <summary>BCL-error pairs stay errors: no twin is emitted for these.</summary>
	internal static readonly HashSet<string> ErrorPairs = new()
	{
		"sbyte|ulong",
		"short|ulong",
		"int|ulong",
		"long|ulong",
		"ulong|sbyte",
		"ulong|short",
		"ulong|int",
		"ulong|long",
		"float|decimal",
		"double|decimal",
		"decimal|float",
		"decimal|double",
	};

	/// <summary>BCL numeric promotion for two primitive operand types.</summary>
	internal static string Promote(string a, string b)
	{
		if (a == "decimal" || b == "decimal")
		{
			return "decimal";
		}

		if (a == "double" || b == "double")
		{
			return "double";
		}

		if (a == "float" || b == "float")
		{
			return "float";
		}

		if (a == "ulong" || b == "ulong")
		{
			return "ulong";
		}

		if (a == "long" || b == "long")
		{
			return "long";
		}

		if (a == "uint" || b == "uint")
		{
			string o = a == "uint" ? b : a;
			if (o is "sbyte" or "short" or "int")
			{
				return "long";
			}

			return "uint";
		}

		return "int";
	}

	/// <summary>Secured wrapper for a promoted primitive type.</summary>
	internal static string Secured(string t) =>
		t switch
		{
			"sbyte" => "SecureSByte",
			"byte" => "SecureByte",
			"short" => "SecureShort",
			"ushort" => "SecureUShort",
			"int" => "SecureInt",
			"uint" => "SecureUInt",
			"long" => "SecureLong",
			"ulong" => "SecureULong",
			"float" => "SecureFloat",
			"double" => "SecureDouble",
			"decimal" => "SecureDecimal",
			"char" => "SecureChar",
			_ => throw new InvalidOperationException("No secured wrapper for " + t),
		};

	private static readonly (string Op, string Verb)[] Arithmetic =
	{
		("+", "Adds"),
		("-", "Subtracts"),
		("*", "Multiplies"),
		("/", "Divides"),
		("%", "Remainders"),
	};

	private static readonly (string Op, string Verb)[] Comparisons =
	{
		("==", "Tests"),
		("!=", "Tests"),
		("<", "Compares"),
		("<=", "Compares"),
		(">", "Compares"),
		(">=", "Compares"),
	};

	private static readonly (string Op, string Verb)[] Bitwise = { ("&", "ANDs"), ("|", "ORs") };

	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	private static string Article(string t) => t == "int" ? "an" : "a";

	private static string Noun(string t) => t == "char" ? "a char" : Article(t) + " " + t;

	/// <summary>Wraps a decrypted expression back into its secured form (hand-parity casts).</summary>
	internal static string Wrap(string secure, string primitive, string expr) =>
		primitive == "ulong" ? $"new {secure}(({expr}))" : $"new {secure}(({primitive})({expr}))";

	/// <summary>
	/// Emits the full minimal set for one integral wrapper: O(P,P) + own twins
	/// both orders + int interop twins both orders (skipped for SecureInt
	/// itself and where the BCL rejects the pair) + C#9-legal (P,int) shifts.
	/// </summary>
	public static void EmitIntegral(StringBuilder sb, string secure, string primitive)
	{
		EmitSelf(sb, secure, primitive);

		string ownRet = Secured(Promote(primitive, primitive));
		foreach ((string op, string verb) in Arithmetic)
		{
			Twin(sb, secure, primitive, ownRet, op, verb, isComparison: false, operand: primitive);
			Mirror(
				sb,
				secure,
				primitive,
				ownRet,
				op,
				verb,
				isComparison: false,
				operand: primitive
			);
		}

		foreach ((string op, string verb) in Comparisons)
		{
			Twin(sb, secure, primitive, ownRet, op, verb, isComparison: true, operand: primitive);
			Mirror(sb, secure, primitive, ownRet, op, verb, isComparison: true, operand: primitive);
		}

		foreach ((string op, string verb) in Bitwise)
		{
			Twin(sb, secure, primitive, ownRet, op, verb, isComparison: false, operand: primitive);
			Mirror(
				sb,
				secure,
				primitive,
				ownRet,
				op,
				verb,
				isComparison: false,
				operand: primitive
			);
		}

		// Int-literal interop (unsuffixed literals are int). Skipped for
		// SecureInt itself (own twins already cover int) and where the BCL
		// rejects the pair (ulong x int stays an error, like the BCL).
		if (primitive != "int" && !ErrorPairs.Contains(primitive + "|int"))
		{
			string ret = Secured(Promote(primitive, "int"));
			foreach ((string op, string verb) in Arithmetic)
			{
				Twin(sb, secure, primitive, ret, op, verb, isComparison: false, operand: "int");
				Mirror(sb, secure, primitive, ret, op, verb, isComparison: false, operand: "int");
			}

			foreach ((string op, string verb) in Comparisons)
			{
				Twin(sb, secure, primitive, ret, op, verb, isComparison: true, operand: "int");
				Mirror(sb, secure, primitive, ret, op, verb, isComparison: true, operand: "int");
			}

			foreach ((string op, string verb) in Bitwise)
			{
				Twin(sb, secure, primitive, ret, op, verb, isComparison: false, operand: "int");
				Mirror(sb, secure, primitive, ret, op, verb, isComparison: false, operand: "int");
			}
		}

		EmitShifts(sb, secure, primitive);
	}

	/// <summary>O(P,P): hand-parity operators over two secured values (narrowing, like the originals).</summary>
	private static void EmitSelf(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(Doc($"Tests two secured {primitive} values for equality."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static bool operator ==({secure} left, {secure} right) =>\n");
		sb.Append("\t\t\tleft.Equals(right);\n");
		sb.Append(Doc($"Tests two secured {primitive} values for inequality."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static bool operator !=({secure} left, {secure} right) =>\n");
		sb.Append("\t\t\t!left.Equals(right);\n");
		foreach ((string op, _) in Comparisons)
		{
			if (op is "==" or "!=")
			{
				continue;
			}

			sb.Append(Doc($"Compares two secured {primitive} values."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static bool operator {op}({secure} left, {secure} right) =>\n");
			sb.Append($"\t\t\tleft.Decrypted {op} right.Decrypted;\n");
		}

		foreach ((string op, string verb) in Arithmetic)
		{
			sb.Append(Doc($"{verb} two secured {primitive} values."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, {secure} b) =>\n");
			sb.Append($"\t\t\t{Wrap(secure, primitive, $"a.Decrypted {op} b.Decrypted")};\n");
		}

		EmitUnary(sb, secure, primitive);

		foreach ((string op, string verb) in Bitwise)
		{
			sb.Append(
				Doc(
					$"Computes the bitwise {(op == "&" ? "AND" : "OR")} of two secured {primitive} values."
				)
			);
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, {secure} b) =>\n");
			sb.Append($"\t\t\t{Wrap(secure, primitive, $"a.Decrypted {op} b.Decrypted")};\n");
		}

		sb.Append(Doc($"Computes the bitwise XOR of two secured {primitive} values."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ^({secure} a, {secure} b) =>\n");
		sb.Append($"\t\t\t{Wrap(secure, primitive, "a.Decrypted ^ b.Decrypted")};\n");
	}

	/// <summary>Unary O(P): hand-parity forms (ULong keeps its 0UL-based negate/plus).</summary>
	private static void EmitUnary(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(Doc($"Negates a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator -({secure} a) =>\n");
		sb.Append(
			primitive == "ulong"
				? $"\t\t\tnew {secure}(0UL - a.Decrypted);\n"
				: $"\t\t\t{Wrap(secure, primitive, "-a.Decrypted")};\n"
		);
		sb.Append(Doc($"Applies unary plus to a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator +({secure} a) =>\n");
		sb.Append(
			primitive == "ulong"
				? $"\t\t\tnew {secure}(0UL + a.Decrypted);\n"
				: $"\t\t\t{Wrap(secure, primitive, "+a.Decrypted")};\n"
		);
		sb.Append(Doc($"Increments a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ++({secure} a) =>\n");
		sb.Append(
			primitive == "ulong"
				? $"\t\t\tnew {secure}((a.Decrypted + 1));\n"
				: $"\t\t\t{Wrap(secure, primitive, "a.Decrypted + 1")};\n"
		);
		sb.Append(Doc($"Decrements a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator --({secure} a) =>\n");
		sb.Append(
			primitive == "ulong"
				? $"\t\t\tnew {secure}((a.Decrypted - 1UL));\n"
				: $"\t\t\t{Wrap(secure, primitive, "a.Decrypted - 1")};\n"
		);
		sb.Append(Doc($"Computes the bitwise NOT of a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ~({secure} a) =>\n");
		sb.Append(
			primitive == "ulong"
				? $"\t\t\tnew {secure}((~a.Decrypted));\n"
				: $"\t\t\t{Wrap(secure, primitive, "~a.Decrypted")};\n"
		);
	}

	private static void Twin(
		StringBuilder sb,
		string secure,
		string primitive,
		string ret,
		string op,
		string verb,
		bool isComparison,
		string operand
	)
	{
		sb.Append(Doc($"{verb} a secured {primitive} value and {Noun(operand)}."));
		sb.Append(Inline);
		sb.Append(
			$"\t\tpublic static {(isComparison ? "bool" : ret)} operator {op}({secure} a, {operand} b) =>\n"
		);
		if (isComparison)
		{
			sb.Append($"\t\t\ta.Decrypted {op} b;\n");
		}
		else if (op == "|" && ret is "SecureInt" or "SecureLong")
		{
			// Cross-width | uses Bits.Or for BCL two's-complement semantics.
			sb.Append($"\t\t\tnew {ret}(Bits.Or(a.Decrypted, b));\n");
		}
		else
		{
			sb.Append($"\t\t\tnew {ret}(a.Decrypted {op} b);\n");
		}
	}

	private static void Mirror(
		StringBuilder sb,
		string secure,
		string primitive,
		string ret,
		string op,
		string verb,
		bool isComparison,
		string operand
	)
	{
		sb.Append(Doc($"{verb} {Noun(operand)} and a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append(
			$"\t\tpublic static {(isComparison ? "bool" : ret)} operator {op}({operand} a, {secure} b) =>\n"
		);
		if (isComparison)
		{
			sb.Append($"\t\t\ta {op} b.Decrypted;\n");
		}
		else if (op == "|" && ret is "SecureInt" or "SecureLong")
		{
			sb.Append($"\t\t\tnew {ret}(Bits.Or(a, b.Decrypted));\n");
		}
		else
		{
			sb.Append($"\t\t\tnew {ret}(a {op} b.Decrypted);\n");
		}
	}

	/// <summary>C# 9 only allows an int shift count; (P,int) shifts are the legal shape.</summary>
	private static void EmitShifts(StringBuilder sb, string secure, string primitive)
	{
		string cast = primitive is "int" or "long" or "uint" or "ulong" ? "" : $"({primitive})";
		foreach (string op in new[] { "<<", ">>" })
		{
			sb.Append(
				Doc(
					$"Shifts a secured {primitive} value {(op == "<<" ? "left" : "right")} by a plain amount."
				)
			);
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, int b) =>\n");
			sb.Append($"\t\t\tnew {secure}({cast}(a.Decrypted {op} b));\n");
		}
	}
}
