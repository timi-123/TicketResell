using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;

namespace TicketResell.Controllers;

[Authorize(Roles = DbSeeder.AdminRole)]
[Route("admin/venues")]
public class AdminVenuesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminVenuesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues
            .Include(v => v.Events)
            .OrderBy(v => v.City)
            .ThenBy(v => v.Name)
            .ToListAsync();

        return View(venues);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("Form", new Venue());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Address,City,Capacity")] Venue model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _context.Venues.Add(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Venue \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _context.Venues.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View("Form", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,City,Capacity")] Venue model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var existing = await _context.Venues.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = model.Name;
        existing.Address = model.Address;
        existing.City = model.City;
        existing.Capacity = model.Capacity;

        await _context.SaveChangesAsync();

        TempData["Message"] = $"Venue \"{existing.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _context.Venues.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        if (await _context.Events.AnyAsync(e => e.VenueId == id))
        {
            TempData["Error"] = "That venue has events, so it can't be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.Venues.Remove(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Venue \"{model.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
