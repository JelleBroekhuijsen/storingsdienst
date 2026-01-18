using FluentAssertions;
using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Tests.Services;

/// <summary>
/// Tests for subject filtering logic used in Graph API mode
/// </summary>
public class SubjectFilteringLogicTests
{
    [Fact]
    public void ExtractDistinctSubjects_MultipleSubjects_ReturnsDistinctSubjects()
    {
        // Arrange - simulating the GraphService returning multiple events with different subjects
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 30, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Daily admin tasks",
                StartDateTime = new DateTime(2024, 1, 16, 14, 0, 0),
                EndDateTime = new DateTime(2024, 1, 16, 15, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "3",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 17, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 17, 10, 30, 0),
                IsAllDay = false
            }
        };

        // Act - This is the logic used in Home.razor's SearchMeetingsAsync
        var distinctSubjects = events
            .Select(e => e.Subject)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        // Assert
        distinctSubjects.Should().HaveCount(2);
        distinctSubjects.Should().Contain("Daily Standup");
        distinctSubjects.Should().Contain("Daily admin tasks");
    }

    [Fact]
    public void ExtractDistinctSubjects_SingleSubject_ReturnsSingleSubject()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 30, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 16, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 16, 10, 30, 0),
                IsAllDay = false
            }
        };

        // Act
        var distinctSubjects = events
            .Select(e => e.Subject)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        // Assert
        distinctSubjects.Should().HaveCount(1);
        distinctSubjects.Should().Contain("Daily Standup");
    }

    [Fact]
    public void FilterBySelectedSubject_SpecificSubject_FiltersCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 30, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Daily admin tasks",
                StartDateTime = new DateTime(2024, 1, 16, 14, 0, 0),
                EndDateTime = new DateTime(2024, 1, 16, 15, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "3",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 17, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 17, 10, 30, 0),
                IsAllDay = false
            }
        };

        var selectedSubject = "Daily Standup";

        // Act - This is the logic used in Home.razor's AnalyzeSelectedSubjectAsync
        var filteredEvents = events.Where(e => e.Subject == selectedSubject).ToList();

        // Assert
        filteredEvents.Should().HaveCount(2);
        filteredEvents.Should().AllSatisfy(e => e.Subject.Should().Be("Daily Standup"));
        filteredEvents.Should().Contain(e => e.Id == "1");
        filteredEvents.Should().Contain(e => e.Id == "3");
    }

    [Fact]
    public void FilterBySelectedSubject_AllOption_ReturnsAllEvents()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 30, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Daily admin tasks",
                StartDateTime = new DateTime(2024, 1, 16, 14, 0, 0),
                EndDateTime = new DateTime(2024, 1, 16, 15, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "3",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 17, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 17, 10, 30, 0),
                IsAllDay = false
            }
        };

        var selectedSubject = "All";

        // Act - This is the logic used in Home.razor's AnalyzeSelectedSubjectAsync
        var filteredEvents = selectedSubject == "All"
            ? events
            : events.Where(e => e.Subject == selectedSubject).ToList();

        // Assert
        filteredEvents.Should().HaveCount(3);
        filteredEvents.Should().Contain(e => e.Id == "1");
        filteredEvents.Should().Contain(e => e.Id == "2");
        filteredEvents.Should().Contain(e => e.Id == "3");
    }

    [Fact]
    public void FilterBySelectedSubject_EmptySubject_FiltersCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Daily Standup",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 30, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "",
                StartDateTime = new DateTime(2024, 1, 16, 14, 0, 0),
                EndDateTime = new DateTime(2024, 1, 16, 15, 0, 0),
                IsAllDay = false
            }
        };

        var selectedSubject = "";

        // Act
        var filteredEvents = selectedSubject == "All"
            ? events
            : events.Where(e => e.Subject == selectedSubject).ToList();

        // Assert
        filteredEvents.Should().HaveCount(1);
        filteredEvents.Should().Contain(e => e.Id == "2");
    }
}
