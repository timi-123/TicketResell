using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TicketResell.Models;

public class Order
{
    public int Id { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public ApplicationUser? Buyer { get; set; }
    public int TicketListingId { get; set; }
    public TicketListing? TicketListing { get; set; }
    public int Quantity { get; set; }

    [Precision(18, 2)] public decimal SubtotalPrice { get; set; }
    [Precision(18, 2)] public decimal BuyerFee { get; set; }
    [Precision(18, 2)] public decimal SellerCommission { get; set; }
    [Precision(18, 2)] public decimal TotalPrice { get; set; }

    public DateTime OrderedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    [StringLength(40)] public string? PaymentReference { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public ICollection<TicketFile> TicketFiles { get; set; } = new List<TicketFile>();

    [NotMapped]
    public decimal SellerPayout
    {
        get { return SubtotalPrice - SellerCommission; }
    }
}
