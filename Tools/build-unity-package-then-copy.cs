//:property TargetFramework=net10.0

// Local-iteration helper: rebuilds the Unity package staging, then replaces an
// outdated installed copy with the fresh package. Runs
// Tools/build-unity-package.cs, deletes <targetDir>, and copies the repo's
// SecureValue.Unity folder to <targetDir>. One command instead of
// delete / rebuild / copy by hand (e.g. refreshing a sandbox Assets copy).
//
// Run: dotnet run Tools/build-unity-package-then-copy.cs -- <targetDir>
//   <targetDir>  path of the outdated installed copy to replace.
//                It is deleted first (its parent must already exist), then the
//                fresh SecureValue.Unity package is copied in its place.

using System.Diagnostics;

if (
	args.Length != 1
	|| string.IsNullOrWhiteSpace(args[0])
	|| args[0] == "--help"
	|| args[0] == "-h"
)
{
	Console.Error.WriteLine(
		"Usage: dotnet run Tools/build-unity-package-then-copy.cs -- <targetDir>"
	);
	Console.Error.WriteLine("  <targetDir>  path of the outdated installed copy to replace.");
	Console.Error.WriteLine("               It is deleted first, then the fresh SecureValue.Unity");
	Console.Error.WriteLine("               package is copied in its place.");
	return 2;
}

string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
string source = Path.GetFullPath(Path.Combine(repoRoot, "SecureValue.Unity"));
string target = Path.GetFullPath(args[0].Trim())
	.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

// Destructive-command guards: never delete a drive root, and never delete the
// package source we are about to copy from (the target must be neither the
// source itself, inside it, nor one of its ancestors — deleting the repo root
// would wipe the source too).
string? targetRoot = Path.GetPathRoot(target);
if (target.Length <= (targetRoot?.Length ?? 0))
{
	Console.Error.WriteLine($"Refusing to delete a drive root: {target}");
	return 1;
}
if (IsSameOrInside(target, source) || IsSameOrInside(source, target))
{
	Console.Error.WriteLine($"Refusing: target must not be the package source or overlap it.");
	Console.Error.WriteLine($"  source: {source}");
	Console.Error.WriteLine($"  target: {target}");
	return 1;
}
if (!Directory.Exists(source))
{
	Console.Error.WriteLine($"Package source not found: {source}");
	return 1;
}
string? parent = Path.GetDirectoryName(target);
if (parent is null || !Directory.Exists(parent))
{
	Console.Error.WriteLine($"Target parent directory does not exist: {parent}");
	return 1;
}

// Stage first so a staging failure leaves the installed copy untouched.
if (
	Run("dotnet", $"run \"{Path.Combine(repoRoot, "Tools", "build-unity-package.cs")}\"", repoRoot)
	!= 0
)
{
	Console.Error.WriteLine("Unity package staging failed; target left untouched.");
	return 1;
}

if (File.Exists(target) && !Directory.Exists(target))
{
	File.SetAttributes(target, FileAttributes.Normal);
	File.Delete(target);
	Console.WriteLine($"removed stale file {target}");
}
else if (Directory.Exists(target))
{
	foreach (string dir in Directory.EnumerateDirectories(target, "*", SearchOption.AllDirectories))
	{
		File.SetAttributes(dir, FileAttributes.Normal);
	}
	foreach (string file in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories))
	{
		File.SetAttributes(file, FileAttributes.Normal);
	}
	File.SetAttributes(target, FileAttributes.Normal);
	Directory.Delete(target, recursive: true);
	Console.WriteLine($"removed outdated {target}");
}

int copied = 0;
foreach (string dir in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
{
	Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, dir)));
}
foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
{
	string dest = Path.Combine(target, Path.GetRelativePath(source, file));
	Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
	File.Copy(file, dest, overwrite: true);
	copied++;
}

Console.WriteLine($"copied {copied} files to {target}");
return 0;

static bool IsSameOrInside(string path, string dir) =>
	path.Equals(dir, StringComparison.OrdinalIgnoreCase)
	|| path.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

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
