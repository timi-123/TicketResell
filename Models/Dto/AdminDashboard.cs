namespace TicketResell.Models.Dto;

public class AdminDashboard
{
    public int UpcomingEvents { get; init; }
    public int TotalEvents { get; init; }
    public int Venues { get; init; }
    public int Categories { get; init; }
    public int ActiveListings { get; init; }
    public int Users { get; init; }
    public int PaidOrders { get; init; }
    public int PendingOrders { get; init; }
    public decimal GrossSales { get; init; }
    public decimal Commission { get; init; }
}
