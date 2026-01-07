namespace Storingsdienst.Client.Models;

public class CalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
}
