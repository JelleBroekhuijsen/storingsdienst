using ClosedXML.Excel;
using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] GenerateExcelReport(List<MonthlyBreakdown> monthlyData, string meetingSubject)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Meeting Days Report");

        // Title
        worksheet.Cell(1, 1).Value = $"Meeting Days Report: {meetingSubject}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Generated date
        worksheet.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        worksheet.Cell(2, 1).Style.Font.Italic = true;

        // Headers (Row 4)
        var headerRow = 4;
        worksheet.Cell(headerRow, 1).Value = "Month";
        worksheet.Cell(headerRow, 2).Value = "Year";
        worksheet.Cell(headerRow, 3).Value = "Total Days";
        worksheet.Cell(headerRow, 4).Value = "Weekdays";
        worksheet.Cell(headerRow, 5).Value = "Weekends";
        worksheet.Cell(headerRow, 6).Value = "Holidays";

        // Style headers
        var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;

        // Data rows
        var currentRow = headerRow + 1;
        foreach (var data in monthlyData)
        {
            worksheet.Cell(currentRow, 1).Value = data.MonthName;
            worksheet.Cell(currentRow, 2).Value = data.Year;
            worksheet.Cell(currentRow, 3).Value = data.TotalMeetingDays;
            worksheet.Cell(currentRow, 4).Value = data.WeekdayCount;
            worksheet.Cell(currentRow, 5).Value = data.WeekendCount;
            worksheet.Cell(currentRow, 6).Value = data.HolidayCount;
            currentRow++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to memory stream and return bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
