// Secure-to-Secure conversions: EXPLICIT throughout.
//
// Deliberately NOT implicit, even where the BCL widens implicitly (int → long):
// an implicit P2 → P1 makes every cross-Secure (P1, P2) call ambiguous.
// Explicit conversions never compete in overload resolution, so the forward
// twin matrix stays unambiguous while full cross-type mobility remains one
// cast away (the same UX as BCL narrowing).
namespace SecureValue.CodeGen;

using System.Text;

internal static class Conversions
{
	/// <summary>All convertible primitives in the matrix.</summary>
	internal static readonly string[] Primitives =
	{
		"sbyte",
		"byte",
		"short",
		"ushort",
		"int",
		"uint",
		"long",
		"ulong",
		"char",
		"float",
		"double",
		"decimal",
	};

	private const string Inline = "\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]\n";

	private static string Doc(string text) => "\t\t/// <summary>" + text + "</summary>\n";

	/// <summary>
	/// Emits explicit Secure-to-Secure conversions home in the SOURCE wrapper's
	/// partial, for every convertible primitive pair.
	/// </summary>
	public static void EmitSecureConversions(StringBuilder sb, string secure, string primitive)
	{
		foreach (string target in Primitives)
		{
			if (target == primitive)
			{
				continue;
			}

			string targetSecure = Operators.Secured(target);
			sb.Append(Doc($"Converts a secured {primitive} value into its secured {target} form."));
			sb.Append(Inline);
			sb.Append($"\t\tpublic static explicit operator {targetSecure}({secure} value) =>\n");
			sb.Append($"\t\t\tnew {targetSecure}(({target})value.Decrypted);\n");
		}
	}
}
