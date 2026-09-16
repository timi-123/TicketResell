using TicketResell.Models;

namespace TicketResell.Models.Dto;

public class EventPage
{
    public required Event Event { get; init; }
    public IReadOnlyList<TicketListing> Offers { get; init; } = [];
    public int MinQuantity { get; init; } = 1;
    public int TotalOffers { get; init; }
    public int TicketsAvailable { get; init; }
    public decimal? LowestPrice { get; init; }
    public bool IsUpcoming { get; init; }
}
