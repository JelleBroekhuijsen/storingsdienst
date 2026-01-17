using System.Text.Json;
using System.Text.Json.Serialization;

namespace Storingsdienst.Client.Models;

// Root object for Power Automate export
public class PowerAutomateExport
{
    [JsonPropertyName("events")]
    public List<PowerAutomateEvent> Events { get; set; } = new();

    [JsonPropertyName("exportDate")]
    public DateTime? ExportDate { get; set; }

    [JsonPropertyName("subjectFilter")]
    public string? SubjectFilter { get; set; }
}

// Event in Power Automate format
public class PowerAutomateEvent
{
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("start")]
    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public string? End { get; set; }

    [JsonPropertyName("isAllDay")]
    public bool IsAllDay { get; set; }
}

// Custom converter to handle both string and object date formats
public class FlexibleDateTimeConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle direct string format: "2025-02-03T08:00:00+00:00"
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        // Handle object format: { "dateTime": "...", "timeZone": "..." }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            string? dateTimeValue = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    if (string.Equals(propertyName, "dateTime", StringComparison.OrdinalIgnoreCase))
                    {
                        dateTimeValue = reader.GetString();
                    }
                    // Skip other properties like "timeZone"
                }
            }

            return dateTimeValue;
        }

        // Handle null
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
