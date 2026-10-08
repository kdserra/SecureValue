//:property TargetFramework=net10.0

// Converts the root README.md (GitHub-flavored: centered-HTML header + bare
// <a id> section anchors) to pure markdown for renderers without HTML support
// (NuGet, Unity Package Manager / .unitypackage import).
//
// Hardcoded for the exact HTML the root README uses — NOT a general HTML to
// markdown converter:
//   - centered icon <img>            ->  dropped (an uncentered markdown image
//     renders as an ugly left-aligned block in NuGet/Unity)
//   - centered <h1> title            ->  # title
//   - centered plain-text <p>        ->  bare text line
//   - badge <a><img></a>             ->  [![alt](src)](href)
//   - nav <a href="#x">Text</a>      ->  [Text](#x) (separators untouched)
//   - bare <a id="..."></a> lines   ->  deleted (all renderers auto-anchor ##)
//   - leftover <p>/<h1> tags         ->  stripped
// Fenced code blocks are never touched (C# generics like List<SecureInt>
// look like tags but are code). The root README is the source of truth and is
// never modified in place by CI — callers copy it to the destination first,
// then run this on the copy. Idempotent: a converted file converts to itself.
//
// Run: dotnet run Tools/fix-readme-formatting.cs -- <input> <output>
//   (pass the same path twice for in-place conversion)

using System.Text;
using System.Text.RegularExpressions;

if (args.Length != 2 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
{
	Console.Error.WriteLine("Usage: dotnet run Tools/fix-readme-formatting.cs -- <input> <output>");
	return 2;
}

string input = args[0].Trim();
string output = args[1].Trim();
if (!File.Exists(input))
{
	Console.Error.WriteLine($"Input file not found: {input}");
	return 1;
}

// Preserve the file's existing encoding (BOM or not): a naive
// ReadAllText/WriteAllText round-trip would silently strip a BOM.
byte[] raw = File.ReadAllBytes(input);
bool bom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: bom);

// NOTE: GetString does NOT consume the BOM preamble — it decodes it into a
// U+FEFF character, which would double the BOM on write. Skip it explicitly.
string text = bom ? Encoding.UTF8.GetString(raw, 3, raw.Length - 3) : Encoding.UTF8.GetString(raw);

// Preserve the file's newline style (git normalizes .md to LF, but a local
// edit may have introduced CRLF — don't reformat what we weren't asked to).
string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

int anchorsRemoved = 0;
int badgesConverted = 0;
int linksConverted = 0;
int imagesConverted = 0;
int imagesRemoved = 0;
int tagsStripped = 0;

