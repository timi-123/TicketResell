using System.ComponentModel.DataAnnotations;

namespace TicketResell.Models;

public class Event
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public Venue? Venue { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    [Required]
    public DateTime StartsAt { get; set; }
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;
}