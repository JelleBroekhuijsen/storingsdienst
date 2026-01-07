using Microsoft.Graph;
using Microsoft.Graph.Models;
using Storingsdienst.Client.Models;

namespace Storingsdienst.Client.Services;

public class GraphService : ICalendarDataService
{
    private readonly GraphServiceClient _graphClient;

    public GraphService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<List<CalendarEventDto>> GetMeetingsBySubjectAsync(
        string subjectFilter,
        DateTime startDate,
        DateTime endDate)
    {
        var results = new List<CalendarEventDto>();

        try
        {
            // Use calendar view to get events in date range
            var calendarView = await _graphClient.Me.CalendarView
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.StartDateTime = startDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    requestConfiguration.QueryParameters.EndDateTime = endDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    requestConfiguration.QueryParameters.Select = new[] { "id", "subject", "start", "end", "isAllDay" };
                    requestConfiguration.QueryParameters.Top = 1000;

                    // Apply subject filter if provided
                    if (!string.IsNullOrWhiteSpace(subjectFilter))
                    {
                        requestConfiguration.QueryParameters.Filter = $"contains(subject, '{subjectFilter}')";
                    }
                });

            if (calendarView?.Value == null)
            {
                return results;
            }

            // Map to our DTO
            foreach (var evt in calendarView.Value)
            {
                if (evt.Start == null || evt.End == null)
                {
                    continue;
                }

                results.Add(new CalendarEventDto
                {
                    Id = evt.Id ?? string.Empty,
                    Subject = evt.Subject ?? string.Empty,
                    StartDateTime = DateTime.Parse(evt.Start.DateTime),
                    EndDateTime = DateTime.Parse(evt.End.DateTime),
                    IsAllDay = evt.IsAllDay ?? false
                });
            }

            // Handle pagination if needed
            var pageIterator = PageIterator<Event, EventCollectionResponse>
                .CreatePageIterator(
                    _graphClient,
                    calendarView,
                    (evt) =>
                    {
                        if (evt.Start != null && evt.End != null)
                        {
                            results.Add(new CalendarEventDto
                            {
                                Id = evt.Id ?? string.Empty,
                                Subject = evt.Subject ?? string.Empty,
                                StartDateTime = DateTime.Parse(evt.Start.DateTime),
                                EndDateTime = DateTime.Parse(evt.End.DateTime),
                                IsAllDay = evt.IsAllDay ?? false
                            });
                        }
                        return true; // Continue iteration
                    });

            await pageIterator.IterateAsync();
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 429)
        {
            // Throttling
            throw new InvalidOperationException("Too many requests. Please wait and try again.", ex);
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 401)
        {
            // Unauthorized
            throw new UnauthorizedAccessException("Please sign in again.", ex);
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 403)
        {
            // Forbidden
            throw new UnauthorizedAccessException("Insufficient permissions. Admin consent may be required.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Network error. Please check your internet connection.", ex);
        }

        return results;
    }
}
