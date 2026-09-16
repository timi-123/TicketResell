using TicketResell.Models;

namespace TicketResell.Services;

public static class Reservations
{
    public static void ReturnTickets(Order order, TicketListing listing)
    {
        listing.Quantity += order.Quantity;

        if (listing.Status == ListingStatus.Sold && listing.Event?.StartsAt > DateTime.Now)
        {
            listing.Status = ListingStatus.Active;
        }
    }
}
