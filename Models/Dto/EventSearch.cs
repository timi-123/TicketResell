using TicketResell.Models;

namespace TicketResell.Models.Dto;

public class EventSearch
{
    public string? Query { get; init; }
    public int? CategoryId { get; init; }
    public string? City { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string Sort { get; init; } = "date";
    public int Page { get; init; } = 1;
    public int PageCount { get; init; } = 1;
    public int TotalCount { get; init; }

    public IReadOnlyList<UpcomingEvent> Results { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public IReadOnlyList<string> Cities { get; init; } = [];

    public bool HasFilters
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Query) || CategoryId != null || !string.IsNullOrWhiteSpace(City)
                   || From != null || To != null || Sort != "date";
        }
    }
}
