namespace Storingsdienst.Client.Services;

public class HolidayService : IHolidayService
{
    private readonly Dictionary<int, HashSet<DateOnly>> _holidayCache = new();

    public bool IsDutchHoliday(DateOnly date)
    {
        // Check if we have holidays cached for this year
        if (!_holidayCache.TryGetValue(date.Year, out var holidays))
        {
            // Get Dutch public holidays for this year
            // Using hardcoded Dutch holidays for now - will integrate Nager.Date properly later
            holidays = GetDutchHolidays(date.Year);
            _holidayCache[date.Year] = holidays;
        }

        return holidays.Contains(date);
    }

    private HashSet<DateOnly> GetDutchHolidays(int year)
    {
        var holidays = new HashSet<DateOnly>
        {
            // Fixed holidays
            new DateOnly(year, 1, 1),   // Nieuwjaarsdag (New Year's Day)
            new DateOnly(year, 4, 27),  // Koningsdag (King's Day)
            new DateOnly(year, 12, 25), // Eerste Kerstdag (Christmas Day)
            new DateOnly(year, 12, 26)  // Tweede Kerstdag (Boxing Day)
        };

        // Calculate Easter and related holidays (using simplified algorithm)
        var easter = CalculateEaster(year);
        holidays.Add(easter.AddDays(-2));  // Goede Vrijdag (Good Friday)
        holidays.Add(easter);              // Pasen (Easter Sunday)
        holidays.Add(easter.AddDays(1));   // Paasmaandag (Easter Monday)
        holidays.Add(easter.AddDays(39));  // Hemelvaart (Ascension Day)
        holidays.Add(easter.AddDays(49));  // Pinksteren (Whit Sunday)
        holidays.Add(easter.AddDays(50));  // Pinkstermaandag (Whit Monday)

        // Bevrijdingsdag (Liberation Day) - May 5, every 5 years or always celebrated
        holidays.Add(new DateOnly(year, 5, 5));

        return holidays;
    }

    private DateOnly CalculateEaster(int year)
    {
        // Meeus/Jones/Butcher algorithm for Easter calculation
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }
}

