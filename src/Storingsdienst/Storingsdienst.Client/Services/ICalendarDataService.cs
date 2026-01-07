using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public interface ICalendarDataService
{
    Task<List<CalendarEventDto>> GetMeetingsBySubjectAsync(
        string subjectFilter,
        DateTime startDate,
        DateTime endDate);
}
