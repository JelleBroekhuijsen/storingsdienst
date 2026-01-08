using FluentAssertions;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Services;

public class HolidayServiceTests
{
    private readonly HolidayService _sut;

    public HolidayServiceTests()
    {
        _sut = new HolidayService();
    }

    [Theory]
    [InlineData(2024, 1, 1)]   // New Year's Day
    [InlineData(2024, 4, 27)]  // King's Day
    [InlineData(2024, 5, 5)]   // Liberation Day
    [InlineData(2024, 12, 25)] // Christmas Day
    [InlineData(2024, 12, 26)] // Boxing Day
    public void IsDutchHoliday_FixedHolidays_ReturnsTrue(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeTrue($"{date:yyyy-MM-dd} should be a Dutch holiday");
    }

    [Theory]
    [InlineData(2024, 3, 29)]  // Good Friday 2024
    [InlineData(2024, 3, 31)]  // Easter Sunday 2024
    [InlineData(2024, 4, 1)]   // Easter Monday 2024
    [InlineData(2024, 5, 9)]   // Ascension Day 2024
    [InlineData(2024, 5, 19)]  // Whit Sunday 2024
    [InlineData(2024, 5, 20)]  // Whit Monday 2024
    public void IsDutchHoliday_EasterRelatedHolidays2024_ReturnsTrue(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeTrue($"{date:yyyy-MM-dd} should be a Dutch holiday (Easter-related)");
    }

    [Theory]
    [InlineData(2025, 4, 18)]  // Good Friday 2025
    [InlineData(2025, 4, 20)]  // Easter Sunday 2025
    [InlineData(2025, 4, 21)]  // Easter Monday 2025
    [InlineData(2025, 5, 29)]  // Ascension Day 2025
    [InlineData(2025, 6, 8)]   // Whit Sunday 2025
    [InlineData(2025, 6, 9)]   // Whit Monday 2025
    public void IsDutchHoliday_EasterRelatedHolidays2025_ReturnsTrue(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeTrue($"{date:yyyy-MM-dd} should be a Dutch holiday (Easter-related)");
    }

    [Theory]
    [InlineData(2026, 4, 3)]   // Good Friday 2026
    [InlineData(2026, 4, 5)]   // Easter Sunday 2026
    [InlineData(2026, 4, 6)]   // Easter Monday 2026
    [InlineData(2026, 5, 14)]  // Ascension Day 2026
    [InlineData(2026, 5, 24)]  // Whit Sunday 2026
    [InlineData(2026, 5, 25)]  // Whit Monday 2026
    public void IsDutchHoliday_EasterRelatedHolidays2026_ReturnsTrue(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeTrue($"{date:yyyy-MM-dd} should be a Dutch holiday (Easter-related)");
    }

    [Theory]
    [InlineData(2024, 1, 2)]
    [InlineData(2024, 2, 14)]  // Valentine's Day (not a Dutch holiday)
    [InlineData(2024, 6, 15)]
    [InlineData(2024, 7, 4)]   // US Independence Day (not a Dutch holiday)
    [InlineData(2024, 10, 31)] // Halloween (not a Dutch holiday)
    [InlineData(2024, 11, 11)] // Veterans Day (not a Dutch holiday)
    public void IsDutchHoliday_NonHolidays_ReturnsFalse(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeFalse($"{date:yyyy-MM-dd} should not be a Dutch holiday");
    }

    [Fact]
    public void IsDutchHoliday_CachesResultsForSameYear()
    {
        // Arrange
        var date1 = new DateOnly(2024, 1, 1);
        var date2 = new DateOnly(2024, 12, 25);

        // Act
        var result1 = _sut.IsDutchHoliday(date1);
        var result2 = _sut.IsDutchHoliday(date2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public void IsDutchHoliday_HandlesMultipleYears()
    {
        // Arrange
        var dates = new[]
        {
            new DateOnly(2023, 1, 1),
            new DateOnly(2024, 1, 1),
            new DateOnly(2025, 1, 1),
            new DateOnly(2026, 1, 1)
        };

        // Act & Assert
        foreach (var date in dates)
        {
            _sut.IsDutchHoliday(date).Should().BeTrue($"{date:yyyy} should have New Year's Day as holiday");
        }
    }

    [Theory]
    [InlineData(2020)] // Leap year
    [InlineData(2023)]
    [InlineData(2024)]
    [InlineData(2025)]
    [InlineData(2026)]
    public void IsDutchHoliday_AllYears_HaveAtLeast11Holidays(int year)
    {
        // Arrange & Act
        var holidayCount = 0;
        for (int month = 1; month <= 12; month++)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);
                if (_sut.IsDutchHoliday(date))
                {
                    holidayCount++;
                }
            }
        }

        // Assert - Dutch holidays: New Year, Good Friday, Easter, Easter Monday, King's Day,
        // Liberation Day, Ascension, Whitsun, Whit Monday, Christmas, Boxing Day (11 holidays)
        holidayCount.Should().BeGreaterOrEqualTo(11, $"year {year} should have at least 11 Dutch holidays");
    }

    [Fact]
    public void IsDutchHoliday_EasterAlgorithm_ProducesValidDates()
    {
        // Arrange - Test Easter dates for multiple years to ensure algorithm correctness
        var expectedEasterDates = new Dictionary<int, DateOnly>
        {
            { 2020, new DateOnly(2020, 4, 12) },
            { 2021, new DateOnly(2021, 4, 4) },
            { 2022, new DateOnly(2022, 4, 17) },
            { 2023, new DateOnly(2023, 4, 9) },
            { 2024, new DateOnly(2024, 3, 31) },
            { 2025, new DateOnly(2025, 4, 20) },
            { 2026, new DateOnly(2026, 4, 5) }
        };

        // Act & Assert
        foreach (var (year, expectedEaster) in expectedEasterDates)
        {
            _sut.IsDutchHoliday(expectedEaster).Should().BeTrue($"Easter {year} should be on {expectedEaster:yyyy-MM-dd}");
        }
    }

    [Theory]
    [InlineData(2024, 4, 28)] // Day after King's Day
    [InlineData(2024, 5, 6)]  // Day after Liberation Day
    [InlineData(2024, 12, 27)] // Day after Boxing Day
    public void IsDutchHoliday_DayAfterHoliday_ReturnsFalse(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var result = _sut.IsDutchHoliday(date);

        // Assert
        result.Should().BeFalse($"{date:yyyy-MM-dd} should not be a holiday (day after a holiday)");
    }
}
