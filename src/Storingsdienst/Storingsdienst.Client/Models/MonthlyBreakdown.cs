namespace Storingsdienst.Client.Models;

public class MonthlyBreakdown
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalMeetingDays { get; set; }
    public int WeekdayCount { get; set; }
    public int WeekendCount { get; set; }
    public int HolidayCount { get; set; }
}
