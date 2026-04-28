using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeIsland.Windows.Models;

/// <summary>
/// Dynamic JSON value type matching Swift's JSONValue.
/// </summary>
[JsonConverter(typeof(JsonValueConverter))]
public abstract class JsonValue
{
    public static JsonValue Null { get; } = new JsonNullValue();
    public static JsonValue Bool(bool value) => new JsonBoolValue(value);
    public static JsonValue Int(int value) => new JsonIntValue(value);
    public static JsonValue Double(double value) => new JsonDoubleValue(value);
    public static JsonValue String(string value) => new JsonStringValue(value);
    public static JsonValue Array(List<JsonValue> value) => new JsonArrayValue(value);
    public static JsonValue Object(Dictionary<string, JsonValue> value) => new JsonObjectValue(value);

    public abstract object? ToClrValue();
}

public sealed class JsonNullValue : JsonValue
{
    public override object? ToClrValue() => null;
}

public sealed class JsonBoolValue : JsonValue
{
    public bool Value { get; }
    public JsonBoolValue(bool value) => Value = value;
    public override object? ToClrValue() => Value;
}

public sealed class JsonIntValue : JsonValue
{
    public int Value { get; }
    public JsonIntValue(int value) => Value = value;
    public override object? ToClrValue() => Value;
}

public sealed class JsonDoubleValue : JsonValue
{
    public double Value { get; }
    public JsonDoubleValue(double value) => Value = value;
    public override object? ToClrValue() => Value;
}

public sealed class JsonStringValue : JsonValue
{
    public string Value { get; }
    public JsonStringValue(string value) => Value = value;
    public override object? ToClrValue() => Value;
}

public sealed class JsonArrayValue : JsonValue
{
    public List<JsonValue> Value { get; }
    public JsonArrayValue(List<JsonValue> value) => Value = value;
    public override object? ToClrValue() => Value;
}

public sealed class JsonObjectValue : JsonValue
{
    public Dictionary<string, JsonValue> Value { get; }
    public JsonObjectValue(Dictionary<string, JsonValue> value) => Value = value;
    public override object? ToClrValue() => Value;
}

/// <summary>
/// JSON converter for the JsonValue discriminated union.
/// </summary>
public sealed class JsonValueConverter : JsonConverter<JsonValue>
{
    public override JsonValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => JsonValue.Null,
            JsonTokenType.True => JsonValue.Bool(true),
            JsonTokenType.False => JsonValue.Bool(false),
            JsonTokenType.Number => reader.TryGetInt32(out int i)
                ? JsonValue.Int(i)
                : JsonValue.Double(reader.GetDouble()),
            JsonTokenType.String => JsonValue.String(reader.GetString()!),
            JsonTokenType.StartArray => ReadArray(ref reader, options),
            JsonTokenType.StartObject => ReadObject(ref reader, options),
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    private static JsonValue ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var list = new List<JsonValue>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return JsonValue.Array(list);
            list.Add(Read(ref reader, typeof(JsonValue), options)!);
        }
        throw new JsonException("Unexpected end of array");
    }

    private static JsonValue ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, JsonValue>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return JsonValue.Object(dict);
            var key = reader.GetString()!;
            reader.Read();
            dict[key] = Read(ref reader, typeof(JsonValue), options)!;
        }
        throw new JsonException("Unexpected end of object");
    }

    public override void Write(Utf8JsonWriter writer, JsonValue value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case JsonNullValue:
                writer.WriteNullValue();
                break;
            case JsonBoolValue b:
                writer.WriteBooleanValue(b.Value);
                break;
            case JsonIntValue i:
                writer.WriteNumberValue(i.Value);
                break;
            case JsonDoubleValue d:
                writer.WriteNumberValue(d.Value);
                break;
            case JsonStringValue s:
                writer.WriteStringValue(s.Value);
                break;
            case JsonArrayValue a:
                writer.WriteStartArray();
                foreach (var item in a.Value)
                    Write(writer, item, options);
                writer.WriteEndArray();
                break;
            case JsonObjectValue o:
                writer.WriteStartObject();
                foreach (var kvp in o.Value)
                {
                    writer.WritePropertyName(kvp.Key);
                    Write(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
                break;
        }
    }
}

/// <summary>
/// Bridge response sent from the app back to CodeIslandBridge.
/// Corresponds to BridgeResponse in IslandShared/Models.swift:293.
/// </summary>
public sealed class BridgeResponse
{
    [JsonPropertyName("requestID")]
    public Guid RequestId { get; set; }

    [JsonPropertyName("decision")]
    public InterventionDecision? Decision { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("updatedInput")]
    public Dictionary<string, JsonValue>? UpdatedInput { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Intervention decision types.
/// Corresponds to InterventionDecision in IslandShared/Models.swift.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InterventionDecision
{
    [JsonPropertyName("approve")]
    Approve,
    [JsonPropertyName("approveForSession")]
    ApproveForSession,
    [JsonPropertyName("deny")]
    Deny,
    [JsonPropertyName("cancel")]
    Cancel
}

/// <summary>
/// Helper to extract answers from BridgeResponse.UpdatedInput.
/// </summary>
public static class BridgeAnswerPayload
{
    public static Dictionary<string, string> ExtractAnswers(Dictionary<string, JsonValue>? updatedInput)
    {
        if (updatedInput == null || !updatedInput.TryGetValue("answers", out var answersValue))
            return [];

        if (answersValue is JsonObjectValue obj)
        {
            return obj.Value.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value switch
                {
                    JsonStringValue s => s.Value,
                    _ => string.Empty
                });
        }

        return [];
    }
}
