using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TicketResell.Models;

public class TicketListing
{
    public int Id { get; set; }
    [Display(Name = "Event")]
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string SellerId { get; set; } = string.Empty;
    public ApplicationUser? Seller { get; set; }
    [Precision(18, 2)]
    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "Price must be between {1} and {2}.")]
    [Display(Name = "Price per ticket")]
    public decimal PricePerTicket { get; set; }
    [Range(1, 100, ErrorMessage = "Quantity must be between {1} and {2}.")]
    [Display(Name = "Tickets")]
    [ConcurrencyCheck]
    public int Quantity { get; set; }
    public string? Section { get; set; }
    public string? Row { get; set; }
    public ListingStatus Status { get; set; }
    [Display(Name = "Listed")]
    public DateTime CreatedAt { get; set; }

    [NotMapped]
    public string SeatDescription
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Section)) parts.Add($"Section {Section}");
            if (!string.IsNullOrWhiteSpace(Row)) parts.Add($"Row {Row}");
            return parts.Count == 0 ? "Unreserved seating" : string.Join(", ", parts);
        }
    }
}
