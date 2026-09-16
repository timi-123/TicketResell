using TicketResell.Models;

namespace TicketResell.Models.Dto;

public class OrderView
{
    public required Order Order { get; init; }
    public bool IsBuyer { get; init; }
    public bool IsSeller { get; init; }
}
