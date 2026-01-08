using FluentAssertions;
using Moq;
using Storingsdienst.Client.Models;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Services;

public class MeetingAnalysisServiceTests
{
    private readonly Mock<IHolidayService> _holidayServiceMock;
    private readonly MeetingAnalysisService _sut;

    public MeetingAnalysisServiceTests()
    {
        _holidayServiceMock = new Mock<IHolidayService>();
        _sut = new MeetingAnalysisService(_holidayServiceMock.Object);
    }

    [Fact]
    public void AnalyzeMeetings_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var events = new List<CalendarEventDto>();

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AnalyzeMeetings_SingleDayEvent_ReturnsCorrectBreakdown()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Test Meeting",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0), // Monday
                EndDateTime = new DateTime(2024, 1, 15, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].Year.Should().Be(2024);
        result[0].Month.Should().Be(1);
        result[0].TotalMeetingDays.Should().Be(1);
        result[0].WeekdayCount.Should().Be(1);
        result[0].WeekendCount.Should().Be(0);
        result[0].HolidayCount.Should().Be(0);
    }

    [Fact]
    public void AnalyzeMeetings_MultiDayEvent_CountsEachDaySeparately()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Multi-day Conference",
                StartDateTime = new DateTime(2024, 1, 15, 9, 0, 0),  // Monday
                EndDateTime = new DateTime(2024, 1, 17, 17, 0, 0),    // Wednesday
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(3, "event spans 3 days: Jan 15, 16, 17");
        result[0].WeekdayCount.Should().Be(3);
    }

    [Fact]
    public void AnalyzeMeetings_WeekendDays_CategorizedCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Weekend Meeting",
                StartDateTime = new DateTime(2024, 1, 6, 10, 0, 0),  // Saturday
                EndDateTime = new DateTime(2024, 1, 7, 12, 0, 0),    // Sunday
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(2);
        result[0].WeekendCount.Should().Be(2);
        result[0].WeekdayCount.Should().Be(0);
    }

    [Fact]
    public void AnalyzeMeetings_Holiday_CategorizedCorrectly()
    {
        // Arrange
        var newYearsDay = new DateOnly(2024, 1, 1);
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "New Year Meeting",
                StartDateTime = new DateTime(2024, 1, 1, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 1, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(newYearsDay)).Returns(true);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(1);
        result[0].HolidayCount.Should().Be(1);
        result[0].WeekdayCount.Should().Be(0);
        result[0].WeekendCount.Should().Be(0);
    }

    [Fact]
    public void AnalyzeMeetings_MultipleEventsOnSameDay_CountsOnlyOnce()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Morning Meeting",
                StartDateTime = new DateTime(2024, 1, 15, 9, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Afternoon Meeting",
                StartDateTime = new DateTime(2024, 1, 15, 14, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 15, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(1, "multiple events on same day should count as 1 day");
    }

    [Fact]
    public void AnalyzeMeetings_MultipleMonths_ReturnsMultipleBreakdowns()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "January Meeting",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 11, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "February Meeting",
                StartDateTime = new DateTime(2024, 2, 20, 10, 0, 0),
                EndDateTime = new DateTime(2024, 2, 20, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(2);
        result[0].Month.Should().Be(2, "results should be ordered by date descending");
        result[1].Month.Should().Be(1);
    }

    [Fact]
    public void AnalyzeMeetings_MixedDayTypes_CategorizesCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Multi-day Event",
                StartDateTime = new DateTime(2024, 1, 5, 9, 0, 0),   // Friday
                EndDateTime = new DateTime(2024, 1, 8, 17, 0, 0),    // Monday (includes weekend)
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(4, "Jan 5, 6, 7, 8");
        result[0].WeekdayCount.Should().Be(2, "Friday and Monday");
        result[0].WeekendCount.Should().Be(2, "Saturday and Sunday");
    }

    [Fact]
    public void AnalyzeMeetings_EventSpanningMonths_SplitsCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Month-spanning Event",
                StartDateTime = new DateTime(2024, 1, 30, 9, 0, 0),
                EndDateTime = new DateTime(2024, 2, 2, 17, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(2);

        var februaryBreakdown = result.First(r => r.Month == 2);
        februaryBreakdown.TotalMeetingDays.Should().Be(2, "Feb 1 and 2");

        var januaryBreakdown = result.First(r => r.Month == 1);
        januaryBreakdown.TotalMeetingDays.Should().Be(2, "Jan 30 and 31");
    }

    [Fact]
    public void AnalyzeMeetings_SortsByYearAndMonthDescending()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Old Meeting",
                StartDateTime = new DateTime(2023, 1, 15, 10, 0, 0),
                EndDateTime = new DateTime(2023, 1, 15, 11, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Recent Meeting",
                StartDateTime = new DateTime(2024, 12, 20, 10, 0, 0),
                EndDateTime = new DateTime(2024, 12, 20, 11, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "3",
                Subject = "Mid-year Meeting",
                StartDateTime = new DateTime(2024, 6, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 6, 15, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(3);
        result[0].Year.Should().Be(2024);
        result[0].Month.Should().Be(12);
        result[1].Year.Should().Be(2024);
        result[1].Month.Should().Be(6);
        result[2].Year.Should().Be(2023);
        result[2].Month.Should().Be(1);
    }

    [Fact]
    public void AnalyzeMeetings_MonthName_IsSetCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Test Meeting",
                StartDateTime = new DateTime(2024, 3, 15, 10, 0, 0),
                EndDateTime = new DateTime(2024, 3, 15, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result[0].MonthName.Should().NotBeNullOrEmpty();
        result[0].MonthName.Should().Contain("March", "month name should be March");
    }

    [Fact]
    public void AnalyzeMeetings_AllDayEvent_CountedCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "All Day Event",
                StartDateTime = new DateTime(2024, 1, 15, 0, 0, 0),
                EndDateTime = new DateTime(2024, 1, 15, 23, 59, 59),
                IsAllDay = true
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(1);
    }

    [Fact]
    public void AnalyzeMeetings_HolidayPriority_HolidayOverridesWeekend()
    {
        // Arrange - Christmas Day 2024 is a Wednesday, but let's test a holiday on weekend
        var christmasDay = new DateOnly(2024, 12, 25);
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Holiday Meeting",
                StartDateTime = new DateTime(2024, 12, 25, 10, 0, 0),
                EndDateTime = new DateTime(2024, 12, 25, 11, 0, 0),
                IsAllDay = false
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(christmasDay)).Returns(true);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].HolidayCount.Should().Be(1, "holidays should take priority over weekends");
        result[0].WeekendCount.Should().Be(0);
    }

    [Fact]
    public void AnalyzeMeetings_AllDayEventWithExclusiveEndDate_CountsCorrectDays()
    {
        // Arrange - Graph API returns all-day events with exclusive end dates
        // A 6-day event from Monday (March 10) to Saturday (March 15) 
        // will have end date of Sunday (March 16) at 00:00:00
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Storingsdienst",
                StartDateTime = new DateTime(2025, 3, 10, 0, 0, 0), // Monday
                EndDateTime = new DateTime(2025, 3, 16, 0, 0, 0),   // Sunday 00:00 (exclusive)
                IsAllDay = true
            }
        };

        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.IsAny<DateOnly>())).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(6, "Monday through Saturday = 6 days");
        result[0].WeekdayCount.Should().Be(5, "Monday through Friday");
        result[0].WeekendCount.Should().Be(1, "Saturday only");
    }

    [Fact]
    public void AnalyzeMeetings_ComplexScenario_HandlesCorrectly()
    {
        // Arrange
        var events = new List<CalendarEventDto>
        {
            new CalendarEventDto
            {
                Id = "1",
                Subject = "Regular Weekday",
                StartDateTime = new DateTime(2024, 1, 15, 10, 0, 0), // Monday
                EndDateTime = new DateTime(2024, 1, 15, 11, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "2",
                Subject = "Weekend Day",
                StartDateTime = new DateTime(2024, 1, 20, 10, 0, 0), // Saturday
                EndDateTime = new DateTime(2024, 1, 20, 11, 0, 0),
                IsAllDay = false
            },
            new CalendarEventDto
            {
                Id = "3",
                Subject = "Duplicate Weekday",
                StartDateTime = new DateTime(2024, 1, 15, 14, 0, 0), // Same Monday
                EndDateTime = new DateTime(2024, 1, 15, 15, 0, 0),
                IsAllDay = false
            }
        };

        var newYearsDay = new DateOnly(2024, 1, 1);
        _holidayServiceMock.Setup(x => x.IsDutchHoliday(newYearsDay)).Returns(true);
        _holidayServiceMock.Setup(x => x.IsDutchHoliday(It.Is<DateOnly>(d => d != newYearsDay))).Returns(false);

        // Act
        var result = _sut.AnalyzeMeetings(events);

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalMeetingDays.Should().Be(2, "Jan 15 (Monday) and Jan 20 (Saturday)");
        result[0].WeekdayCount.Should().Be(1);
        result[0].WeekendCount.Should().Be(1);
    }
}
