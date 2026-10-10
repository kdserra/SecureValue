//:property TargetFramework=net10.0
#:project ../SecureValue/SecureValue.csproj

using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SecureValue;

const int DefaultValuesPerType = 1024;
const long Seed = 0x5345435552455641L;
string repoRoot = FindRepoRoot(Environment.CurrentDirectory);
int count = ParseCount(args);
string outputRoot = Path.Combine(repoRoot, "SecureValue.Tests", "SerializationCorpus");
Directory.CreateDirectory(outputRoot);

Type[] wrappers = typeof(SecureInt)
	.Assembly.GetTypes()
	.Where(t => t.IsValueType && t.Namespace is "SecureValue" or "SecureValue.Numerics")
	.Where(t =>
		t.GetInterfaces()
			.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISecureValue<>))
	)
	.Where(t => typeof(ISecureSerialization).IsAssignableFrom(t))
	.OrderBy(t => t.FullName, StringComparer.Ordinal)
	.ToArray();

var entries = new Dictionary<string, object[]>(StringComparer.Ordinal);

foreach (Type wrapperType in wrappers)
{
	Type valueType = wrapperType
		.GetInterfaces()
		.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISecureValue<>))
		.GetGenericArguments()[0];
	var typeEntries = new ValidationEntry[count];
	var random = new Random(unchecked((int)(Seed ^ StableHash(wrapperType.FullName!))));

	for (int i = 0; i < count; i++)
	{
		object value = CreateValue(valueType, i, random);
		object wrapper =
			Activator.CreateInstance(wrapperType, value)
			?? throw new InvalidOperationException($"Cannot construct {wrapperType}.");
		uint[] serialized = ((ISecureSerialization)wrapper).SaveToSerialized();
		object restored = Activator.CreateInstance(wrapperType)!;
		((ISecureSerialization)restored).LoadFromSerialized(serialized);
		object actual =
			wrapperType.GetProperty("Decrypted")?.GetValue(restored)
			?? throw new InvalidOperationException($"{wrapperType} has no Decrypted property.");
		if (!Equals(value, actual))
			throw new InvalidOperationException(
				$"Round-trip mismatch for {wrapperType} at index {i}."
			);
		typeEntries[i] = new ValidationEntry(value, serialized);
	}

	entries[wrapperType.FullName!] = typeEntries.Cast<object>().ToArray();
}

const string corpusFileName = "serialization-corpus.json";
WriteJson(Path.Combine(outputRoot, corpusFileName), new Corpus(count, entries));

Console.WriteLine($"Generated {count:N0} values per type for {wrappers.Length} wrappers.");
long writtenBytes = new FileInfo(Path.Combine(outputRoot, corpusFileName)).Length;
Console.WriteLine($"{corpusFileName}: {writtenBytes:N0} bytes");
if (writtenBytes > 80L * 1024 * 1024)
	throw new InvalidOperationException($"{corpusFileName} exceeds the 80 MiB corpus budget.");

return;

static int ParseCount(string[] args)
{
	if (args.Length == 0)
		return DefaultValuesPerType;
	if (args.Length != 1 || !int.TryParse(args[0], out int count) || count < 1)
		throw new ArgumentException(
			"Usage: dotnet run Tools/serialization-corpus-generator.cs -- [values-per-type]"
		);
	return count;
}

static string FindRepoRoot(string start)
{
	string? current = Path.GetFullPath(start);
	while (current != null && !File.Exists(Path.Combine(current, "SecureValue.slnx")))
		current = Directory.GetParent(current)?.FullName;
	return current ?? throw new InvalidOperationException("Could not locate repository root.");
}

static int StableHash(string value)
{
	uint hash = 2166136261;
	foreach (char c in value)
	{
		hash ^= c;
		hash *= 16777619;
	}
	return unchecked((int)hash);
}

static void WriteJson(string path, object value)
{
	var options = new JsonSerializerOptions
	{
		WriteIndented = true,
		IncludeFields = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		Converters =
		{
			new BigIntegerJsonConverter(),
			new ComplexJsonConverter(),
			new CharJsonConverter(),
			new RuneJsonConverter(),
		},
	};
	File.WriteAllText(path, JsonSerializer.Serialize(value, options));
}

