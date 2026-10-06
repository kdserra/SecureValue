// Unity engine-type twins (namespace SecureValue.Unity, whole-file
// UNITY_5_3_OR_NEWER guarded): ==/!= self-with-plain everywhere the hand
// (P,P) proves engine == exists, EXCEPT Ray and Color32: UnityEngine defines
// no ==/!= on those two, so their twins route through
// Equals like the hand (P,P) forms do. Arithmetic self-twins only for certain
// engine forms (vectors/Color/Quaternion/Matrix products), scalar twins
// forward-only (no pairs exist out here to rescue a mirror tie, so reverse
// goes through the engine via conversions).
namespace SecureValue.CodeGen;

using System.Text;

internal static class Unity
{
	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	/// <summary>Self-with-plain arithmetic ops per Unity type (engine inventory).</summary>
	internal static readonly Dictionary<string, string[]> SelfArithmetic = new()
	{
		["SecureVector2"] = new[] { "+", "-" },
		["SecureVector2Int"] = new[] { "+", "-" },
		["SecureVector3"] = new[] { "+", "-" },
		["SecureVector3Int"] = new[] { "+", "-" },
		["SecureVector4"] = new[] { "+", "-" },
		["SecureColor"] = new[] { "+", "-" },
		["SecureQuaternion"] = new[] { "*" },
		["SecureMatrix4x4"] = new[] { "*" },
	};

	/// <summary>Scalar twin (op, scalar type), forward-only, per Unity type.</summary>
	internal static readonly Dictionary<string, (string Op, string Scalar)[]> ScalarTwins = new()
	{
		["SecureVector2"] = new[] { ("*", "float"), ("/", "float") },
		["SecureVector3"] = new[] { ("*", "float"), ("/", "float") },
		["SecureVector4"] = new[] { ("*", "float"), ("/", "float") },
		["SecureVector2Int"] = new[] { ("*", "int"), ("/", "int") },
		["SecureVector3Int"] = new[] { ("*", "int"), ("/", "int") },
		["SecureColor"] = new[] { ("*", "float") },
	};

	public static void EmitUnityTwins(StringBuilder sb, string secure, string primitive)
	{
		bool equalsBased = secure is "SecureRay" or "SecureColor32";
		foreach (string op in new[] { "==", "!=" })
		{
			SelfBool(sb, secure, primitive, op, equalsBased);
		}

		if (SelfArithmetic.TryGetValue(secure, out string[]? arith))
		{
			foreach (string op in arith)
			{
				SelfValue(sb, secure, primitive, op);
			}
		}

		if (ScalarTwins.TryGetValue(secure, out (string Op, string Scalar)[]? scalars))
		{
			foreach ((string op, string scalar) in scalars)
			{
				string verb = op == "*" ? "Multiplies" : "Divides";
				sb.Append(Doc($"{verb} a secured {primitive} value by a plain {scalar} factor."));
				sb.Append(Inline);
				sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, {scalar} b) =>\n");
				sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
			}
		}
	}

	private static void SelfBool(
		StringBuilder sb,
		string secure,
		string primitive,
		string op,
		bool equalsBased
	)
	{
		// Equals-routed bodies for engine types without ==/!= (Ray, Color32):
		// a.Decrypted.Equals(b) / !a.Decrypted.Equals(b) forward,
		// a.Equals(b.Decrypted) / !a.Equals(b.Decrypted) reverse.
		string fwdBody = op == "==" ? "a.Decrypted.Equals(b);" : "!a.Decrypted.Equals(b);";
		string revBody = op == "==" ? "a.Equals(b.Decrypted);" : "!a.Equals(b.Decrypted);";
		sb.Append(
			Doc(
				$"Tests a secured {primitive} value and a plain {primitive} value for {(op == "==" ? "equality" : "inequality")}."
			)
		);
		sb.Append(Inline);
		sb.Append($"\t\tpublic static bool operator {op}({secure} a, {primitive} b) =>\n");
		sb.Append(equalsBased ? $"\t\t\t{fwdBody}\n" : $"\t\t\ta.Decrypted {op} b;\n");
		sb.Append(
			Doc(
				$"Tests a plain {primitive} value and a secured {primitive} value for {(op == "==" ? "equality" : "inequality")}."
			)
		);
		sb.Append(Inline);
		sb.Append($"\t\tpublic static bool operator {op}({primitive} a, {secure} b) =>\n");
		sb.Append(equalsBased ? $"\t\t\t{revBody}\n" : $"\t\t\ta {op} b.Decrypted;\n");
	}

	private static void SelfValue(StringBuilder sb, string secure, string primitive, string op)
	{
		string verb = op switch
		{
			"+" => "Adds",
			"-" => "Subtracts",
			"*" => "Multiplies",
			_ => "Divides",
		};
		sb.Append(Doc($"{verb} a secured {primitive} value and a plain {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator {op}({secure} a, {primitive} b) =>\n");
		sb.Append($"\t\t\tnew {secure}(a.Decrypted {op} b);\n");
		sb.Append(Doc($"{verb} a plain {primitive} value and a secured {primitive} value."));
		sb.Append(Inline);
		sb.Append($"\t\tpublic static {secure} operator {op}({primitive} a, {secure} b) =>\n");
		sb.Append($"\t\t\tnew {secure}(a {op} b.Decrypted);\n");
	}
}
