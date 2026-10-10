//:property TargetFramework=net10.0

// Keeps the Unity UPM version-pinning example in the root README.md pointed at
// the latest release: rewrites ONLY the pin-example line
//   To pin a version, append the version tag, ex: **`#vX.Y.Z`**
// leaving every other line (including any historical version mentions)
// byte-identical. The staged SecureValue.Unity/README.md copy picks the new
// pin up on the next Unity staging run. Fails loudly when the anchor line is
// missing or the version is not plain X.Y.Z (this repo releases from master
// only, so prerelease suffixes are rejected rather than guessed at).
//
// Run: dotnet run Tools/sync-readme-version.cs -- <version>
//   (called by semantic-release exec prepare; see .releaserc.json. The git
//   plugin commits README.md back under the same [skip ci] release commit.)

using System.Text;
using System.Text.RegularExpressions;

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
	Console.Error.WriteLine("Usage: dotnet run Tools/sync-readme-version.cs -- <version>");
	return 2;
}

string version = args[0].Trim();
if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
{
	Console.Error.WriteLine($"Refusing non-plain version '{version}': expected X.Y.Z.");
	return 1;
}

string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
string readme = Path.Combine(repoRoot, "README.md");

// Preserve the file's existing encoding (BOM or not): a naive
// ReadAllText/WriteAllText round-trip would silently strip a BOM.
byte[] raw = File.ReadAllBytes(readme);
bool bom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: bom);

// NOTE: GetString does NOT consume the BOM preamble — it decodes it into a
// U+FEFF character, which would double the BOM on write. Skip it explicitly.
string text = bom ? Encoding.UTF8.GetString(raw, 3, raw.Length - 3) : Encoding.UTF8.GetString(raw);

// Match on LF-normalized text (Windows checkouts carry CRLF, which would
// defeat the `$` anchor); restore the original newlines before writing so a
// no-op run stays byte-identical.
string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
string lf = text.Replace("\r\n", "\n", StringComparison.Ordinal);

string pattern = @"^(To pin a version, append the version tag, ex: \*\*`#)v\d+\.\d+\.\d+(`\*\*)$";
MatchCollection hits = Regex.Matches(lf, pattern, RegexOptions.Multiline);
if (hits.Count != 1)
{
	Console.Error.WriteLine(
		hits.Count == 0
			? "Pin-example anchor line not found in README.md; refusing to guess."
			: $"Pin-example anchor matched {hits.Count} lines; expected exactly 1."
	);
	return 1;
}

string updated = Regex
	.Replace(lf, pattern, "${1}v" + version + "${2}", RegexOptions.Multiline)
	.Replace("\n", newline, StringComparison.Ordinal);
if (updated == text)
{
	Console.WriteLine($"README pin example already at #v{version}");
	return 0;
}

File.WriteAllText(readme, updated, encoding);
Console.WriteLine($"README pin example -> #v{version}");
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