static object CreateValue(Type type, int index, Random random)
{
	if (type == typeof(bool))
		return index switch
		{
			0 => false,
			1 => true,
			_ => random.Next(2) == 0,
		};
	if (type == typeof(byte))
		return index switch
		{
			0 => byte.MinValue,
			1 => byte.MaxValue,
			_ => (byte)random.Next(256),
		};
	if (type == typeof(sbyte))
		return index switch
		{
			0 => (sbyte)0,
			1 => (sbyte)1,
			2 => (sbyte)-1,
			3 => sbyte.MinValue,
			4 => sbyte.MaxValue,
			5 => (sbyte)2,
			6 => (sbyte)4,
			_ => (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1),
		};
	if (type == typeof(short))
		return index switch
		{
			0 => (short)0,
			1 => (short)1,
			2 => (short)-1,
			3 => short.MinValue,
			4 => short.MaxValue,
			5 => (short)2,
			6 => (short)4,
			7 => (short)8,
			_ => (short)random.Next(short.MinValue, short.MaxValue),
		};
	if (type == typeof(ushort))
		return index switch
		{
			0 => (ushort)0,
			1 => (ushort)1,
			2 => (ushort)2,
			3 => ushort.MinValue,
			4 => ushort.MaxValue,
			5 => (ushort)4,
			6 => (ushort)8,
			_ => (ushort)random.Next(ushort.MaxValue + 1),
		};
	if (type == typeof(int))
		return index switch
		{
			0 => 0,
			1 => 1,
			2 => -1,
			3 => int.MinValue,
			4 => int.MaxValue,
			5 => 2,
			6 => 4,
			7 => 8,
			8 => 16,
			9 => 32,
			_ => random.Next(int.MinValue, int.MaxValue),
		};
	if (type == typeof(uint))
		return index switch
		{
			0 => 0u,
			1 => 1u,
			2 => 2u,
			3 => uint.MinValue,
			4 => uint.MaxValue,
			5 => 4u,
			6 => 8u,
			_ => unchecked((uint)random.NextInt64(0, (long)uint.MaxValue + 1)),
		};
	if (type == typeof(long))
		return index switch
		{
			0 => 0L,
			1 => 1L,
			2 => -1L,
			3 => long.MinValue,
			4 => long.MaxValue,
			5 => 2L,
			6 => 4L,
			7 => 8L,
			8 => 16L,
			9 => 32L,
			_ => random.NextInt64(),
		};
	if (type == typeof(ulong))
		return index switch
		{
			0 => 0UL,
			1 => 1UL,
			2 => 2UL,
			3 => ulong.MinValue,
			4 => ulong.MaxValue,
			5 => 4UL,
			6 => 8UL,
			_ => unchecked((ulong)random.NextInt64()) << 32 | (uint)random.Next(),
		};
	if (type == typeof(float))
		return index switch
		{
			0 => 0f,
			1 => 1f,
			2 => -1f,
			3 => float.MaxValue,
			4 => float.MinValue,
			5 => float.Epsilon,
			_ => (float)(random.NextDouble() * 2000 - 1000),
		};
	if (type == typeof(double))
		return index switch
		{
			0 => 0d,
			1 => 1d,
			2 => -1d,
			3 => double.MaxValue,
			4 => double.MinValue,
			5 => double.Epsilon,
			_ => random.NextDouble() * 2000 - 1000,
		};
	if (type == typeof(char))
		return index switch
		{
			0 => '\0',
			1 => '\uffff',
			_ => (char)random.Next(0x10000),
		};
	if (type == typeof(string))
		return index == 0 ? string.Empty : $"secure-value-{index:x8}-{random.Next():x8}";
	if (type == typeof(Guid))
		return new Guid(Enumerable.Range(0, 16).Select(_ => (byte)random.Next(256)).ToArray());
	if (type == typeof(decimal))
		return index switch
		{
			0 => 0m,
			1 => 1m,
			2 => -1m,
			3 => decimal.MaxValue,
			4 => decimal.MinValue,
			_ => (decimal)(random.NextDouble() * 2000000 - 1000000),
		};
	if (type == typeof(DateTime))
		return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(index * 7919L);
	if (type == typeof(DateTimeOffset))
		return new DateTimeOffset(
			DateTime.SpecifyKind(
				(DateTime)CreateValue(typeof(DateTime), index, random),
				DateTimeKind.Unspecified
			),
			TimeSpan.FromHours((index % 25) - 12)
		);
	if (type == typeof(TimeSpan))
		return TimeSpan.FromTicks(index == 0 ? 0 : index * 7919L);
	if (type == typeof(DateOnly))
		return DateOnly.MinValue.AddDays(index % 3652059);
	if (type == typeof(TimeOnly))
		return TimeOnly.MinValue.Add(TimeSpan.FromTicks(index * 7919L % TimeSpan.TicksPerDay));
	if (type == typeof(Rune))
		return new Rune(index % 0x10FFFF is >= 0xD800 and <= 0xDFFF ? 0x20 : index % 0x10FFFF);
	if (type == typeof(BigInteger))
		return new BigInteger(index == 0 ? 0 : random.NextInt64())
			* (index % 3 == 0 ? BigInteger.One << (index % 127) : BigInteger.One);
	if (type == typeof(Complex))
		return new Complex(
			(double)CreateValue(typeof(double), index, random),
			(double)CreateValue(typeof(double), index + 11, random)
		);
	if (type == typeof(Vector2))
		return new Vector2(F(random), F(random));
	if (type == typeof(Vector3))
		return new Vector3(F(random), F(random), F(random));
	if (type == typeof(Vector4))
		return new Vector4(F(random), F(random), F(random), F(random));
	if (type == typeof(Quaternion))
		return new Quaternion(F(random), F(random), F(random), F(random));
	if (type == typeof(Plane))
		return new Plane(new Vector3(F(random), F(random), F(random)), F(random));
	if (type == typeof(Matrix3x2))
		return new Matrix3x2(F(random), F(random), F(random), F(random), F(random), F(random));
	if (type == typeof(Matrix4x4))
		return new Matrix4x4(
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random),
			F(random)
		);

	throw new NotSupportedException($"No corpus value factory for {type.FullName}.");
}

