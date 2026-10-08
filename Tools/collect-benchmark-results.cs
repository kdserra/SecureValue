//:property TargetFramework=net10.0

// Runs a BenchmarkDotNet suite and assembles the per-class GitHub reports
// into ONE markdown file for the GitHub Release:
//   dist/Benchmark-IntFloatString.md  (curated int+float+string trio — fast)
//   dist/Benchmark-All.md             (every benchmark class — slow)
// The .benchmarks.yml workflow uploads each file the moment it is assembled,
// so the fast file is attached before the slow run even starts. Filenames
// carry no version — the Release they attach to IS the version.
//
// Run: dotnet run Tools/collect-benchmark-results.cs -- <version> <suite>
//   <suite>: IntFloatString | All
//   Optional 3rd arg "short": append --job short (local smoke tests only;
//   CI runs the default job so published numbers stay full-quality).
//   (Called by .github/workflows/benchmarks.yml, NOT by semantic-release.)

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

if (
	args.Length < 2
	|| args.Length > 3
	|| string.IsNullOrWhiteSpace(args[0])
	|| string.IsNullOrWhiteSpace(args[1])
)
{
	Console.Error.WriteLine(
		"Usage: dotnet run Tools/collect-benchmark-results.cs -- <version> <suite> [short]"
	);
	Console.Error.WriteLine("  <suite>: IntFloatString | All");
	return 2;
}

string version = args[0].Trim();
string suite = args[1].Trim();
bool shortJob =
	args.Length == 3 && args[2].Trim().Equals("short", StringComparison.OrdinalIgnoreCase);
if (args.Length == 3 && !shortJob)
{
	Console.Error.WriteLine("Third argument, if given, must be \"short\".");
	return 2;
}

(string Filter, string FileName) = suite switch
{
	"IntFloatString" => ("*IntFloatStringBenchmarks*", "Benchmark-IntFloatString.md"),
	"All" => ("*Benchmarks*", "Benchmark-All.md"),
	_ => throw new InvalidOperationException(
		$"Unknown suite '{suite}'. Expected IntFloatString or All."
	),
};

string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
string distDir = Path.Combine(repoRoot, "dist");
Directory.CreateDirectory(distDir);
string outputPath = Path.Combine(distDir, FileName);
if (File.Exists(outputPath))
{
	File.Delete(outputPath);
}

string scratchDir = Path.Combine(distDir, $"tmp-benchmarks-{suite}");
if (Directory.Exists(scratchDir))
{
	Directory.Delete(scratchDir, recursive: true);
}

Console.WriteLine(
	$"running {suite} benchmarks (filter {Filter}{(shortJob ? ", short job" : string.Empty)})..."
);
string bdnArgs =
	$"run --project \"{Path.Combine(repoRoot, "SecureValue.Benchmarks", "SecureValue.Benchmarks.csproj")}\""
	+ $" -c Release -- --filter {Filter} --artifacts \"{scratchDir}\""
	+ (shortJob ? " --job short" : string.Empty);
if (Run("dotnet", bdnArgs, repoRoot) != 0)
{
	Console.Error.WriteLine($"{suite} benchmarks failed; no results file written.");
	return 1;
}

string resultsDir = Path.Combine(scratchDir, "results");
string[] reports = Directory.Exists(resultsDir)
	? Directory
		.GetFiles(resultsDir, "*-report-github.md")
		.OrderBy(p => p, StringComparer.Ordinal)
		.ToArray()
	: [];
if (reports.Length == 0)
{
	Console.Error.WriteLine(
		$"No *-report-github.md files found in {resultsDir}; refusing to publish partial numbers."
	);
	return 1;
}

string commit = GitSha(repoRoot);
string bdnVersion = BenchmarkDotNetVersion(repoRoot);
var body = new StringBuilder();
body.AppendLine($"# SecureValue Benchmarks — {version} ({suite})");
body.AppendLine();
body.AppendLine($"- Version: {version}");
body.AppendLine($"- Commit: {commit}");
body.AppendLine($"- Date (UTC): {DateTime.UtcNow:yyyy-MM-dd}");
body.AppendLine($"- BenchmarkDotNet: {bdnVersion}");
body.AppendLine($"- Target framework: net10.0, configuration Release");
body.AppendLine(
	$"- Filter: `{Filter}`{(shortJob ? " (`--job short` smoke run — NOT release quality)" : string.Empty)}"
);
body.AppendLine();
body.AppendLine(
	"> These numbers are indicative, not guarantees: CI runners are shared and "
		+ "noisy. Compare runs from the same runner, and always measure the release "
		+ "build on your own shipping hardware."
);
foreach (string report in reports)
{
	string name = Path.GetFileName(report);
	const string prefix = "SecureValue.Benchmarks.";
	const string suffix = "-report-github.md";
	string className =
		name.StartsWith(prefix, StringComparison.Ordinal)
		&& name.EndsWith(suffix, StringComparison.Ordinal)
			? name[prefix.Length..^suffix.Length]
			: Path.GetFileNameWithoutExtension(report);
	body.AppendLine();
	body.AppendLine($"## {className}");
	body.AppendLine();
	body.Append(File.ReadAllText(report).Trim());
	body.AppendLine();
}

File.WriteAllText(
	outputPath,
	body.ToString(),
	new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
);
Directory.Delete(scratchDir, recursive: true);
Console.WriteLine($"wrote {outputPath} ({reports.Length} class reports)");
return 0;

static string GitSha(string workDir)
{
	try
	{
		var psi = new ProcessStartInfo("git", "rev-parse HEAD")
		{
			RedirectStandardOutput = true,
			UseShellExecute = false,
			WorkingDirectory = workDir,
		};
		using var proc = Process.Start(psi)!;
		string sha = proc.StandardOutput.ReadToEnd().Trim();
		proc.WaitForExit();
		return proc.ExitCode == 0 && sha.Length >= 7 ? sha : "unknown";
	}
	catch
	{
		return "unknown";
	}
}

static string BenchmarkDotNetVersion(string repoRoot)
{
	string csproj = Path.Combine(
		repoRoot,
		"SecureValue.Benchmarks",
		"SecureValue.Benchmarks.csproj"
	);
	Match m = Regex.Match(
		File.ReadAllText(csproj),
		@"<PackageReference\s+Include=""BenchmarkDotNet""\s+Version=""(?<v>[^""]+)"""
	);
	return m.Success ? m.Groups["v"].Value : "unknown";
}

static int Run(string cmd, string arguments, string workDir)
{
	var psi = new ProcessStartInfo(cmd, arguments)
	{
		UseShellExecute = false,
		WorkingDirectory = workDir,
	};
	using var proc = Process.Start(psi)!;
	proc.WaitForExit();
	return proc.ExitCode;
}

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
