using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;
using TicketResell.Models.Dto;

namespace TicketResell.Controllers;

[Authorize(Roles = DbSeeder.AdminRole)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        var settled = await _context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Paid || o.OrderStatus == OrderStatus.Delivered)
            .Select(o => new { o.TotalPrice, o.SellerCommission })
            .ToListAsync();

        var model = new AdminDashboard
        {
            UpcomingEvents = await _context.Events.CountAsync(e => e.StartsAt > now),
            TotalEvents = await _context.Events.CountAsync(),
            Venues = await _context.Venues.CountAsync(),
            Categories = await _context.Categories.CountAsync(),
            ActiveListings = await _context.TicketListings.CountAsync(l => l.Status == ListingStatus.Active && l.Quantity > 0),
            Users = await _context.Users.CountAsync(),
            PaidOrders = settled.Count,
            PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending),
            GrossSales = settled.Sum(o => o.TotalPrice),
            Commission = settled.Sum(o => o.SellerCommission)
        };

        return View(model);
    }

    [HttpGet("listings")]
    public async Task<IActionResult> Listings()
    {
        var listings = await _context.TicketListings
            .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
            .Include(l => l.Seller)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return View(listings);
    }

    [HttpPost("listings/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelListing(int id)
    {
        var listing = await _context.TicketListings.FindAsync(id);
        if (listing == null)
        {
            return NotFound();
        }

        if (listing.Status != ListingStatus.Active)
        {
            TempData["Error"] = "That listing is not on sale.";
            return RedirectToAction(nameof(Listings));
        }

        listing.Status = ListingStatus.Cancelled;
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Listing #{id} cancelled.";
        return RedirectToAction(nameof(Listings));
    }
}
