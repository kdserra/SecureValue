// System.Numerics-family twins: BigInteger owns O(P,P) + O(P,BigInteger) +
// O(BigInteger,P) plus int-literal interop twins (BCL defines BigInteger+int);
// Complex keeps its double pairs + self forms; vectors/matrices/quaternion/
// plane keep self forms + forward-only float scalars. No cross-type matrix,
// no cross-Secure pairs: cross-width mixes need an explicit cast.
namespace SecureValue.CodeGen;

using System.Text;

internal static class Numerics
{
	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	private static readonly (string Op, string Verb)[] BigIntArithmetic =
	{
		("+", "Adds"),
		("-", "Subtracts"),
		("*", "Multiplies"),
		("/", "Divides"),
		("%", "Remainders"),
	};

	private static readonly (string Op, string Verb)[] BigIntComparisons =
	{
		("==", "Tests"),
		("!=", "Tests"),
		("<", "Compares"),
		("<=", "Compares"),
		(">", "Compares"),
		(">=", "Compares"),
	};

	private static readonly (string Op, string Verb)[] BigIntBitwise =
	{
		("&", "ANDs"),
		("|", "ORs"),
		("^", "XORs"),
	};

	/// <summary>Minimal set for BigInteger: self + own twins + int twins + shifts.</summary>
	public static void EmitBigInteger(StringBuilder sb, string secure)
	{
		foreach ((string op, string verb) in BigIntArithmetic)
		{
			Pair(
				sb,
				secure,
				secure,
				"BigInteger",
				secure,
				"BigInteger",
				op,
				verb,
				isComparison: false
			);
		}

		foreach ((string op, string verb) in BigIntComparisons)
		{
			Pair(
				sb,
				secure,
				secure,
				"BigInteger",
				secure,
				"BigInteger",
				op,
				verb,
				isComparison: true
			);
		}

		foreach ((string op, string verb) in BigIntBitwise)
		{
			Pair(
				sb,
				secure,
				secure,
				"BigInteger",
				secure,
				"BigInteger",
				op,
				verb,
				isComparison: false
			);
		}

		sb.Append(Doc("Negates a secured big integer value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator -({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}(-a.Decrypted);\n");
		sb.Append(Doc("Applies unary plus to a secured big integer value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator +({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}(+a.Decrypted);\n");
		sb.Append(Doc("Increments a secured big integer value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ++({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}(a.Decrypted + BigInteger.One);\n");
		sb.Append(Doc("Decrements a secured big integer value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator --({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}(a.Decrypted - BigInteger.One);\n");
		sb.Append(Doc("Computes the bitwise NOT of a secured big integer value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator ~({secure} a) =>\n");
		sb.Append($"\t\t\tnew {secure}(~a.Decrypted);\n");

		foreach (string x in new[] { "BigInteger", "int" })
		{
			foreach ((string op, string verb) in BigIntArithmetic)
			{
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: false,
					fwd: true,
					operand: x
				);
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: false,
					fwd: false,
					operand: x
				);
			}

			foreach ((string op, string verb) in BigIntComparisons)
			{
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: true,
					fwd: true,
					operand: x
				);
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: true,
					fwd: false,
					operand: x
				);
			}

			foreach ((string op, string verb) in BigIntBitwise)
			{
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: false,
					fwd: true,
					operand: x
				);
				Binary(
					sb,
					secure,
					"BigInteger",
					op,
					verb,
					isComparison: false,
					fwd: false,
					operand: x
				);
			}
		}

		foreach (string op in new[] { "<<", ">>" })
		{
			sb.Append(
				Doc(
					$"Shifts a secured big integer value {(op == "<<" ? "left" : "right")} by a plain amount."
				)
			);
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, int b) =>\n");
			sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
		}
	}

	private static void Binary(
		StringBuilder sb,
		string secure,
		string primitive,
		string op,
		string verb,
		bool isComparison,
		bool fwd,
		string operand
	)
	{
		string lt = fwd ? secure : operand;
		string rt = fwd ? operand : secure;
		string l = fwd ? "a.Decrypted" : "a";
		string r = fwd ? "b" : "b.Decrypted";
		sb.Append(
			Doc(
				fwd
					? $"{verb} a secured big integer value and a {operand}."
					: $"{verb} a {operand} and a secured big integer value."
			)
		);
		sb.Append(Inline);
		sb.Append(
			$"\t\tpublic static {(isComparison ? "bool" : secure)} operator {op}({lt} a, {rt} b) =>\n"
		);
		sb.Append(isComparison ? $"\t\t\t{l} {op} {r};\n" : $"\t\t\tnew {secure}({l} {op} {r});\n");
	}

	private static void Pair(
		StringBuilder sb,
		string ret,
		string first,
		string firstPrimitive,
		string second,
		string secondPrimitive,
		string op,
		string verb,
		bool isComparison
	)
	{
		sb.Append(
			Doc($"{verb} a secured {firstPrimitive} value and a secured {secondPrimitive} value.")
		);
		sb.Append(Inline);
		sb.Append(
			$"\t\tpublic static {(isComparison ? "bool" : ret)} operator {op}({first} a, {second} b) =>\n"
		);
		sb.Append(
			isComparison
				? "\t\t\ta.Decrypted " + op + " b.Decrypted;\n"
				: "\t\t\tnew " + ret + "(a.Decrypted " + op + " b.Decrypted);\n"
		);
	}

	/// <summary>Complex double pairs + self forms (BCL inventory; no ordering, no pairs).</summary>
	public static void EmitComplex(StringBuilder sb, string secure, bool emitReverse)
	{
		foreach (string op in new[] { "+", "-", "*", "/" })
		{
			string verb = op switch
			{
				"+" => "Adds",
				"-" => "Subtracts",
				"*" => "Multiplies",
				_ => "Divides",
			};
			sb.Append(Doc($"{verb} a secured complex value and a plain double."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, double b) =>\n");
			sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
			if (emitReverse)
			{
				sb.Append(Doc($"{verb} a plain double and a secured complex value."));
				sb.Append(Inline);
				sb.Append($"\t\tpublic static {secure} operator {op}(double a, {secure} b) =>\n");
				sb.Append($"\t\t\tnew {secure}(a {op} b.Decrypted);\n");
			}
		}

		foreach (string op in new[] { "+", "-", "*", "/" })
		{
			string verb = op switch
			{
				"+" => "Adds",
				"-" => "Subtracts",
				"*" => "Multiplies",
				_ => "Divides",
			};
			sb.Append(Doc($"{verb} a secured complex value and a plain complex value."));
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static {secure} operator {op}({secure} a, System.Numerics.Complex b) =>\n"
			);
			sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
			sb.Append(Doc($"{verb} a plain complex value and a secured complex value."));
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static {secure} operator {op}(System.Numerics.Complex a, {secure} b) =>\n"
			);
			sb.Append($"\t\t\tnew {secure}(a {op} b.Decrypted);\n");
		}

		foreach (string op in new[] { "==", "!=" })
		{
			sb.Append(
				Doc(
					$"Tests a secured complex value and a plain complex value for {(op == "==" ? "equality" : "inequality")}."
				)
			);
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static bool operator {op}({secure} a, System.Numerics.Complex b) =>\n"
			);
			sb.Append($"\t\t\ta.Decrypted {op} b;\n");
			sb.Append(
				Doc(
					$"Tests a plain complex value and a secured complex value for {(op == "==" ? "equality" : "inequality")}."
				)
			);
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static bool operator {op}(System.Numerics.Complex a, {secure} b) =>\n"
			);
			sb.Append($"\t\t\ta {op} b.Decrypted;\n");
		}
	}

	/// <summary>Self-with-plain twins for vector/matrix/quaternion/plane wrappers.</summary>
	/// <param name="scalarOps">Scalar twin ops with float, forward-only (e.g. "*", "/").</param>
	/// <param name="selfOps">Self-with-plain ops, both orders (e.g. "+", "-", "*", "/").</param>
	public static void EmitSelfForms(
		StringBuilder sb,
		string secure,
		string primitive,
		string[] selfOps,
		string[] scalarOps
	)
	{
		foreach (string op in selfOps)
		{
			bool comparison = op is "==" or "!=";
			string verb = comparison
				? "Tests"
				: op switch
				{
					"+" => "Adds",
					"-" => "Subtracts",
					"*" => "Multiplies",
					_ => "Divides",
				};
			sb.Append(Doc($"{verb} a secured {primitive} value and a plain {primitive} value."));
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static {(comparison ? "bool" : secure)} operator {op}({secure} a, {primitive} b) =>\n"
			);
			sb.Append(
				comparison
					? $"\t\t\ta.Decrypted {op} b;\n"
					: $"\t\t\tnew {secure}(a.Decrypted {op} b);\n"
			);
			sb.Append(Doc($"{verb} a plain {primitive} value and a secured {primitive} value."));
			sb.Append(Inline);
			sb.Append(
				$"\t\tpublic static {(comparison ? "bool" : secure)} operator {op}({primitive} a, {secure} b) =>\n"
			);
			sb.Append(
				comparison
					? $"\t\t\ta {op} b.Decrypted;\n"
					: $"\t\t\tnew {secure}(a {op} b.Decrypted);\n"
			);
		}

		foreach (string op in scalarOps)
		{
			string verb = op == "*" ? "Multiplies" : "Divides";
			sb.Append(Doc($"{verb} a secured {primitive} value by a plain float factor."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, float b) =>\n");
			sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
		}
	}
}
