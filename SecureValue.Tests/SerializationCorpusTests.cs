using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SecureValue;

namespace SecureValue.Tests;

public class SerializationCorpusTests
{
	private static readonly JsonSerializerOptions Json = new()
	{
		PropertyNameCaseInsensitive = true,
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

	[Fact]
	public void GenerationCorpus_CurrentSerializationRoundTrips()
	{
		using JsonDocument document = Load("serialization-corpus.json");
		JsonElement valuesByType = document.RootElement.GetProperty("Values");
		int count = Limit(document.RootElement.GetProperty("ValuesPerType").GetInt32());

		foreach (JsonProperty typeEntries in valuesByType.EnumerateObject())
		{
			(Type wrapperType, Type valueType) = Resolve(typeEntries.Name);
			JsonElement.ArrayEnumerator entries = typeEntries.Value.EnumerateArray();
			int index = 0;
			foreach (JsonElement entry in entries)
			{
				if (index++ == count)
					break;
				object value = Deserialize(entry.GetProperty("Value"), valueType);
				object wrapper = NewWrapper(wrapperType, value);
				uint[] serialized = ((ISecureSerialization)wrapper).SaveToSerialized();
				object restored = Activator.CreateInstance(wrapperType)!;
				((ISecureSerialization)restored).LoadFromSerialized(serialized);
				Assert.Equal(value, Decrypted(restored, wrapperType));
			}
		}
	}

	[Fact]
	public void ValidationCorpus_HistoricalSerializationStillLoads()
	{
		using JsonDocument document = Load("serialization-corpus.json");
		JsonElement valuesByType = document.RootElement.GetProperty("Values");
		int count = Limit(document.RootElement.GetProperty("ValuesPerType").GetInt32());

		foreach (JsonProperty typeEntries in valuesByType.EnumerateObject())
		{
			(Type wrapperType, Type valueType) = Resolve(typeEntries.Name);
			int index = 0;
			foreach (JsonElement entry in typeEntries.Value.EnumerateArray())
			{
				if (index++ == count)
					break;
				object expected = Deserialize(entry.GetProperty("Value"), valueType);
				uint[] serialized = entry
					.GetProperty("Serialized")
					.EnumerateArray()
					.Select(v => v.GetUInt32())
					.ToArray();
				object restored = Activator.CreateInstance(wrapperType)!;
				((ISecureSerialization)restored).LoadFromSerialized(serialized);
				Assert.Equal(expected, Decrypted(restored, wrapperType));
			}
		}
	}

	private static JsonDocument Load(string fileName)
	{
		string path = Path.Combine(AppContext.BaseDirectory, "SerializationCorpus", fileName);
		Assert.True(File.Exists(path), $"Missing serialization corpus: {path}");
		return JsonDocument.Parse(File.ReadAllText(path));
	}

	private static int Limit(int corpusCount)
	{
		string? setting = Environment.GetEnvironmentVariable("SECUREVALUE_SERIALIZATION_COUNT");
		return int.TryParse(setting, out int limit) && limit > 0
			? Math.Min(limit, corpusCount)
			: corpusCount;
	}

	private static (Type Wrapper, Type Value) Resolve(string name)
	{
		Type wrapper =
			typeof(SecureInt).Assembly.GetType(name)
			?? throw new InvalidOperationException($"Unknown corpus wrapper: {name}");
		Type value = wrapper
			.GetInterfaces()
			.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISecureValue<>))
			.GetGenericArguments()[0];
		return (wrapper, value);
	}

	private static object Deserialize(JsonElement element, Type type) =>
		JsonSerializer.Deserialize(element.GetRawText(), type, Json)!
		?? throw new InvalidOperationException($"Could not deserialize {type}.");

	private static object NewWrapper(Type wrapperType, object value) =>
		Activator.CreateInstance(wrapperType, value)!
		?? throw new InvalidOperationException($"Could not construct {wrapperType}.");

	private static object Decrypted(object wrapper, Type wrapperType) =>
		wrapperType.GetProperty("Decrypted")?.GetValue(wrapper)
		?? throw new InvalidOperationException($"{wrapperType} has no Decrypted property.");

	private sealed class BigIntegerJsonConverter : JsonConverter<BigInteger>
	{
		public override BigInteger Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options
		) =>
			BigInteger.Parse(
				reader.GetString()!,
				System.Globalization.CultureInfo.InvariantCulture
			);

		public override void Write(
			Utf8JsonWriter writer,
			BigInteger value,
			JsonSerializerOptions options
		) =>
			writer.WriteStringValue(
				value.ToString(System.Globalization.CultureInfo.InvariantCulture)
			);
	}

	private sealed class ComplexJsonConverter : JsonConverter<Complex>
	{
		// System.Text.Json cannot bind Complex's get-only Real/Imaginary properties to its
		// (double, double) constructor, so stock deserialization silently returns (0, 0)
		// even for payloads it serialized itself. Handle Real/Imaginary explicitly and
		// ignore the computed Magnitude/Phase members.
		public override Complex Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options
		)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException(
					$"Expected StartObject for Complex, got {reader.TokenType}."
				);
			double real = 0;
			double imaginary = 0;
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					return new Complex(real, imaginary);
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException(
						$"Unexpected token in Complex object: {reader.TokenType}."
					);
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

		public override void Write(
			Utf8JsonWriter writer,
			Complex value,
			JsonSerializerOptions options
		)
		{
			writer.WriteStartObject();
			writer.WriteNumber("Real", value.Real);
			writer.WriteNumber("Imaginary", value.Imaginary);
			writer.WriteEndObject();
		}
	}

	private sealed class CharJsonConverter : JsonConverter<char>
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

		public override void Write(
			Utf8JsonWriter writer,
			char value,
			JsonSerializerOptions options
		) => writer.WriteNumberValue((int)value);
	}

	private sealed class RuneJsonConverter : JsonConverter<System.Text.Rune>
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
					throw new JsonException(
						$"Unexpected token in Rune object: {reader.TokenType}."
					);
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
}
