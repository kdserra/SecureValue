//:property TargetFramework=net10.0

// Packs SecureValue.Unity/ into dist/SecureValue.unitypackage for GitHub Releases:
// a .unitypackage is a gzipped tar archive where every asset lives in a
// top-level directory named after its Unity GUID, holding 'asset' (raw file
// bytes), 'asset.meta' (raw .meta bytes) and 'pathname' (forward-slashed Unity
// path, e.g. "Assets/SecureValue/Runtime/..."). No Unity Editor needed — the
// GUIDs come from the committed .meta files.
//
// FILES AND FOLDERS — matching Unity's own exporter byte-for-byte: file entries
// hold `asset` + `asset.meta` + `pathname`; folder entries hold `asset.meta` +
// `pathname` with NO `asset` file. (An empty `asset` on a folder entry makes the
// importer attempt a file copy onto the directory path — "Failed to copy package
// file to ..." with no diagnostics. Verified against an authentic Unity export.)
// Unity rebuilds intermediate directories from file pathnames regardless;
// shipping the folder entries preserves their GUIDs. Folder GUIDs are not
// referenced by anything in this package (asmdefs link by assembly name).
//
// Content mirrors the UPM install 1:1 (package.json, README, Runtime/**,
// Tests/**) so both install methods deliver identical files. Every non-meta,
// non-dotfile MUST have a sibling .meta with a valid guid; anything else is a
// hard error (a silent skip would ship a package Unity imports with fresh,
// reference-breaking GUIDs). Output is deterministic: entries sorted by guid,
// fixed mtime/uid/gid/mode, plain ustar (Pax extended headers would embed the
// writer PID).
//
// Run: dotnet run Tools/pack-unitypackage.cs [-- <outputFileName>]
//   (called by semantic-release exec prepare AFTER bump-versions, so the
//   embedded package.json already carries the release version; see
//   .releaserc.json. Re-runs Unity staging first as a freshness safety net.
//   The optional file name (default SecureValue.unitypackage) is for local
//   testing — e.g. while Unity holds the real output open during an import.)

using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length > 1 || (args.Length == 1 && string.IsNullOrWhiteSpace(args[0])))
{
	Console.Error.WriteLine("Usage: dotnet run Tools/pack-unitypackage.cs [-- <outputFileName>]");
	return 2;
}

string outputName = args.Length == 1 ? args[0].Trim() : "SecureValue.unitypackage";

string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
string unityDir = Path.Combine(repoRoot, "SecureValue.Unity");
string distDir = Path.Combine(repoRoot, "dist");
Directory.CreateDirectory(distDir);
string outputPath = Path.Combine(distDir, outputName);

if (
	Run("dotnet", $"run \"{Path.Combine(repoRoot, "Tools", "build-unity-package.cs")}\"", repoRoot)
	!= 0
)
{
	Console.Error.WriteLine("Unity staging failed; refusing to pack.");
	return 1;
}

const string installRoot = "Assets/SecureValue";
var entries = new SortedDictionary<string, (string Kind, string Source, string Target)>(
	StringComparer.Ordinal
);

foreach (string dir in Directory.EnumerateDirectories(unityDir, "*", SearchOption.AllDirectories))
{
	string rel = Path.GetRelativePath(unityDir, dir);
	if (rel.Split(Path.DirectorySeparatorChar).Any(p => p.StartsWith('.')))
	{
		Console.WriteLine($"::warning::skipping dot-directory {rel}");
		continue;
	}
	string meta = dir + ".meta";
	if (!File.Exists(meta))
	{
		continue;
	}
	string guid = ReadGuid(meta);
	entries.Add(guid, ("dir", dir, $"{installRoot}/{ToPosix(rel)}"));
}

foreach (string file in Directory.EnumerateFiles(unityDir, "*", SearchOption.AllDirectories))
{
	string name = Path.GetFileName(file);
	if (name.StartsWith('.'))
	{
		Console.WriteLine($"::warning::skipping dotfile {Path.GetRelativePath(unityDir, file)}");
		continue;
	}
	if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
	{
		continue;
	}
	string rel = Path.GetRelativePath(unityDir, file);
	if (rel.Split(Path.DirectorySeparatorChar).Any(p => p.StartsWith('.')))
	{
		Console.WriteLine($"::warning::skipping dot-path file {rel}");
		continue;
	}
	string meta = file + ".meta";
	if (!File.Exists(meta))
	{
		Console.Error.WriteLine($"::error::no .meta for {rel}; refusing to pack.");
		return 1;
	}
	string guid = ReadGuid(meta);
	if (!entries.TryAdd(guid, ("file", file, $"{installRoot}/{ToPosix(rel)}")))
	{
		Console.Error.WriteLine($"::error::duplicate GUID {guid} ({rel}); refusing to pack.");
		return 1;
	}
}

var epoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
if (File.Exists(outputPath))
{
	File.Delete(outputPath);
}
using (var fileStream = File.Create(outputPath))
using (var gzip = new GZipStream(fileStream, CompressionLevel.Optimal))
using (var tar = new TarWriter(gzip, leaveOpen: false))
{
	foreach ((string guid, (string kind, string source, string target)) in entries)
	{
		var dirEntry = new UstarTarEntry(TarEntryType.Directory, $"{guid}/");
		Stamp(dirEntry, epoch);
		tar.WriteEntry(dirEntry);
		if (kind == "file")
		{
			WriteBytesEntry(tar, $"{guid}/asset", File.ReadAllBytes(source), epoch);
		}

		WriteBytesEntry(tar, $"{guid}/asset.meta", File.ReadAllBytes(source + ".meta"), epoch);
		WriteBytesEntry(tar, $"{guid}/pathname", Encoding.UTF8.GetBytes(target), epoch);
	}
}

var info = new FileInfo(outputPath);
Console.WriteLine($"packed {entries.Count} entries -> {outputPath} ({info.Length} bytes)");
return 0;

static string ReadGuid(string metaPath)
{
	foreach (string line in File.ReadLines(metaPath, Encoding.UTF8).Take(5))
	{
		Match m = Regex.Match(line, @"guid:\s*([0-9a-fA-F]{32})");
		if (m.Success)
		{
			return m.Groups[1].Value.ToLowerInvariant();
		}
	}
	Console.Error.WriteLine($"::error::no valid guid in {metaPath}; refusing to pack.");
	Environment.Exit(1);
	throw new UnreachableException();
}

static string ToPosix(string rel) => rel.Replace(Path.DirectorySeparatorChar, '/');

static void Stamp(TarEntry entry, DateTimeOffset mtime)
{
	entry.ModificationTime = mtime;
	entry.Uid = 0;
	entry.Gid = 0;
}

static void WriteBytesEntry(TarWriter tar, string name, byte[] bytes, DateTimeOffset mtime)
{
	var entry = new UstarTarEntry(TarEntryType.RegularFile, name);
	Stamp(entry, mtime);
	entry.Mode =
		UnixFileMode.UserRead
		| UnixFileMode.UserWrite
		| UnixFileMode.GroupRead
		| UnixFileMode.OtherRead;
	entry.DataStream = new MemoryStream(bytes, writable: false);
	tar.WriteEntry(entry);
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
