using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public interface IMeetingAnalysisService
{
    List<MonthlyBreakdown> AnalyzeMeetings(List<CalendarEventDto> events);
}
