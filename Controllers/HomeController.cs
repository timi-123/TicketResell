using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;
using TicketResell.Models.Dto;

namespace TicketResell.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Where(e => e.StartsAt > now)
            .OrderBy(e => e.StartsAt)
            .ToListAsync();

        var listings = await _context.TicketListings
            .Where(l => l.Status == ListingStatus.Active && l.Quantity > 0 && l.Event!.StartsAt > now)
            .Select(l => new { l.EventId, l.Quantity, l.PricePerTicket })
            .ToListAsync();

        var upcoming = events.Select(e =>
        {
            var forEvent = listings.Where(l => l.EventId == e.Id).ToList();
            return new UpcomingEvent
            {
                Event = e,
                TicketsAvailable = forEvent.Sum(l => l.Quantity),
                LowestPrice = forEvent.Count > 0 ? forEvent.Min(l => l.PricePerTicket) : null
            };
        }).ToList();

        return View(upcoming);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
