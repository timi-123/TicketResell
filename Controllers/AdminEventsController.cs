using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;

namespace TicketResell.Controllers;

[Authorize(Roles = DbSeeder.AdminRole)]
[Route("admin/events")]
public class AdminEventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminEventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .OrderBy(e => e.StartsAt)
            .ToListAsync();

        return View(events);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var venues = await _context.Venues.OrderBy(v => v.Name).ToListAsync();
        ViewData["VenueId"] = venues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToList();

        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewData["CategoryId"] = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

        return View("Form", new Event { StartsAt = DateTime.Today.AddDays(7).AddHours(20) });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,StartsAt,VenueId,CategoryId")] Event model)
    {
        ModelState.Remove(nameof(Event.Venue));
        ModelState.Remove(nameof(Event.Category));

        if (!ModelState.IsValid)
        {
            var venues = await _context.Venues.OrderBy(v => v.Name).ToListAsync();
            ViewData["VenueId"] = venues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToList();

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewData["CategoryId"] = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            return View("Form", model);
        }

        _context.Events.Add(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Event \"{model.Title}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _context.Events.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        var venues = await _context.Venues.OrderBy(v => v.Name).ToListAsync();
        ViewData["VenueId"] = venues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToList();

        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewData["CategoryId"] = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

        return View("Form", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartsAt,VenueId,CategoryId")] Event model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Event.Venue));
        ModelState.Remove(nameof(Event.Category));

        if (!ModelState.IsValid)
        {
            var venues = await _context.Venues.OrderBy(v => v.Name).ToListAsync();
            ViewData["VenueId"] = venues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToList();

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewData["CategoryId"] = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            return View("Form", model);
        }

        var existing = await _context.Events.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Title = model.Title;
        existing.Description = model.Description;
        existing.StartsAt = model.StartsAt;
        existing.VenueId = model.VenueId;
        existing.CategoryId = model.CategoryId;

        await _context.SaveChangesAsync();

        TempData["Message"] = $"Event \"{existing.Title}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _context.Events.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        if (await _context.TicketListings.AnyAsync(l => l.EventId == id))
        {
            TempData["Error"] = "That event has listings, so it can't be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.Events.Remove(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Event \"{model.Title}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
