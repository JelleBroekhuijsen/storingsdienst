using FluentAssertions;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Services;

public class JsonImportServiceTests
{
    private readonly JsonImportService _sut;

    public JsonImportServiceTests()
    {
        _sut = new JsonImportService();
    }

    [Fact]
    public async Task ParseJsonFileAsync_EmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var jsonContent = "";
        var subjectFilter = "";
        var startDate = DateTime.Now.AddYears(-1);
        var endDate = DateTime.Now;

        // Act
        var act = async () => await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*JSON content cannot be empty*");
    }

    [Fact]
    public async Task ParseJsonFileAsync_NullContent_ThrowsArgumentException()
    {
        // Arrange
        string jsonContent = null!;
        var subjectFilter = "";
        var startDate = DateTime.Now.AddYears(-1);
        var endDate = DateTime.Now;

        // Act
        var act = async () => await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*JSON content cannot be empty*");
    }

    [Fact]
    public async Task ParseJsonFileAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var jsonContent = "This is not valid JSON";
        var subjectFilter = "";
        var startDate = DateTime.Now.AddYears(-1);
        var endDate = DateTime.Now;

        // Act
        var act = async () => await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid JSON format*");
    }

    [Fact]
    public async Task ParseJsonFileAsync_MissingEventsField_ReturnsEmptyList()
    {
        // Arrange - When events field is missing, the model defaults to empty list
        var jsonContent = "{\"exportDate\": \"2024-01-01T00:00:00Z\"}";
        var subjectFilter = "";
        var startDate = DateTime.Now.AddYears(-1);
        var endDate = DateTime.Now;

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().BeEmpty("missing events field should result in empty list due to default initialization");
    }

    [Fact]
    public async Task ParseJsonFileAsync_EmptyEventsArray_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = "{\"events\": []}";
        var subjectFilter = "";
        var startDate = DateTime.Now.AddYears(-1);
        var endDate = DateTime.Now;

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseJsonFileAsync_ValidSingleEvent_ReturnsOneEvent()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Test Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Test Meeting");
        result[0].StartDateTime.Should().Be(new DateTime(2024, 1, 15, 10, 0, 0));
        result[0].EndDateTime.Should().Be(new DateTime(2024, 1, 15, 11, 0, 0));
        result[0].IsAllDay.Should().BeFalse();
        result[0].Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ParseJsonFileAsync_SubjectFilter_FiltersCorrectly()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Team Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Project Review"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "Standup";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Team Standup");
    }

    [Fact]
    public async Task ParseJsonFileAsync_SubjectFilter_IsCaseInsensitive()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Team STANDUP"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "standup";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseJsonFileAsync_DateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Old Meeting"",
                    ""start"": {
                        ""dateTime"": ""2023-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2023-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Current Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Current Meeting");
    }

    [Fact]
    public async Task ParseJsonFileAsync_EventMissingStartDate_SkipsEvent()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Invalid Meeting"",
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Valid Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Valid Meeting");
    }

    [Fact]
    public async Task ParseJsonFileAsync_EventWithInvalidDateFormat_SkipsEvent()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Invalid Date Meeting"",
                    ""start"": {
                        ""dateTime"": ""not-a-valid-date"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Valid Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Valid Meeting");
    }

    [Fact]
    public async Task ParseJsonFileAsync_AllDayEvent_ParsedCorrectly()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""All Day Event"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T00:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T23:59:59"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": true
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsAllDay.Should().BeTrue();
    }

    [Fact]
    public async Task ParseJsonFileAsync_MultipleEvents_AllParsedCorrectly()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Meeting 1"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Meeting 2"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Meeting 3"",
                    ""start"": {
                        ""dateTime"": ""2024-01-17T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-17T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(3);
        result[0].Subject.Should().Be("Meeting 1");
        result[1].Subject.Should().Be("Meeting 2");
        result[2].Subject.Should().Be("Meeting 3");
    }

    [Fact]
    public async Task ParseJsonFileAsync_EventStartingBeforeRangeEndingInRange_IsIncluded()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Overlapping Meeting"",
                    ""start"": {
                        ""dateTime"": ""2023-12-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseJsonFileAsync_EventStartingInRangeEndingAfterRange_IsIncluded()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Overlapping Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-12-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2025-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseJsonFileAsync_EventCompletelyOutsideRange_IsExcluded()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Outside Meeting"",
                    ""start"": {
                        ""dateTime"": ""2023-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2023-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseJsonFileAsync_CaseInsensitivePropertyNames_ParsedCorrectly()
    {
        // Arrange - Properties in different case
        var jsonContent = @"{
            ""Events"": [
                {
                    ""Subject"": ""Test Meeting"",
                    ""Start"": {
                        ""DateTime"": ""2024-01-15T10:00:00"",
                        ""TimeZone"": ""UTC""
                    },
                    ""End"": {
                        ""DateTime"": ""2024-01-15T11:00:00"",
                        ""TimeZone"": ""UTC""
                    },
                    ""IsAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Subject.Should().Be("Test Meeting");
    }

    [Fact]
    public async Task ParseJsonFileAsync_EachEventGetsUniqueId()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Meeting 1"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Meeting 2"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";
        var subjectFilter = "";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var result = await _sut.ParseJsonFileAsync(jsonContent, subjectFilter, startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().NotBe(result[1].Id);
        result[0].Id.Should().NotBeNullOrEmpty();
        result[1].Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_EmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = "";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_NullContent_ReturnsEmptyList()
    {
        // Arrange
        string jsonContent = null!;

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = "This is not valid JSON";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_NoRecurringSubjects_ReturnsEmptyList()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Meeting 1"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Meeting 2"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_OneRecurringSubject_ReturnsThatSubject()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Daily Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Daily Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Unique Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-17T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-17T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be("Daily Standup");
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_MultipleRecurringSubjects_ReturnsAllSortedAlphabetically()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Weekly Review"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Daily Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Weekly Review"",
                    ""start"": {
                        ""dateTime"": ""2024-01-22T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-22T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Daily Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-17T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-17T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be("Daily Standup");
        result[1].Should().Be("Weekly Review");
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_CaseInsensitiveGrouping_GroupsCorrectly()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": ""Daily Standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""DAILY STANDUP"",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""daily standup"",
                    ""start"": {
                        ""dateTime"": ""2024-01-17T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-17T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().HaveCount(1);
        // The result preserves the casing of the first occurrence
        result[0].Should().Be("Daily Standup");
    }

    [Fact]
    public async Task GetRecurringSubjectsAsync_IgnoresEmptySubjects()
    {
        // Arrange
        var jsonContent = @"{
            ""events"": [
                {
                    ""subject"": """",
                    ""start"": {
                        ""dateTime"": ""2024-01-15T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-15T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": """",
                    ""start"": {
                        ""dateTime"": ""2024-01-16T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-16T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                },
                {
                    ""subject"": ""Valid Meeting"",
                    ""start"": {
                        ""dateTime"": ""2024-01-17T10:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""end"": {
                        ""dateTime"": ""2024-01-17T11:00:00"",
                        ""timeZone"": ""UTC""
                    },
                    ""isAllDay"": false
                }
            ]
        }";

        // Act
        var result = await _sut.GetRecurringSubjectsAsync(jsonContent);

        // Assert
        result.Should().BeEmpty();
    }
}
