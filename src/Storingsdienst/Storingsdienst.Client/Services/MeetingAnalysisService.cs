using System.Globalization;
using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public class MeetingAnalysisService : IMeetingAnalysisService
{
    private readonly IHolidayService _holidayService;

    public MeetingAnalysisService(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    public List<MonthlyBreakdown> AnalyzeMeetings(List<CalendarEventDto> events)
    {
        // Extract unique days from events (handling multi-day events)
        var meetingDaysByMonth = new Dictionary<(int Year, int Month), HashSet<DateOnly>>();

        foreach (var evt in events)
        {
            var currentDay = evt.StartDateTime.Date;
            var endDay = evt.EndDateTime.Date;

            // Iterate through each day of the event
            while (currentDay <= endDay)
            {
                var dateOnly = DateOnly.FromDateTime(currentDay);
                var key = (dateOnly.Year, dateOnly.Month);

                if (!meetingDaysByMonth.ContainsKey(key))
                {
                    meetingDaysByMonth[key] = new HashSet<DateOnly>();
                }

                meetingDaysByMonth[key].Add(dateOnly);
                currentDay = currentDay.AddDays(1);
            }
        }

        // Generate monthly breakdown
        var results = new List<MonthlyBreakdown>();

        foreach (var ((year, month), days) in meetingDaysByMonth.OrderByDescending(x => x.Key.Year).ThenByDescending(x => x.Key.Month))
        {
            var weekdayCount = 0;
            var weekendCount = 0;
            var holidayCount = 0;

            foreach (var day in days)
            {
                var category = CategorizeDay(day);

                switch (category)
                {
                    case DayCategory.Holiday:
                        holidayCount++;
                        break;
                    case DayCategory.Weekend:
                        weekendCount++;
                        break;
                    case DayCategory.Weekday:
                        weekdayCount++;
                        break;
                }
            }

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            results.Add(new MonthlyBreakdown
            {
                Year = year,
                Month = month,
                MonthName = monthName,
                TotalMeetingDays = days.Count,
                WeekdayCount = weekdayCount,
                WeekendCount = weekendCount,
                HolidayCount = holidayCount
            });
        }

        return results;
    }

    private DayCategory CategorizeDay(DateOnly date)
    {
        // Check if it's a Dutch holiday first
        if (_holidayService.IsDutchHoliday(date))
        {
            return DayCategory.Holiday;
        }

        // Check if it's a weekend (Saturday or Sunday)
        var dayOfWeek = date.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
        {
            return DayCategory.Weekend;
        }

        return DayCategory.Weekday;
    }
}
