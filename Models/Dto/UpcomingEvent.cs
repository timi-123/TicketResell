using TicketResell.Models;

namespace TicketResell.Models.Dto;

public class UpcomingEvent
{
    public required Event Event { get; init; }
    public int TicketsAvailable { get; init; }
    public decimal? LowestPrice { get; init; }
}