var converted = new List<string>(lines.Length);
bool inFence = false;
bool paraTrim = false;
foreach (string line in lines)
{
	// Fence tracking comes first: code content (List<SecureInt>, Span<char>,
	// shifting operators) must pass through byte-identical.
	if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
	{
		inFence = !inFence;
		converted.Add(line);
		continue;
	}

	if (inFence)
	{
		converted.Add(line);
		continue;
	}

	// Bare section anchors: whole-line <a id="..."></a> carries no content.
	if (Regex.IsMatch(line, @"^\s*<a\s+id=""[^""]*""\s*>\s*</a>\s*$"))
	{
		anchorsRemoved++;
		continue;
	}

	string current = line;

	// Centered title: <h1 ...>text</h1> -> # text (runs before tag stripping).
	current = Regex.Replace(
		current,
		@"<h1\b[^>]*>(.*?)</h1>",
		m => "# " + m.Groups[1].Value.Trim(),
		RegexOptions.IgnoreCase
	);

	// Badges: <a href="H"><img ... /></a> -> [![alt](src)](H).
	// Runs before the plain-link rule (which would otherwise eat the <a>).
	current = Regex.Replace(
		current,
		@"<a\s+href=""(?<href>[^""]+)""[^>]*>\s*<img(?<attrs>[^>]*?)/?>\s*</a>",
		m =>
		{
			badgesConverted++;
			(string src, string alt) = SrcAlt(m.Groups["attrs"].Value);
			return $"[![{alt}]({src})]({m.Groups["href"].Value})";
		},
		RegexOptions.IgnoreCase
	);

	// Plain links: <a href="H">text</a> -> [text](H).
	current = Regex.Replace(
		current,
		@"<a\s+href=""(?<href>[^""]+)""[^>]*>(?<linkText>.*?)</a>",
		m =>
		{
			linksConverted++;
			return $"[{m.Groups["linkText"].Value}]({m.Groups["href"].Value})";
		},
		RegexOptions.IgnoreCase
	);

	// Standalone icon image: a whole-line <img> is the centered layout icon —
	// drop the line (badges above already ran, so only the icon can match).
	if (Regex.IsMatch(current, @"^\s*<img\b[^>]*>\s*$", RegexOptions.IgnoreCase))
	{
		imagesRemoved++;
		continue;
	}

	// Inline images: <img ... /> -> ![alt](src), whatever the attr order.
	current = Regex.Replace(
		current,
		@"<img(?<attrs>[^>]*?)/?>",
		m =>
		{
			(string src, string alt) = SrcAlt(m.Groups["attrs"].Value);
			if (string.IsNullOrEmpty(src))
			{
				Console.WriteLine($"::warning::img without src left as-is: {m.Value}");
				return m.Value;
			}

			imagesConverted++;
			return $"![{alt}]({src})";
		},
		RegexOptions.IgnoreCase
	);

	// Leftover centered-paragraph / heading tags. Whole-line tags drop the
	// line (they were layout only); inline remnants are stripped in place.
	Match wholeTag = Regex.Match(
		current,
		@"^\s*</?(?<tag>p|h1)\b[^>]*>\s*$",
		RegexOptions.IgnoreCase
	);
	if (wholeTag.Success)
	{
		tagsStripped++;
		// Inside a dropped <p> block, the enclosed prose lines carry the
		// block's layout indent — strip it until the closing </p> drops.
		paraTrim =
			wholeTag.Groups["tag"].Value.Equals("p", StringComparison.OrdinalIgnoreCase)
			&& !wholeTag.Value.Contains('/', StringComparison.Ordinal);
		continue;
	}

	string stripped = Regex.Replace(
		current,
		@"</?(p|h1)\b[^>]*>",
		string.Empty,
		RegexOptions.IgnoreCase
	);
	if (!stripped.Equals(current, StringComparison.Ordinal))
	{
		tagsStripped++;
		current = stripped;
	}

	// A line that opened with a tag was HTML layout: its leading indent was
	// inside the <p> block, not markdown structure. Drop it so converted
	// lines start at column 0 (untouched prose/code keeps its indent).
	if (!current.Equals(line, StringComparison.Ordinal) && Regex.IsMatch(line, @"^\s*<"))
	{
		current = current.TrimStart();
	}

	if (paraTrim && current.Length > 0)
	{
		current = current.TrimStart();
	}

	converted.Add(current);
}

string result = string.Join("\n", converted);

// Collapse runs of 2+ blank lines left behind by dropped tag lines.
result = Regex.Replace(result, @"\n{3,}", "\n\n");
if (newline != "\n")
{
	result = result.Replace("\n", newline, StringComparison.Ordinal);
}

File.WriteAllText(output, result, encoding);
Console.WriteLine(
	$"converted {input} -> {output} "
		+ $"(anchors:{anchorsRemoved} badges:{badgesConverted} links:{linksConverted} images:{imagesConverted} iconsRemoved:{imagesRemoved} tags:{tagsStripped})"
);
return 0;

static (string Src, string Alt) SrcAlt(string attrs)
{
	string src = Regex
		.Match(attrs, @"src=""(?<src>[^""]*)""", RegexOptions.IgnoreCase)
		.Groups["src"]
		.Value;
	string alt = Regex
		.Match(attrs, @"alt=""(?<alt>[^""]*)""", RegexOptions.IgnoreCase)
		.Groups["alt"]
		.Value;
	return (src, alt);
}
