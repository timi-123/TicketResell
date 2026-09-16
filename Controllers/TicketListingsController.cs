using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;

namespace TicketResell.Controllers
{
    [Authorize]
    public class TicketListingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketListingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        // GET: TicketListings
        public async Task<IActionResult> Index(int? eventId)
        {
            if (eventId != null)
            {
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var now = DateTime.Now;
            var listings = await _context.TicketListings
                .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
                .Include(l => l.Seller)
                .Where(l => l.Status == ListingStatus.Active && l.Quantity > 0 && l.Event!.StartsAt > now)
                .OrderBy(l => l.Event!.StartsAt)
                .ThenBy(l => l.PricePerTicket)
                .ToListAsync();

            return View(listings);
        }

        [AllowAnonymous]
        // GET: TicketListings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketListing = await _context.TicketListings
                .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id.Value);
            if (ticketListing == null)
            {
                return NotFound();
            }

            return View(ticketListing);
        }

        // GET: TicketListings/Create
        public async Task<IActionResult> Create(int? eventId)
        {
            var now = DateTime.Now;
            var events = await _context.Events
                .Where(e => e.StartsAt > now)
                .OrderBy(e => e.StartsAt)
                .ToListAsync();

            ViewData["EventId"] = events
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Title} ({e.StartsAt:d MMM yyyy})" })
                .ToList();

            return View();
        }

        // POST: TicketListings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,PricePerTicket,Quantity,Section,Row")] TicketListing ticketListing)
        {
            ModelState.Remove(nameof(TicketListing.SellerId));
            ModelState.Remove(nameof(TicketListing.Event));
            ModelState.Remove(nameof(TicketListing.Seller));

            var eventIsUpcoming = await _context.Events.AnyAsync(e => e.Id == ticketListing.EventId && e.StartsAt > DateTime.Now);
            if (!eventIsUpcoming)
            {
                ModelState.AddModelError(nameof(TicketListing.EventId), "Choose an event that hasn't started yet.");
            }

            if (ModelState.IsValid)
            {
                ticketListing.SellerId = _userManager.GetUserId(User)!;
                ticketListing.CreatedAt = DateTime.Now;
                ticketListing.Status    = ListingStatus.Active;

                _context.Add(ticketListing);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Your tickets are now listed for sale.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.Now;
            var events = await _context.Events
                .Where(e => e.StartsAt > now)
                .OrderBy(e => e.StartsAt)
                .ToListAsync();

            ViewData["EventId"] = events
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Title} ({e.StartsAt:d MMM yyyy})" })
                .ToList();

            return View(ticketListing);
        }

        // GET: TicketListings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.TicketListings
                .Include(l => l.Event)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            if (listing.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (listing.Status == ListingStatus.Cancelled || listing.Event!.StartsAt <= DateTime.Now)
            {
                TempData["Error"] = "This listing can no longer be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var now = DateTime.Now;
            var events = await _context.Events
                .Where(e => e.StartsAt > now)
                .OrderBy(e => e.StartsAt)
                .ToListAsync();

            ViewData["EventId"] = events
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Title} ({e.StartsAt:d MMM yyyy})" })
                .ToList();

            return View(listing);
        }

        // POST: TicketListings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventId,PricePerTicket,Quantity,Section,Row")] TicketListing ticketListing)
        {
            if (id != ticketListing.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(TicketListing.SellerId));

            var listing = await _context.TicketListings
                .Include(l => l.Event)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            if (listing.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (listing.Status == ListingStatus.Cancelled || listing.Event!.StartsAt <= DateTime.Now)
            {
                TempData["Error"] = "This listing can no longer be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var eventIsUpcoming = await _context.Events.AnyAsync(e => e.Id == ticketListing.EventId && e.StartsAt > DateTime.Now);
            if (!eventIsUpcoming)
            {
                ModelState.AddModelError(nameof(TicketListing.EventId), "Choose an event that hasn't started yet.");
            }

            if (ticketListing.EventId != listing.EventId &&
                await _context.Orders.AnyAsync(o => o.TicketListingId == id))
            {
                ModelState.AddModelError(nameof(TicketListing.EventId),
                    "Tickets from this listing have already been sold, so its event can't be changed.");
            }

            if (ModelState.IsValid)
            {
                listing.EventId        = ticketListing.EventId;
                listing.PricePerTicket = ticketListing.PricePerTicket;
                listing.Quantity       = ticketListing.Quantity;
                listing.Section        = ticketListing.Section;
                listing.Row            = ticketListing.Row;
                listing.Status         = ListingStatus.Active;

                await _context.SaveChangesAsync();

                TempData["Message"] = "Listing updated.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var now = DateTime.Now;
            var events = await _context.Events
                .Where(e => e.StartsAt > now)
                .OrderBy(e => e.StartsAt)
                .ToListAsync();

            ViewData["EventId"] = events
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.Title} ({e.StartsAt:d MMM yyyy})" })
                .ToList();

            return View(ticketListing);
        }

        // GET: TicketListings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketListing = await _context.TicketListings
                .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id.Value);
            if (ticketListing == null)
            {
                return NotFound();
            }

            if (ticketListing.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (ticketListing.Status != ListingStatus.Active)
            {
                TempData["Error"] = "Only listings that are still on sale can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(ticketListing);
        }

        // POST: TicketListings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _context.TicketListings.FindAsync(id);
            if (listing == null) return NotFound();

            if (listing.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (listing.Status != ListingStatus.Active)
            {
                TempData["Error"] = "Only listings that are still on sale can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            listing.Status = ListingStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Listing cancelled. Orders already placed on it are kept.";
            return RedirectToAction(nameof(Index));
        }
    }
}
