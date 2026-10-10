#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// EditMode mirror of the xUnit SerializationCorpusTests: the unified
	/// serialization-corpus.json fixture drives both checks — the generation
	/// check round-trips each entry's Value through the current
	/// SaveToSerialized/LoadFromSerialized bridge, and the validation check
	/// reloads each entry's historical Serialized payload.
	/// Unity has no System.Text.Json dependency, so a small dependency-free
	/// JSON reader below parses the fixtures instead. Corpus members that only
	/// exist as computed properties in the fixtures (Complex Magnitude/Phase,
	/// matrix IsIdentity/Translation/row views, Rune derived flags) are ignored;
	/// only the round-trippable members are read. Wrapper types that Unity
	/// builds exclude (Rune, DateOnly, TimeOnly) are skipped; any other
	/// unresolvable wrapper fails the run so staging regressions cannot hide
	/// behind skips.
	/// </summary>
	public class SerializationCorpusTests
	{
		// Wrappers excluded from Unity/netstandard2.1 builds (see README
		// compatibility notes). Every other corpus wrapper must resolve.
		private static readonly HashSet<string> UnityExcludedWrappers = new HashSet<string>(
			new[]
			{
				"SecureValue.SecureRune",
				"SecureValue.SecureDateOnly",
				"SecureValue.SecureTimeOnly",
			}
		);

		private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

		[Test]
		public void GenerationCorpus_CurrentSerializationRoundTrips()
		{
			JsonNode root = Load("serialization-corpus.json");
			JsonNode valuesByType = RequireProperty(root, "Values");
			int count = Limit(RequireProperty(root, "ValuesPerType"));
			int checks = 0;
			int failures = 0;
			var log = new StringBuilder();
			var skipped = new List<string>();
			foreach (KeyValuePair<string, JsonNode> typeValues in valuesByType.ObjectEntries())
			{
				if (CheckExcluded(typeValues.Key, skipped))
					continue;
				Type wrapperType;
				Type valueType;
				try
				{
					Resolve(typeValues.Key, out wrapperType, out valueType);
				}
				catch (Exception ex)
				{
					Note(ref failures, log, $"[Generation] {typeValues.Key}: {ex.Message}.");
					continue;
				}
				int index = 0;
				foreach (JsonNode entry in typeValues.Value.ArrayItems())
				{
					if (index++ == count)
						break;
					checks++;
					try
					{
						object value = ReadValue(RequireProperty(entry, "Value"), valueType);
						object wrapper = Activator.CreateInstance(wrapperType, value);
						uint[] serialized = (
							(SecureValue.ISecureSerialization)wrapper
						).SaveToSerialized();
						object restored = Activator.CreateInstance(wrapperType);
						((SecureValue.ISecureSerialization)restored).LoadFromSerialized(serialized);
						object actual = Decrypted(restored, wrapperType);
						if (!object.Equals(value, actual))
						{
							Note(
								ref failures,
								log,
								$"[Generation] {typeValues.Key}[{index - 1}] mismatch: expected {value}, got {actual}."
							);
						}
					}
					catch (Exception ex)
					{
						Note(
							ref failures,
							log,
							$"[Generation] {typeValues.Key}[{index - 1}] threw {ex.GetType().Name}: {ex.Message}."
						);
					}
				}
			}
			Report("Generation", checks, failures, log, skipped);
		}

		[Test]
		public void ValidationCorpus_HistoricalSerializationStillLoads()
		{
			JsonNode root = Load("serialization-corpus.json");
			JsonNode valuesByType = RequireProperty(root, "Values");
			int count = Limit(RequireProperty(root, "ValuesPerType"));
			int checks = 0;
			int failures = 0;
			var log = new StringBuilder();
			var skipped = new List<string>();
			foreach (KeyValuePair<string, JsonNode> typeEntries in valuesByType.ObjectEntries())
			{
				if (CheckExcluded(typeEntries.Key, skipped))
					continue;
				Type wrapperType;
				Type valueType;
				try
				{
					Resolve(typeEntries.Key, out wrapperType, out valueType);
				}
				catch (Exception ex)
				{
					Note(ref failures, log, $"[Validation] {typeEntries.Key}: {ex.Message}.");
					continue;
				}
				int index = 0;
				foreach (JsonNode entry in typeEntries.Value.ArrayItems())
				{
					if (index++ == count)
						break;
					checks++;
					try
					{
						object expected = ReadValue(RequireProperty(entry, "Value"), valueType);
						uint[] serialized = ReadUIntArray(RequireProperty(entry, "Serialized"));
						object restored = Activator.CreateInstance(wrapperType);
						((SecureValue.ISecureSerialization)restored).LoadFromSerialized(serialized);
						object actual = Decrypted(restored, wrapperType);
						if (!object.Equals(expected, actual))
						{
							Note(
								ref failures,
								log,
								$"[Validation] {typeEntries.Key}[{index - 1}] mismatch: expected {expected}, got {actual}."
							);
						}
					}
					catch (Exception ex)
					{
						Note(
							ref failures,
							log,
							$"[Validation] {typeEntries.Key}[{index - 1}] threw {ex.GetType().Name}: {ex.Message}."
						);
					}
				}
			}
			Report("Validation", checks, failures, log, skipped);
		}

		private static void Report(
			string corpus,
			int checks,
			int failures,
			StringBuilder log,
			List<string> skipped
		)
		{
			if (failures > 0)
				Assert.Fail($"{corpus} corpus: {failures}/{checks} values deviated:\n{log}");
			string skipNote =
				skipped.Count == 0 ? "no skips" : $"skipped: {string.Join(", ", skipped)}";
			Assert.Pass($"{corpus} corpus: {checks}/{checks} values match ({skipNote}).");
		}

		private static void Note(ref int failures, StringBuilder log, string message)
		{
			failures++;
			if (log.Length < 8000)
				log.AppendLine(message);
		}

		private static JsonNode Load(string fileName)
		{
			string path = LocateCorpus(fileName);
			Assert.IsTrue(File.Exists(path), $"Missing serialization corpus: {path}");
			return JsonParser.Parse(File.ReadAllText(path));
		}

		private static int Limit(JsonNode valuesPerType)
		{
			int corpusCount = int.Parse(valuesPerType.AsNumber(), Invariant);
			string setting = Environment.GetEnvironmentVariable("SECUREVALUE_SERIALIZATION_COUNT");
			return int.TryParse(setting, out int limit) && limit > 0
				? Math.Min(limit, corpusCount)
				: corpusCount;
		}

		private static bool CheckExcluded(string name, List<string> skipped)
		{
			if (!UnityExcludedWrappers.Contains(name))
				return false;
			skipped.Add(name);
			return true;
		}

		private static void Resolve(string name, out Type wrapperType, out Type valueType)
		{
			Type found = typeof(SecureValue.SecureInt).Assembly.GetType(name);
			if (found == null)
				throw new InvalidOperationException($"Unknown corpus wrapper: {name}.");
			foreach (Type i in found.GetInterfaces())
			{
				if (
					i.IsGenericType
					&& i.GetGenericTypeDefinition() == typeof(SecureValue.ISecureValue<>)
				)
				{
					wrapperType = found;
					valueType = i.GetGenericArguments()[0];
					return;
				}
			}
			throw new InvalidOperationException($"Corpus wrapper has no ISecureValue<T>: {name}.");
		}

		private static object Decrypted(object wrapper, Type wrapperType)
		{
			PropertyInfo property = wrapperType.GetProperty("Decrypted");
			if (property == null)
				throw new InvalidOperationException($"{wrapperType} has no Decrypted property.");
			return property.GetValue(wrapper);
		}

		private static uint[] ReadUIntArray(JsonNode node)
		{
			List<JsonNode> items = node.ArrayItems();
			uint[] result = new uint[items.Count];
			for (int i = 0; i < items.Count; i++)
				result[i] = uint.Parse(items[i].AsNumber(), Invariant);
			return result;
		}

		/// <summary>
		/// Locates a corpus fixture by querying the directory that holds THIS
		/// test file for the file name, with editor fallbacks when the package
		/// is installed elsewhere (UPM cache, Assets copy). The primary lookup
		/// is a same-directory file query, not a hardcoded path.
		/// </summary>
		private static string LocateCorpus(
			string fileName,
			[CallerFilePath] string? callerFilePath = null
		)
		{
			var searched = new List<string>();
			string thisDir = string.IsNullOrEmpty(callerFilePath)
				? null
				: Path.GetDirectoryName(callerFilePath);
			if (!string.IsNullOrEmpty(thisDir))
			{
				searched.Add(thisDir);
				string[] sameDir = Directory.GetFiles(thisDir, fileName);
				if (sameDir.Length > 0)
					return sameDir[0];
			}
			string assemblyDir = Path.GetDirectoryName(
				typeof(SerializationCorpusTests).Assembly.Location
			);
			if (!string.IsNullOrEmpty(assemblyDir) && !searched.Contains(assemblyDir))
			{
				searched.Add(assemblyDir);
				string[] nearAssembly = Directory.GetFiles(assemblyDir, fileName);
				if (nearAssembly.Length > 0)
					return nearAssembly[0];
			}
			try
			{
				string testDir = TestContext.CurrentContext.TestDirectory;
				if (!string.IsNullOrEmpty(testDir) && !searched.Contains(testDir))
				{
					searched.Add(testDir);
					string[] nearTests = Directory.GetFiles(testDir, fileName);
					if (nearTests.Length > 0)
						return nearTests[0];
				}
			}
			catch (Exception)
			{
				// TestContext may be unavailable; fall through to asset search.
			}
			foreach (string root in AssetRoots())
			{
				try
				{
					string[] found = Directory.GetFiles(
						root,
						fileName,
						SearchOption.AllDirectories
					);
					if (found.Length > 0)
						return found[0];
					searched.Add(root);
				}
				catch (Exception)
				{
					// Unreadable root: keep searching the others.
				}
			}
			Assert.Fail(
				$"Missing serialization corpus '{fileName}'. Searched: {string.Join("; ", searched)}."
			);
			return string.Empty;
		}

		private static IEnumerable<string> AssetRoots()
		{
			string dataPath = string.Empty;
			try
			{
				dataPath = UnityEngine.Application.dataPath;
			}
			catch (Exception)
			{
				yield break;
			}
			if (!string.IsNullOrEmpty(dataPath) && Directory.Exists(dataPath))
				yield return dataPath;
			string projectDir = string.IsNullOrEmpty(dataPath)
				? string.Empty
				: Directory.GetParent(dataPath)?.FullName ?? string.Empty;
			if (!string.IsNullOrEmpty(projectDir))
			{
				string packages = Path.Combine(projectDir, "Packages");
				if (Directory.Exists(packages))
					yield return packages;
			}
		}

		private static JsonNode RequireProperty(JsonNode node, string name)
		{
			if (
				node.Kind != JsonNode.NodeKind.Object
				|| !node.Fields.TryGetValue(name, out JsonNode value)
			)
				throw new InvalidOperationException($"Corpus JSON is missing '{name}'.");
			return value;
		}

		private static object ReadValue(JsonNode node, Type type)
		{
			if (type == typeof(bool))
				return node.AsBool();
			if (type == typeof(byte))
				return byte.Parse(node.AsNumber(), Invariant);
			if (type == typeof(sbyte))
				return sbyte.Parse(node.AsNumber(), Invariant);
			if (type == typeof(short))
				return short.Parse(node.AsNumber(), Invariant);
			if (type == typeof(ushort))
				return ushort.Parse(node.AsNumber(), Invariant);
			if (type == typeof(int))
				return int.Parse(node.AsNumber(), Invariant);
			if (type == typeof(uint))
				return uint.Parse(node.AsNumber(), Invariant);
			if (type == typeof(long))
				return long.Parse(node.AsNumber(), Invariant);
			if (type == typeof(ulong))
				return ulong.Parse(node.AsNumber(), Invariant);
			if (type == typeof(float))
				return ReadFloat(node);
			if (type == typeof(double))
				return ReadDouble(node);
			if (type == typeof(char))
				return ReadChar(node);
			if (type == typeof(string))
			{
				if (node.Kind == JsonNode.NodeKind.Null)
					throw new InvalidOperationException("Unexpected null string in corpus.");
				return node.AsString();
			}
			if (type == typeof(Guid))
				return new Guid(node.AsString());
			if (type == typeof(decimal))
				return decimal.Parse(node.AsNumber(), Invariant);
			if (type == typeof(DateTime))
				return DateTime.Parse(node.AsString(), Invariant, DateTimeStyles.RoundtripKind);
			if (type == typeof(DateTimeOffset))
				return DateTimeOffset.Parse(
					node.AsString(),
					Invariant,
					DateTimeStyles.RoundtripKind
				);
			if (type == typeof(TimeSpan))
				return TimeSpan.Parse(node.AsString(), Invariant);
			if (type == typeof(BigInteger))
				return BigInteger.Parse(node.AsString(), Invariant);
			if (type == typeof(Complex))
				return new Complex(
					ReadDouble(RequireProperty(node, "Real")),
					ReadDouble(RequireProperty(node, "Imaginary"))
				);
			if (type == typeof(Vector2))
				return new Vector2(
					ReadFloat(RequireProperty(node, "X")),
					ReadFloat(RequireProperty(node, "Y"))
				);
			if (type == typeof(Vector3))
				return new Vector3(
					ReadFloat(RequireProperty(node, "X")),
					ReadFloat(RequireProperty(node, "Y")),
					ReadFloat(RequireProperty(node, "Z"))
				);
			if (type == typeof(Vector4))
				return new Vector4(
					ReadFloat(RequireProperty(node, "X")),
					ReadFloat(RequireProperty(node, "Y")),
					ReadFloat(RequireProperty(node, "Z")),
					ReadFloat(RequireProperty(node, "W"))
				);
			if (type == typeof(Quaternion))
				return new Quaternion(
					ReadFloat(RequireProperty(node, "X")),
					ReadFloat(RequireProperty(node, "Y")),
					ReadFloat(RequireProperty(node, "Z")),
					ReadFloat(RequireProperty(node, "W"))
				);
			if (type == typeof(Plane))
			{
				JsonNode normal = RequireProperty(node, "Normal");
				return new Plane(
					new Vector3(
						ReadFloat(RequireProperty(normal, "X")),
						ReadFloat(RequireProperty(normal, "Y")),
						ReadFloat(RequireProperty(normal, "Z"))
					),
					ReadFloat(RequireProperty(node, "D"))
				);
			}
			if (type == typeof(Matrix3x2))
				return new Matrix3x2(
					ReadFloat(RequireProperty(node, "M11")),
					ReadFloat(RequireProperty(node, "M12")),
					ReadFloat(RequireProperty(node, "M21")),
					ReadFloat(RequireProperty(node, "M22")),
					ReadFloat(RequireProperty(node, "M31")),
					ReadFloat(RequireProperty(node, "M32"))
				);
			if (type == typeof(Matrix4x4))
				return new Matrix4x4(
					ReadFloat(RequireProperty(node, "M11")),
					ReadFloat(RequireProperty(node, "M12")),
					ReadFloat(RequireProperty(node, "M13")),
					ReadFloat(RequireProperty(node, "M14")),
					ReadFloat(RequireProperty(node, "M21")),
					ReadFloat(RequireProperty(node, "M22")),
					ReadFloat(RequireProperty(node, "M23")),
					ReadFloat(RequireProperty(node, "M24")),
					ReadFloat(RequireProperty(node, "M31")),
					ReadFloat(RequireProperty(node, "M32")),
					ReadFloat(RequireProperty(node, "M33")),
					ReadFloat(RequireProperty(node, "M34")),
					ReadFloat(RequireProperty(node, "M41")),
					ReadFloat(RequireProperty(node, "M42")),
					ReadFloat(RequireProperty(node, "M43")),
					ReadFloat(RequireProperty(node, "M44"))
				);
			throw new NotSupportedException($"No corpus reader for {type.FullName}.");
		}

		private static float ReadFloat(JsonNode node)
		{
			if (node.Kind == JsonNode.NodeKind.String)
				return float.Parse(NamedFloat(node.AsString()), Invariant);
			return float.Parse(node.AsNumber(), Invariant);
		}

		private static double ReadDouble(JsonNode node)
		{
			if (node.Kind == JsonNode.NodeKind.String)
				return double.Parse(NamedFloat(node.AsString()), Invariant);
			return double.Parse(node.AsNumber(), Invariant);
		}

		private static string NamedFloat(string text)
		{
			// System.Text.Json emits non-finite doubles as quoted names when the
			// corpus allows them; accept the same spellings here.
			if (text == "NaN" || text == "Infinity" || text == "-Infinity")
				return text;
			return text;
		}

		private static char ReadChar(JsonNode node)
		{
			if (node.Kind == JsonNode.NodeKind.Number)
				return (char)int.Parse(node.AsNumber(), Invariant);
			string text = node.AsString();
			if (text.Length == 1)
				return text[0];
			return (char)int.Parse(text, Invariant);
		}

		/// <summary>Minimal JSON document model for the corpus fixtures.</summary>
		private sealed class JsonNode
		{
			public enum NodeKind
			{
				Object,
				Array,
				String,
				Number,
				True,
				False,
				Null,
			}

			public NodeKind Kind;
			public Dictionary<string, JsonNode> Fields = null!;
			public List<JsonNode> Items = null!;
			public string Text = string.Empty;

			public IEnumerable<KeyValuePair<string, JsonNode>> ObjectEntries()
			{
				if (Kind != NodeKind.Object || Fields == null)
					throw new InvalidOperationException("Expected a JSON object.");
				return Fields;
			}

			public List<JsonNode> ArrayItems()
			{
				if (Kind != NodeKind.Array || Items == null)
					throw new InvalidOperationException("Expected a JSON array.");
				return Items;
			}

			public string AsString()
			{
				if (Kind != NodeKind.String)
					throw new InvalidOperationException($"Expected a JSON string, got {Kind}.");
				return Text;
			}

			public string AsNumber()
			{
				if (Kind != NodeKind.Number)
					throw new InvalidOperationException($"Expected a JSON number, got {Kind}.");
				return Text;
			}

			public bool AsBool()
			{
				if (Kind == NodeKind.True)
					return true;
				if (Kind == NodeKind.False)
					return false;
				throw new InvalidOperationException($"Expected a JSON boolean, got {Kind}.");
			}
		}

		/// <summary>
		/// Dependency-free recursive-descent JSON reader: the corpus fixtures
		/// predate any Unity JSON utility that could handle them, and the
		/// package takes no external dependencies.
		/// </summary>
		private sealed class JsonParser
		{
			private readonly string _json;
			private int _pos;

			private JsonParser(string json)
			{
				_json = json;
				_pos = 0;
			}

			public static JsonNode Parse(string json)
			{
				var parser = new JsonParser(json);
				JsonNode root = parser.ParseValue();
				parser.SkipWhitespace();
				if (parser._pos != parser._json.Length)
					throw new FormatException($"Unexpected trailing JSON at offset {parser._pos}.");
				return root;
			}

			private JsonNode ParseValue()
			{
				SkipWhitespace();
				if (_pos >= _json.Length)
					throw new FormatException("Unexpected end of JSON.");
				char c = _json[_pos];
				if (c == '{')
					return ParseObject();
				if (c == '[')
					return ParseArray();
				if (c == '"')
					return new JsonNode { Kind = JsonNode.NodeKind.String, Text = ParseString() };
				if (c == 't')
					return ParseLiteral("true", new JsonNode { Kind = JsonNode.NodeKind.True });
				if (c == 'f')
					return ParseLiteral("false", new JsonNode { Kind = JsonNode.NodeKind.False });
				if (c == 'n')
					return ParseLiteral("null", new JsonNode { Kind = JsonNode.NodeKind.Null });
				return ParseNumber();
			}

			private JsonNode ParseObject()
			{
				Expect('{');
				var node = new JsonNode
				{
					Kind = JsonNode.NodeKind.Object,
					Fields = new Dictionary<string, JsonNode>(),
				};
				SkipWhitespace();
				if (Peek() == '}')
				{
					_pos++;
					return node;
				}
				while (true)
				{
					SkipWhitespace();
					if (Peek() != '"')
						throw new FormatException($"Expected a property name at offset {_pos}.");
					string key = ParseString();
					SkipWhitespace();
					Expect(':');
					node.Fields[key] = ParseValue();
					SkipWhitespace();
					char next = Peek();
					if (next == ',')
					{
						_pos++;
						continue;
					}
					if (next == '}')
					{
						_pos++;
						return node;
					}
					throw new FormatException($"Expected ',' or '}}' at offset {_pos}.");
				}
			}

			private JsonNode ParseArray()
			{
				Expect('[');
				var node = new JsonNode
				{
					Kind = JsonNode.NodeKind.Array,
					Items = new List<JsonNode>(),
				};
				SkipWhitespace();
				if (Peek() == ']')
				{
					_pos++;
					return node;
				}
				while (true)
				{
					node.Items.Add(ParseValue());
					SkipWhitespace();
					char next = Peek();
					if (next == ',')
					{
						_pos++;
						continue;
					}
					if (next == ']')
					{
						_pos++;
						return node;
					}
					throw new FormatException($"Expected ',' or ']' at offset {_pos}.");
				}
			}

			private string ParseString()
			{
				Expect('"');
				var text = new StringBuilder();
				while (true)
				{
					if (_pos >= _json.Length)
						throw new FormatException("Unterminated JSON string.");
					char c = _json[_pos++];
					if (c == '"')
						return text.ToString();
					if (c != '\\')
					{
						text.Append(c);
						continue;
					}
					if (_pos >= _json.Length)
						throw new FormatException("Unterminated JSON escape.");
					char e = _json[_pos++];
					switch (e)
					{
						case '"':
							text.Append('"');
							break;
						case '\\':
							text.Append('\\');
							break;
						case '/':
							text.Append('/');
							break;
						case 'b':
							text.Append('\b');
							break;
						case 'f':
							text.Append('\f');
							break;
						case 'n':
							text.Append('\n');
							break;
						case 'r':
							text.Append('\r');
							break;
						case 't':
							text.Append('\t');
							break;
						case 'u':
							AppendUtf16Escape(text);
							break;
						default:
							throw new FormatException(
								$"Invalid JSON escape '\\{e}' at offset {_pos}."
							);
					}
				}
			}

			private void AppendUtf16Escape(StringBuilder text)
			{
				if (_pos + 4 > _json.Length)
					throw new FormatException("Truncated \\u escape.");
				int unit = 0;
				for (int i = 0; i < 4; i++)
					unit = unit * 16 + HexValue(_json[_pos++]);
				if (
					unit >= 0xD800
					&& unit <= 0xDBFF
					&& _pos + 6 <= _json.Length
					&& _json[_pos] == '\\'
					&& _json[_pos + 1] == 'u'
				)
				{
					int low = 0;
					for (int i = 2; i < 6; i++)
						low = low * 16 + HexValue(_json[_pos + i]);
					if (low >= 0xDC00 && low <= 0xDFFF)
					{
						_pos += 6;
						text.Append((char)unit);
						text.Append((char)low);
						return;
					}
				}
				text.Append((char)unit);
			}

			private static int HexValue(char c)
			{
				if (c >= '0' && c <= '9')
					return c - '0';
				if (c >= 'a' && c <= 'f')
					return c - 'a' + 10;
				if (c >= 'A' && c <= 'F')
					return c - 'A' + 10;
				throw new FormatException($"Invalid hex digit '{c}'.");
			}

			private JsonNode ParseNumber()
			{
				int start = _pos;
				if (Peek() == '-')
					_pos++;
				while (_pos < _json.Length && char.IsDigit(_json[_pos]))
					_pos++;
				if (_pos < _json.Length && _json[_pos] == '.')
				{
					_pos++;
					while (_pos < _json.Length && char.IsDigit(_json[_pos]))
						_pos++;
				}
				if (_pos < _json.Length && (_json[_pos] == 'e' || _json[_pos] == 'E'))
				{
					_pos++;
					if (_pos < _json.Length && (_json[_pos] == '+' || _json[_pos] == '-'))
						_pos++;
					while (_pos < _json.Length && char.IsDigit(_json[_pos]))
						_pos++;
				}
				if (_pos == start)
					throw new FormatException($"Invalid JSON value at offset {_pos}.");
				return new JsonNode
				{
					Kind = JsonNode.NodeKind.Number,
					Text = _json.Substring(start, _pos - start),
				};
			}

			private JsonNode ParseLiteral(string word, JsonNode node)
			{
				if (_pos + word.Length > _json.Length || _json.Substring(_pos, word.Length) != word)
					throw new FormatException($"Invalid JSON value at offset {_pos}.");
				_pos += word.Length;
				return node;
			}

			private void SkipWhitespace()
			{
				while (_pos < _json.Length)
				{
					char c = _json[_pos];
					if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
						return;
					_pos++;
				}
			}

			private char Peek()
			{
				if (_pos >= _json.Length)
					throw new FormatException("Unexpected end of JSON.");
				return _json[_pos];
			}

			private void Expect(char c)
			{
				SkipWhitespace();
				if (Peek() != c)
					throw new FormatException($"Expected '{c}' at offset {_pos}.");
				_pos++;
			}
		}
	}
}
#endif
