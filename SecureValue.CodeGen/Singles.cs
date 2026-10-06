// Self-with-plain twins: ==/!= and ordering against the wrapper's OWN
// underlying type, e.g. ==(SecureBool, bool) and ==(bool, SecureBool).
// Emitted exactly where the BCL defines the underlying form (bool and string
// have equality only — no BCL ordering; dates/Guid/Rune ordering mirrors the
// hand-written (P,P) set). Bodies run on Decrypted temporaries like every
// other twin. Date arithmetic and string concatenation live here too:
// each mirrors a real BCL form (nothing novel).
namespace SecureValue.CodeGen;

using System.Text;

internal static class Singles
{
	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	/// <summary>Plain-side signature type (oblivious: no '?' even for string).</summary>
	private static string Plain(string primitive) => primitive;

	/// <summary>== and != against the wrapper's own plain type, both orders.</summary>
	public static void EmitEquality(StringBuilder sb, string secure, string primitive)
	{
		foreach (
			(string op, bool fwd) in new[]
			{
				("==", true),
				("==", false),
				("!=", true),
				("!=", false),
			}
		)
		{
			string lt = fwd ? secure : Plain(primitive);
			string rt = fwd ? Plain(primitive) : secure;
			string l = fwd ? "a.Decrypted" : "a";
			string r = fwd ? "b" : "b.Decrypted";
			string ln = fwd ? $"a secured {primitive} value" : $"a {primitive} value";
			string rn = fwd ? $"a {primitive} value" : $"a secured {primitive} value";
			sb.Append(Doc($"Tests {ln} and {rn} for {(op == "==" ? "equality" : "inequality")}."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static bool operator {op}({lt} a, {rt} b) =>\n");
			sb.Append($"\t\t\t{l} {op} {r};\n");
		}
	}

	/// <summary>Ordering against the wrapper's own plain type, both orders.</summary>
	public static void EmitOrdering(StringBuilder sb, string secure, string primitive)
	{
		foreach (
			(string op, bool fwd) in new[]
			{
				("<", true),
				("<", false),
				("<=", true),
				("<=", false),
				(">", true),
				(">", false),
				(">=", true),
				(">=", false),
			}
		)
		{
			string lt = fwd ? secure : primitive;
			string rt = fwd ? primitive : secure;
			string l = fwd ? "a.Decrypted" : "a";
			string r = fwd ? "b" : "b.Decrypted";
			string ln = fwd ? $"a secured {primitive} value" : $"a {primitive} value";
			string rn = fwd ? $"a {primitive} value" : $"a secured {primitive} value";
			sb.Append(Doc($"Compares {ln} and {rn}."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static bool operator {op}({lt} a, {rt} b) =>\n");
			sb.Append($"\t\t\t{l} {op} {r};\n");
		}
	}

	/// <summary>String concatenation with a plain string, both orders (null-safe via BCL +).</summary>
	public static void EmitStringConcat(StringBuilder sb, string secure)
	{
		foreach (bool fwd in new[] { true, false })
		{
			string lt = fwd ? secure : "string";
			string rt = fwd ? "string" : secure;
			string l = fwd ? "a.Decrypted" : "a";
			string r = fwd ? "b" : "b.Decrypted";
			sb.Append(
				Doc(
					fwd
						? "Concatenates a secured string value and a plain string."
						: "Concatenates a plain string and a secured string value."
				)
			);
			sb.Append(Inline);
			sb.Append($"\t\tpublic static {secure} operator +({lt} a, {rt} b) =>\n");
			sb.Append($"\t\t\tnew {secure}({l} + {r});\n");
		}
	}

	/// <summary>
	/// DateTime-family arithmetic twins. Exactly the BCL inventory, nothing
	/// novel: fwd-only where the BCL defines one order (DT+TS), both orders
	/// where it defines both (TS*double), self-with-plain where it defines
	/// same-type forms (DT-DT, TS/TS).
	/// </summary>
	public static void EmitDateArithmetic(StringBuilder sb, string secure, string primitive)
	{
		switch (primitive)
		{
			case "DateTime":
			case "DateTimeOffset":
				Binary(
					sb,
					secure,
					"+",
					secure,
					"TimeSpan",
					"Adds a time span to a secured date value."
				);
				Binary(
					sb,
					secure,
					"-",
					secure,
					"TimeSpan",
					"Subtracts a time span from a secured date value."
				);
				Difference(sb, secure, primitive);
				break;
			case "TimeSpan":
				Binary(
					sb,
					secure,
					"*",
					secure,
					"double",
					"Multiplies a secured time span by a plain factor."
				);
				Binary(
					sb,
					secure,
					"*",
					"double",
					secure,
					"Multiplies a plain factor by a secured time span."
				);
				Binary(
					sb,
					secure,
					"/",
					secure,
					"double",
					"Divides a secured time span by a plain factor."
				);
				Quotient(sb, secure, primitive);
				break;
		}
	}

	private static void Binary(
		StringBuilder sb,
		string secure,
		string op,
		string lt,
		string right,
		string text
	)
	{
		string l = lt == secure ? "a.Decrypted" : "a";
		string r = right == secure ? "b.Decrypted" : "b";
		sb.Append(Doc(text));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator {op}({lt} a, {right} b) =>\n");
		sb.Append($"\t\t\tnew {secure}({l} {op} {r});\n");
	}

	private static void Difference(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(Doc("Subtracts a plain date value from a secured date value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static SecureTimeSpan operator -({secure} a, {primitive} b) =>\n");
		sb.Append("\t\t\tnew SecureTimeSpan(a.Decrypted - b);\n");
		sb.Append(Doc("Subtracts a secured date value from a plain date value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static SecureTimeSpan operator -({primitive} a, {secure} b) =>\n");
		sb.Append("\t\t\tnew SecureTimeSpan(a - b.Decrypted);\n");
	}

	private static void Quotient(StringBuilder sb, string secure, string primitive)
	{
		sb.Append(Doc("Divides a secured time span by a plain time span."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static double operator /({secure} a, {primitive} b) =>\n");
		sb.Append("\t\t\ta.Decrypted / b;\n");
		sb.Append(Doc("Divides a plain time span by a secured time span."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static double operator /({primitive} a, {secure} b) =>\n");
		sb.Append("\t\t\ta / b.Decrypted;\n");
	}
}
