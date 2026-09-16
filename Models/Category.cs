using System.ComponentModel.DataAnnotations;

namespace TicketResell.Models;

public class Category
{
    public int Id { get; set; }
    [StringLength(20)] [Required] public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}