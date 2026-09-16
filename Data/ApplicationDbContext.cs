using Microsoft.EntityFrameworkCore;
using TicketResell.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace TicketResell.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Venue> Venues { get; set; } = null!;
    public DbSet<TicketListing> TicketListings { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<TicketFile> TicketFiles { get; set; } = null!;
}
