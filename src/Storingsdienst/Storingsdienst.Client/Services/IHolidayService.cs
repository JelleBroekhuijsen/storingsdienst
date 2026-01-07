namespace Storingsdienst.Client.Services;

public interface IHolidayService
{
    bool IsDutchHoliday(DateOnly date);
}
