using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;

namespace TicketResell.Controllers;

[Authorize(Roles = DbSeeder.AdminRole)]
[Route("admin/categories")]
public class AdminCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Events)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("Form", new Category());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Category model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _context.Categories.Add(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Category \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _context.Categories.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View("Form", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var existing = await _context.Categories.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = model.Name;
        existing.Description = model.Description;

        await _context.SaveChangesAsync();

        TempData["Message"] = $"Category \"{existing.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _context.Categories.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        if (await _context.Events.AnyAsync(e => e.CategoryId == id))
        {
            TempData["Error"] = "That category has events, so it can't be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(model);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Category \"{model.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
