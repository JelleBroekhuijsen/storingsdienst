using ClosedXML.Excel;
using FluentAssertions;
using Storingsdienst.Client.Models;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Services;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _sut;

    public ExcelExportServiceTests()
    {
        _sut = new ExcelExportService();
    }

    [Fact]
    public void GenerateExcelReport_EmptyList_CreatesValidExcel()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test Subject";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        // Verify it's valid Excel by loading it
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        workbook.Worksheets.Should().HaveCount(1);
        workbook.Worksheets.First().Name.Should().Be("Meeting Days Report");
    }

    [Fact]
    public void GenerateExcelReport_SingleMonth_ContainsCorrectData()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>
        {
            new MonthlyBreakdown
            {
                Year = 2024,
                Month = 1,
                MonthName = "January",
                TotalMeetingDays = 10,
                WeekdayCount = 8,
                WeekendCount = 1,
                SaturdayCount = 1,
                SundayCount = 0,
                HolidayCount = 1
            }
        };
        var subject = "Team Standup";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Check title
        worksheet.Cell(1, 1).Value.ToString().Should().Contain("Team Standup");

        // Check headers
        worksheet.Cell(4, 1).Value.ToString().Should().Be("Month");
        worksheet.Cell(4, 2).Value.ToString().Should().Be("Year");
        worksheet.Cell(4, 3).Value.ToString().Should().Be("Total Days");
        worksheet.Cell(4, 4).Value.ToString().Should().Be("Weekdays");
        worksheet.Cell(4, 5).Value.ToString().Should().Be("Saturdays");
        worksheet.Cell(4, 6).Value.ToString().Should().Be("Sundays");
        worksheet.Cell(4, 7).Value.ToString().Should().Be("Holidays");

        // Check data row
        worksheet.Cell(5, 1).Value.ToString().Should().Be("January");
        worksheet.Cell(5, 2).GetValue<int>().Should().Be(2024);
        worksheet.Cell(5, 3).GetValue<int>().Should().Be(10);
        worksheet.Cell(5, 4).GetValue<int>().Should().Be(8);
        worksheet.Cell(5, 5).GetValue<int>().Should().Be(1);
        worksheet.Cell(5, 6).GetValue<int>().Should().Be(0);
        worksheet.Cell(5, 7).GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void GenerateExcelReport_MultipleMonths_ContainsAllData()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>
        {
            new MonthlyBreakdown
            {
                Year = 2024,
                Month = 1,
                MonthName = "January",
                TotalMeetingDays = 10,
                WeekdayCount = 8,
                WeekendCount = 1,
                SaturdayCount = 1,
                SundayCount = 0,
                HolidayCount = 1
            },
            new MonthlyBreakdown
            {
                Year = 2024,
                Month = 2,
                MonthName = "February",
                TotalMeetingDays = 8,
                WeekdayCount = 7,
                WeekendCount = 0,
                SaturdayCount = 0,
                SundayCount = 0,
                HolidayCount = 1
            }
        };
        var subject = "All Meetings";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Check first data row
        worksheet.Cell(5, 1).Value.ToString().Should().Be("January");
        worksheet.Cell(5, 2).GetValue<int>().Should().Be(2024);

        // Check second data row
        worksheet.Cell(6, 1).Value.ToString().Should().Be("February");
        worksheet.Cell(6, 2).GetValue<int>().Should().Be(2024);
    }

    [Fact]
    public void GenerateExcelReport_IncludesGeneratedTimestamp()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";
        var beforeGeneration = DateTime.Now.AddSeconds(-1);

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);
        var afterGeneration = DateTime.Now.AddSeconds(1);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var generatedText = worksheet.Cell(2, 1).Value.ToString();
        generatedText.Should().Contain("Generated:");

        // Parse the timestamp from "Generated: 2024-01-15 10:30:45"
        var timestampPart = generatedText.Replace("Generated:", "").Trim();
        var timestamp = DateTime.Parse(timestampPart);

        timestamp.Should().BeOnOrAfter(beforeGeneration);
        timestamp.Should().BeOnOrBefore(afterGeneration);
    }

    [Fact]
    public void GenerateExcelReport_HeadersAreBold()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Check header row styling
        worksheet.Cell(4, 1).Style.Font.Bold.Should().BeTrue();
        worksheet.Cell(4, 2).Style.Font.Bold.Should().BeTrue();
        worksheet.Cell(4, 3).Style.Font.Bold.Should().BeTrue();
    }

    [Fact]
    public void GenerateExcelReport_TitleIsBoldAndLarger()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(1, 1).Style.Font.Bold.Should().BeTrue();
        worksheet.Cell(1, 1).Style.Font.FontSize.Should().Be(14);
    }

    [Fact]
    public void GenerateExcelReport_GeneratedDateIsItalic()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(2, 1).Style.Font.Italic.Should().BeTrue();
    }

    [Fact]
    public void GenerateExcelReport_HeadersHaveGrayBackground()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(4, 1).Style.Fill.BackgroundColor.Should().Be(XLColor.LightGray);
    }

    [Fact]
    public void GenerateExcelReport_HeadersHaveBottomBorder()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(4, 1).Style.Border.BottomBorder.Should().Be(XLBorderStyleValues.Thick);
    }

    [Fact]
    public void GenerateExcelReport_WorksheetNameIsCorrect()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);

        workbook.Worksheets.Should().HaveCount(1);
        workbook.Worksheets.First().Name.Should().Be("Meeting Days Report");
    }

    [Fact]
    public void GenerateExcelReport_HandlesLongSubjectName()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var longSubject = "This is a very long meeting subject name that should still be handled correctly by the Excel export service";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, longSubject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(1, 1).Value.ToString().Should().Contain(longSubject);
    }

    [Fact]
    public void GenerateExcelReport_HandlesSpecialCharactersInSubject()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var specialSubject = "Meeting with @#$% & Special *Characters*";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, specialSubject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(1, 1).Value.ToString().Should().Contain(specialSubject);
    }

    [Fact]
    public void GenerateExcelReport_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>
        {
            new MonthlyBreakdown
            {
                Year = 2024,
                Month = 1,
                MonthName = "January",
                TotalMeetingDays = 5,
                WeekdayCount = 5,
                WeekendCount = 0,
                HolidayCount = 0
            }
        };
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateExcelReport_AllColumnsHaveHeaders()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>();
        var subject = "Test";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Verify all 6 columns have headers
        for (int col = 1; col <= 6; col++)
        {
            worksheet.Cell(4, col).Value.ToString().Should().NotBeNullOrEmpty($"column {col} should have a header");
        }
    }

    [Fact]
    public void GenerateExcelReport_ZeroValues_DisplayedCorrectly()
    {
        // Arrange
        var monthlyData = new List<MonthlyBreakdown>
        {
            new MonthlyBreakdown
            {
                Year = 2024,
                Month = 1,
                MonthName = "January",
                TotalMeetingDays = 0,
                WeekdayCount = 0,
                WeekendCount = 0,
                HolidayCount = 0
            }
        };
        var subject = "Empty Month";

        // Act
        var result = _sut.GenerateExcelReport(monthlyData, subject);

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(5, 3).GetValue<int>().Should().Be(0);
        worksheet.Cell(5, 4).GetValue<int>().Should().Be(0);
        worksheet.Cell(5, 5).GetValue<int>().Should().Be(0);
        worksheet.Cell(5, 6).GetValue<int>().Should().Be(0);
    }
}
