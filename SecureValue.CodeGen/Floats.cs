// Minimal operator sets for the non-integral primitives:
// float/double/decimal own O(P,P) + O(P,T_own) + O(T_own,P) plus int-literal
// interop twins (BCL-defined widening, e.g. float+int); char owns O(P,P)
// comparisons plus (P,char)/(char,P) +,-,comparisons,&,| and (P,int)/(int,P)
// twins (BCL char+int widens to int). No cross-type matrix beyond int, no
// pairs. Decimal/float bitwise stay absent (the BCL defines none).
namespace SecureValue.CodeGen;

using System.Text;

internal static class Floats
{
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

	/// <summary>Wraps a decrypted expression (hand-parity: decimal is cast-free).</summary>
	private static string Wrap(string secure, string primitive, string expr) =>
		primitive == "decimal" ? $"new {secure}({expr})" : $"new {secure}(({primitive})({expr}))";

	/// <summary>Minimal set for float, double and decimal: self + own twins + int twins.</summary>
	public static void EmitFloat(StringBuilder sb, string secure, string primitive)
	{
		EmitFloatSelf(sb, secure, primitive);
		foreach (string operand in new[] { primitive, "int" })
		{
			foreach ((string op, string verb) in Arithmetic)
			{
				Twin(sb, secure, primitive, secure, op, verb, isComparison: false, operand);
				Mirror(sb, secure, primitive, secure, op, verb, isComparison: false, operand);
			}

			foreach ((string op, string verb) in Comparisons)
			{
				Twin(sb, secure, primitive, secure, op, verb, isComparison: true, operand);
				Mirror(sb, secure, primitive, secure, op, verb, isComparison: true, operand);
			}
		}
	}

	/// <summary>O(P,P) for float/double/decimal (hand-parity bodies).</summary>
	private static void EmitFloatSelf(StringBuilder sb, string secure, string primitive)
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

		sb.Append(Doc($"Negates a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator -({secure} a) =>\n");
		sb.Append(
			primitive == "decimal"
				? $"\t\t\tnew {secure}(-a.Decrypted);\n"
				: $"\t\t\t{Wrap(secure, primitive, "-a.Decrypted")};\n"
		);
		sb.Append(Doc($"Applies unary plus to a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator +({secure} a) =>\n");
		sb.Append(
			primitive == "decimal"
				? $"\t\t\tnew {secure}(+a.Decrypted);\n"
				: $"\t\t\t{Wrap(secure, primitive, "+a.Decrypted")};\n"
		);
		sb.Append(Doc($"Increments a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ++({secure} a) =>\n");
		sb.Append($"\t\t\t{Wrap(secure, primitive, "a.Decrypted + 1")};\n");
		sb.Append(Doc($"Decrements a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator --({secure} a) =>\n");
		sb.Append($"\t\t\t{Wrap(secure, primitive, "a.Decrypted - 1")};\n");
	}

	/// <summary>
	/// Minimal set for char: O(P,P) comparisons + ++/--, own twins
	/// (+,-,comparisons,&,| widening to SecureInt), and int twins (same set).
	/// </summary>
	public static void EmitChar(StringBuilder sb, string secure, string primitive)
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

		sb.Append(Doc($"Increments a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ++({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}((char)(a.Decrypted + 1));\n");
		sb.Append(Doc($"Decrements a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator --({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}((char)(a.Decrypted - 1));\n");

		foreach (string operand in new[] { "char", "int" })
		{
			foreach ((string op, string verb) in new[] { ("+", "Adds"), ("-", "Subtracts") })
			{
				Twin(sb, secure, primitive, "SecureInt", op, verb, isComparison: false, operand);
				Mirror(sb, secure, primitive, "SecureInt", op, verb, isComparison: false, operand);
			}

			foreach ((string op, string verb) in Comparisons)
			{
				Twin(sb, secure, primitive, "SecureInt", op, verb, isComparison: true, operand);
				Mirror(sb, secure, primitive, "SecureInt", op, verb, isComparison: true, operand);
			}

			foreach ((string op, string verb) in Bitwise)
			{
				Twin(sb, secure, primitive, "SecureInt", op, verb, isComparison: false, operand);
				Mirror(sb, secure, primitive, "SecureInt", op, verb, isComparison: false, operand);
			}
		}
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
}
