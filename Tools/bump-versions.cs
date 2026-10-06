//:property TargetFramework=net10.0

// Sets the release version in both versioned artifacts:
//   - SecureValue.Unity/package.json "version"
//   - SecureValue/SecureValue.csproj <Version>
// Formatting-preserving in both files (verified byte-identical round-trip
// when the version is unchanged). The root package.json is private release
// tooling (0.0.0) and stays as is. The csproj edit is a targeted single-line
// replacement (no XML rewrite), so file headers/whitespace never churn.
//
// Run: dotnet run Tools/bump-versions.cs -- <version>
//   (called by semantic-release exec prepare; see .releaserc.json. The git
//   plugin commits both files back under the same [skip ci] release commit.)

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
	Console.Error.WriteLine("Usage: dotnet run Tools/bump-versions.cs -- <version>");
	return 2;
}

string version = args[0].Trim();
string repoRoot = FindRepoRoot(Environment.CurrentDirectory);

string packageJson = Path.Combine(repoRoot, "SecureValue.Unity", "package.json");
JsonObject root = JsonNode.Parse(File.ReadAllText(packageJson))!.AsObject();
string? currentJson = root["version"]?.GetValue<string>();
if (currentJson == version)
{
	Console.WriteLine($"package.json already at {version}");
}
else
{
	root["version"] = version;
	File.WriteAllText(
		packageJson,
		root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n"
	);
	Console.WriteLine($"package.json version {currentJson} -> {version}");
}

string csproj = Path.Combine(repoRoot, "SecureValue", "SecureValue.csproj");

// Preserve the file's existing encoding (BOM or not): a naive
// ReadAllText/WriteAllText round-trip would silently strip a BOM.
byte[] csprojRaw = File.ReadAllBytes(csproj);
bool csprojBom =
	csprojRaw.Length >= 3 && csprojRaw[0] == 0xEF && csprojRaw[1] == 0xBB && csprojRaw[2] == 0xBF;
Encoding csprojEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: csprojBom);

// NOTE: GetString does NOT consume the BOM preamble — it decodes it into a
// U+FEFF character, which would double the BOM on write. Skip it explicitly.
string csprojText = csprojBom
	? Encoding.UTF8.GetString(csprojRaw, 3, csprojRaw.Length - 3)
	: Encoding.UTF8.GetString(csprojRaw);
Match m = Regex.Match(csprojText, @"<Version>[^<]*</Version>");
if (!m.Success)
{
	Console.Error.WriteLine($"No <Version> element found in {csproj}");
	return 1;
}
string currentCsproj = m.Value["<Version>".Length..^"</Version>".Length];
if (currentCsproj == version)
{
	Console.WriteLine($"SecureValue.csproj already at {version}");
}
else
{
	string updated =
		csprojText.Substring(0, m.Index)
		+ $"<Version>{version}</Version>"
		+ csprojText.Substring(m.Index + m.Length);
	File.WriteAllText(csproj, updated, csprojEncoding);
	Console.WriteLine($"SecureValue.csproj version {currentCsproj} -> {version}");
}

return 0;

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
		$"Could not locate SecureValue.slnx upward from '{start}'. Run from inside the repo."
	);
}
