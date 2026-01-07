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
    public EventDateTime? Start { get; set; }

    [JsonPropertyName("end")]
    public EventDateTime? End { get; set; }

    [JsonPropertyName("isAllDay")]
    public bool IsAllDay { get; set; }
}

// DateTime with timezone info
public class EventDateTime
{
    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; } = string.Empty;  // ISO 8601 format

    [JsonPropertyName("timeZone")]
    public string TimeZone { get; set; } = string.Empty;   // e.g., "UTC"
}
