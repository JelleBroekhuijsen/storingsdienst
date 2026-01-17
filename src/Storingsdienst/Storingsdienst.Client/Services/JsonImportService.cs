using System.Text.Json;
using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public class JsonImportService
{
    public Task<List<CalendarEventDto>> ParseJsonFileAsync(
        string jsonContent,
        string subjectFilter,
        DateTime startDate,
        DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            throw new ArgumentException("JSON content cannot be empty", nameof(jsonContent));
        }

        // Parse JSON with case-insensitive property matching
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        PowerAutomateExport? export;
        try
        {
            export = JsonSerializer.Deserialize<PowerAutomateExport>(jsonContent, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON format: {ex.Message}", ex);
        }

        if (export == null || export.Events == null)
        {
            throw new InvalidOperationException("JSON missing required 'events' field");
        }

        // Map to CalendarEventDto and apply filters
        var results = new List<CalendarEventDto>();

        foreach (var evt in export.Events)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(evt.Start) || string.IsNullOrEmpty(evt.End))
            {
                continue; // Skip events with missing date/time info
            }

            // Subject filter (case-insensitive contains)
            if (!string.IsNullOrWhiteSpace(subjectFilter) &&
                !evt.Subject.Contains(subjectFilter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Parse dates
            DateTime startDateTime;
            DateTime endDateTime;

            try
            {
                startDateTime = DateTime.Parse(evt.Start);
                endDateTime = DateTime.Parse(evt.End);
            }
            catch (FormatException)
            {
                continue; // Skip events with invalid date format
            }

            // Date range filter
            if (startDateTime.Date > endDate.Date || endDateTime.Date < startDate.Date)
            {
                continue;
            }

            // Create DTO
            results.Add(new CalendarEventDto
            {
                Id = Guid.NewGuid().ToString(), // Generate ID for imported events
                Subject = evt.Subject,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                IsAllDay = evt.IsAllDay
            });
        }

        return Task.FromResult(results);
    }

    public Task<List<string>> GetRecurringSubjectsAsync(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return Task.FromResult(new List<string>());
        }

        // Parse JSON with case-insensitive property matching
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        PowerAutomateExport? export;
        try
        {
            export = JsonSerializer.Deserialize<PowerAutomateExport>(jsonContent, options);
        }
        catch (JsonException)
        {
            return Task.FromResult(new List<string>());
        }

        if (export == null || export.Events == null || !export.Events.Any())
        {
            return Task.FromResult(new List<string>());
        }

        // Group events by subject (case-insensitive) and count occurrences
        // Use First().Subject to ensure consistent casing (preserves first occurrence)
        var subjectCounts = export.Events
            .Where(evt => !string.IsNullOrWhiteSpace(evt.Subject))
            .GroupBy(evt => evt.Subject, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.First().Subject)
            .OrderBy(subject => subject, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(subjectCounts);
    }
}
