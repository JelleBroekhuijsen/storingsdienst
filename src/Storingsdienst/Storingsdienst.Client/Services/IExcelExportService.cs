using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public interface IExcelExportService
{
    byte[] GenerateExcelReport(List<MonthlyBreakdown> monthlyData, string meetingSubject);
}