static float F(Random random) => (float)(random.NextDouble() * 2000 - 1000);

record Corpus(int ValuesPerType, Dictionary<string, object[]> Values);

record ValidationEntry(object Value, uint[] Serialized);

sealed class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
	public override BigInteger Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options
	) => BigInteger.Parse(reader.GetString()!, System.Globalization.CultureInfo.InvariantCulture);

	public override void Write(
		Utf8JsonWriter writer,
		BigInteger value,
		JsonSerializerOptions options
	) => writer.WriteStringValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
}

sealed class ComplexJsonConverter : JsonConverter<Complex>
{
	// System.Text.Json cannot bind Complex's get-only Real/Imaginary properties to its
	// (double, double) constructor, so stock deserialization silently returns (0, 0).
	// Write only the round-trippable members; accept (and ignore) Magnitude/Phase
	// when reading corpora produced before this converter existed.
	public override Complex Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options
	)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for Complex, got {reader.TokenType}.");
		double real = 0;
		double imaginary = 0;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				return new Complex(real, imaginary);
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException($"Unexpected token in Complex object: {reader.TokenType}.");
			string? name = reader.GetString();
			reader.Read();
			if (string.Equals(name, "Real", StringComparison.OrdinalIgnoreCase))
				real =
					reader.TokenType == JsonTokenType.String
						? double.Parse(
							reader.GetString()!,
							System.Globalization.CultureInfo.InvariantCulture
						)
						: reader.GetDouble();
			else if (string.Equals(name, "Imaginary", StringComparison.OrdinalIgnoreCase))
				imaginary =
					reader.TokenType == JsonTokenType.String
						? double.Parse(
							reader.GetString()!,
							System.Globalization.CultureInfo.InvariantCulture
						)
						: reader.GetDouble();
			else
				reader.Skip();
		}
		throw new JsonException("Unterminated Complex object.");
	}

	public override void Write(Utf8JsonWriter writer, Complex value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteNumber("Real", value.Real);
		writer.WriteNumber("Imaginary", value.Imaginary);
		writer.WriteEndObject();
	}
}

sealed class CharJsonConverter : JsonConverter<char>
{
	// Stock char handling writes a JSON string, in which lone-surrogate code units
	// are irreversibly replaced with U+FFFD. Persist the UTF-16 code unit as a
	// number instead; still accept the legacy single-character string form.
	public override char Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options
	)
	{
		if (reader.TokenType == JsonTokenType.Number)
			return (char)reader.GetInt32();
		if (reader.TokenType == JsonTokenType.String)
		{
			string text = reader.GetString()!;
			if (text.Length == 1)
				return text[0];
			if (
				int.TryParse(
					text,
					System.Globalization.NumberStyles.Integer,
					System.Globalization.CultureInfo.InvariantCulture,
					out int code
				)
			)
				return (char)code;
		}
		throw new JsonException($"Cannot convert {reader.TokenType} to char.");
	}

	public override void Write(Utf8JsonWriter writer, char value, JsonSerializerOptions options) =>
		writer.WriteNumberValue((int)value);
}

sealed class RuneJsonConverter : JsonConverter<System.Text.Rune>
{
	// Like Complex, Rune exposes get-only properties that stock deserialization
	// cannot bind, silently returning the default rune. Persist Value explicitly.
	public override System.Text.Rune Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options
	)
	{
		if (reader.TokenType == JsonTokenType.Number)
			return new System.Text.Rune(reader.GetInt32());
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for Rune, got {reader.TokenType}.");
		int value = 0;
		bool found = false;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				if (!found)
					throw new JsonException("Rune object has no Value property.");
				return new System.Text.Rune(value);
			}
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException($"Unexpected token in Rune object: {reader.TokenType}.");
			string? name = reader.GetString();
			reader.Read();
			if (string.Equals(name, "Value", StringComparison.OrdinalIgnoreCase))
			{
				value =
					reader.TokenType == JsonTokenType.String
						? int.Parse(
							reader.GetString()!,
							System.Globalization.CultureInfo.InvariantCulture
						)
						: reader.GetInt32();
				found = true;
			}
			else
				reader.Skip();
		}
		throw new JsonException("Unterminated Rune object.");
	}

	public override void Write(
		Utf8JsonWriter writer,
		System.Text.Rune value,
		JsonSerializerOptions options
	)
	{
		writer.WriteStartObject();
		writer.WriteNumber("Value", value.Value);
		writer.WriteEndObject();
	}
}
