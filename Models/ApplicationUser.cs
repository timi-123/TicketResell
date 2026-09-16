using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TicketResell.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; } =  string.Empty;

    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
}