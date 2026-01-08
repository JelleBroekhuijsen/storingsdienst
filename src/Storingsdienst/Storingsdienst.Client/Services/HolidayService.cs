namespace Storingsdienst.Client.Services;

public class HolidayService : IHolidayService
{
    private readonly Dictionary<int, HashSet<DateOnly>> _holidayCache = new();
    private readonly Dictionary<int, Dictionary<DateOnly, string>> _holidayNamesCache = new();

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

    public string? GetHolidayName(DateOnly date)
    {
        // Ensure holidays are cached for this year
        if (!_holidayNamesCache.TryGetValue(date.Year, out var holidayNames))
        {
            // This will populate both caches
            GetDutchHolidays(date.Year);
            holidayNames = _holidayNamesCache[date.Year];
        }

        return holidayNames.TryGetValue(date, out var name) ? name : null;
    }

    private HashSet<DateOnly> GetDutchHolidays(int year)
    {
        var holidays = new HashSet<DateOnly>();
        var holidayNames = new Dictionary<DateOnly, string>();

        // Fixed holidays
        AddHoliday(new DateOnly(year, 1, 1), "Nieuwjaarsdag", holidays, holidayNames);
        AddHoliday(new DateOnly(year, 4, 27), "Koningsdag", holidays, holidayNames);
        AddHoliday(new DateOnly(year, 12, 25), "Eerste Kerstdag", holidays, holidayNames);
        AddHoliday(new DateOnly(year, 12, 26), "Tweede Kerstdag", holidays, holidayNames);

        // Calculate Easter and related holidays (using simplified algorithm)
        var easter = CalculateEaster(year);
        AddHoliday(easter.AddDays(-2), "Goede Vrijdag", holidays, holidayNames);
        AddHoliday(easter, "Eerste Paasdag", holidays, holidayNames);
        AddHoliday(easter.AddDays(1), "Tweede Paasdag", holidays, holidayNames);
        AddHoliday(easter.AddDays(39), "Hemelvaartsdag", holidays, holidayNames);
        AddHoliday(easter.AddDays(49), "Eerste Pinksterdag", holidays, holidayNames);
        AddHoliday(easter.AddDays(50), "Tweede Pinksterdag", holidays, holidayNames);

        // Bevrijdingsdag (Liberation Day) - May 5, every 5 years or always celebrated
        AddHoliday(new DateOnly(year, 5, 5), "Bevrijdingsdag", holidays, holidayNames);

        // Cache the names
        _holidayNamesCache[year] = holidayNames;

        return holidays;
    }

    private void AddHoliday(DateOnly date, string name, HashSet<DateOnly> holidays, Dictionary<DateOnly, string> holidayNames)
    {
        holidays.Add(date);
        holidayNames[date] = name;
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

