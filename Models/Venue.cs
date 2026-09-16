using System.ComponentModel.DataAnnotations;

namespace TicketResell.Models;

public class Venue
{
    public int Id { get; set; }
    [Required]
    [StringLength(40)] public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    public string Address { get; set; } = string.Empty;
    [StringLength(30)]
    public string City { get; set; } = string.Empty;
    [Required]
    public int Capacity { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}