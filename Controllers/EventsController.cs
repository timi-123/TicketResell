using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;
using TicketResell.Models.Dto;

namespace TicketResell.Controllers;

public class EventsController : Controller
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, int? categoryId, string? city,
        DateTime? from, DateTime? to, string? sort, int page = 1)
    {
        var now = DateTime.Now;

        var query = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Where(e => e.StartsAt > now);

        var term = q?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term}%";
            query = query.Where(e =>
                EF.Functions.Like(e.Title, pattern) ||
                EF.Functions.Like(e.Description, pattern) ||
                EF.Functions.Like(e.Venue!.Name, pattern) ||
                EF.Functions.Like(e.Venue!.City, pattern) ||
                EF.Functions.Like(e.Category!.Name, pattern));
        }

        if (categoryId != null)
        {
            query = query.Where(e => e.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(e => e.Venue!.City == city);
        }

        if (from != null)
        {
            query = query.Where(e => e.StartsAt >= from.Value.Date);
        }

        if (to != null)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(e => e.StartsAt < end);
        }

        var matched = await query.OrderBy(e => e.StartsAt).ToListAsync();

        var matchedIds = matched.Select(e => e.Id).ToList();
        var matchedListings = await _context.TicketListings
            .Where(l => matchedIds.Contains(l.EventId) && l.Status == ListingStatus.Active && l.Quantity > 0)
            .Select(l => new { l.EventId, l.Quantity, l.PricePerTicket })
            .ToListAsync();

        var summaries = matched.Select(e =>
        {
            var forEvent = matchedListings.Where(l => l.EventId == e.Id).ToList();
            return new UpcomingEvent
            {
                Event = e,
                TicketsAvailable = forEvent.Sum(l => l.Quantity),
                LowestPrice = forEvent.Count > 0 ? forEvent.Min(l => l.PricePerTicket) : null
            };
        }).ToList();

        sort = sort == "price" ? "price" : "date";
        if (sort == "price")
        {
            summaries = summaries
                .OrderBy(s => s.LowestPrice == null)
                .ThenBy(s => s.LowestPrice)
                .ThenBy(s => s.Event.StartsAt)
                .ToList();
        }

        var pageCount = Math.Max(1, (int)Math.Ceiling(summaries.Count / (double)PageSize));
        page = Math.Clamp(page, 1, pageCount);

        var model = new EventSearch
        {
            Query = term,
            CategoryId = categoryId,
            City = city,
            From = from,
            To = to,
            Sort = sort,
            Page = page,
            PageCount = pageCount,
            TotalCount = summaries.Count,
            Results = summaries.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
            Cities = await _context.Venues
                .Where(v => v.Events.Any(e => e.StartsAt > now))
                .Select(v => v.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int? id, int minQuantity = 1)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticketEvent = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ticketEvent == null)
        {
            return NotFound();
        }

        var offers = await _context.TicketListings
            .Include(l => l.Seller)
            .Where(l => l.EventId == id && l.Status == ListingStatus.Active && l.Quantity > 0)
            .ToListAsync();

        var isUpcoming = ticketEvent.StartsAt > DateTime.Now;
        if (!isUpcoming)
        {
            offers = [];
        }

        minQuantity = Math.Clamp(minQuantity, 1, 10);
        var filtered = offers
            .Where(l => l.Quantity >= minQuantity)
            .OrderBy(l => l.PricePerTicket)
            .ToList();

        var model = new EventPage
        {
            Event = ticketEvent,
            Offers = filtered,
            MinQuantity = minQuantity,
            TotalOffers = offers.Count,
            TicketsAvailable = offers.Sum(l => l.Quantity),
            LowestPrice = offers.Count > 0 ? offers.Min(l => l.PricePerTicket) : null,
            IsUpcoming = isUpcoming
        };

        return View(model);
    }
}
