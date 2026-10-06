// SecureValue.CodeGen: owns all mechanical wrapper boilerplate (operators,
// conversions, interface implementations). Hand-written wrappers keep storage,
// construction, Decrypted/TryDecrypt, plain conversions and Unity callbacks;
// everything enumerable lives here and is emitted as sibling
// <c>SecureX.g.cs</c> partials.
//
// Run: dotnet run --project SecureValue.CodeGen [--check]
// --check exits 1 when generated output drifts.

using System.Text;
using SecureValue.CodeGen;

bool checkOnly = args.Contains("--check", StringComparer.Ordinal);
string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
int changed = 0;

foreach (WrapperSpec spec in Spec.All)
{
	StringBuilder members = new();
	switch (spec.Kind)
	{
		case WrapperKind.Integral:
			Operators.EmitIntegral(members, spec.SecureName, spec.Primitive);
			Conversions.EmitSecureConversions(members, spec.SecureName, spec.Primitive);
			break;
		case WrapperKind.Char:
			Floats.EmitChar(members, spec.SecureName, spec.Primitive);
			Conversions.EmitSecureConversions(members, spec.SecureName, spec.Primitive);
			break;
		case WrapperKind.Float:
			Floats.EmitFloat(members, spec.SecureName, spec.Primitive);
			Conversions.EmitSecureConversions(members, spec.SecureName, spec.Primitive);
			break;
		case WrapperKind.Bool:
		case WrapperKind.Guid:
		case WrapperKind.Rune:
			// No twin matrix for these kinds; self equality comes from here.
			Singles.EmitEquality(members, spec.SecureName, spec.Primitive);
			if (spec.Kind == WrapperKind.Rune)
			{
				Singles.EmitOrdering(members, spec.SecureName, spec.Primitive);
			}

			break;
		case WrapperKind.String:
			// Concat only: ==/!= are covered by (P,P) plus conversions.
			Singles.EmitStringConcat(members, spec.SecureName);
			break;
		case WrapperKind.DateTime:
			Singles.EmitEquality(members, spec.SecureName, spec.Primitive);
			Singles.EmitOrdering(members, spec.SecureName, spec.Primitive);
			Singles.EmitDateArithmetic(members, spec.SecureName, spec.Primitive);
			break;
		case WrapperKind.BigInteger:
			Numerics.EmitBigInteger(members, spec.SecureName);
			break;
		case WrapperKind.Complex:
			Numerics.EmitComplex(members, spec.SecureName, emitReverse: true);
			break;
		case WrapperKind.Numerics:
			switch (spec.Primitive)
			{
				case "Vector2":
				case "Vector3":
				case "Vector4":
					Numerics.EmitSelfForms(
						members,
						spec.SecureName,
						spec.Primitive,
						new[] { "==", "!=", "+", "-", "*", "/" },
						new[] { "*", "/" }
					);
					break;
				case "Matrix3x2":
					Numerics.EmitSelfForms(
						members,
						spec.SecureName,
						spec.Primitive,
						new[] { "==", "!=", "+", "-", "*" },
						Array.Empty<string>()
					);
					break;
				case "Matrix4x4":
					Numerics.EmitSelfForms(
						members,
						spec.SecureName,
						spec.Primitive,
						new[] { "==", "!=", "*" },
						Array.Empty<string>()
					);
					break;
				case "Quaternion":
					Numerics.EmitSelfForms(
						members,
						spec.SecureName,
						spec.Primitive,
						new[] { "==", "!=", "*", "/" },
						Array.Empty<string>()
					);
					break;
				default: // Plane: equality only.
					Numerics.EmitSelfForms(
						members,
						spec.SecureName,
						spec.Primitive,
						new[] { "==", "!=" },
						Array.Empty<string>()
					);
					break;
			}

			break;
		case WrapperKind.Unity:
			Unity.EmitUnityTwins(members, spec.SecureName, spec.Primitive);
			break;
		default:
			continue;
	}

	Interfaces.EmitFor(members, spec);
	Parsable.EmitFor(members, spec);
	GenWriter.WriteWrapper(repoRoot, spec, members.ToString(), checkOnly, ref changed);
}

Console.WriteLine(
	checkOnly ? $"codegen check: {changed} drifted file(s)" : $"codegen: {changed} file(s) written"
);
return checkOnly && changed != 0 ? 1 : 0;

static string FindRepoRoot(string start)
{
	string? dir = start;
	while (dir is not null)
	{
		if (File.Exists(Path.Combine(dir, "SecureValue.slnx")))
		{
			return dir;
		}

		dir = Path.GetDirectoryName(dir);
	}

	throw new InvalidOperationException(
		$"Could not locate SecureValue.slnx upward from '{start}'."
	);
}
