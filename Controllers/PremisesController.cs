using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class PremisesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PremisesController> _logger;

    public PremisesController(ApplicationDbContext db, ILogger<PremisesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /Premises
    public async Task<IActionResult> Index()
    {
        var premises = await _db.Premises
            .OrderBy(p => p.Town)
            .ThenBy(p => p.Name)
            .ToListAsync();
        return View(premises);
    }

    // GET: /Premises/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var premises = await _db.Premises
            .Include(p => p.Inspections)
                .ThenInclude(i => i.FollowUps)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (premises is null) return NotFound();
        return View(premises);
    }

    // GET: /Premises/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    // POST: /Premises/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Premises model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            _db.Premises.Add(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Premises created: {PremisesId} - {Name} in {Town}",
                model.Id, model.Name, model.Town);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premises {Name}", model.Name);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            return View(model);
        }
    }

    // GET: /Premises/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var premises = await _db.Premises.FindAsync(id);
        if (premises is null) return NotFound();
        return View(premises);
    }

    // POST: /Premises/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Premises model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        try
        {
            _db.Premises.Update(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Premises updated: {PremisesId} - {Name}", model.Id, model.Name);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating premises {PremisesId}", id);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            return View(model);
        }
    }

    // GET: /Premises/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var premises = await _db.Premises.FindAsync(id);
        if (premises is null) return NotFound();
        return View(premises);
    }

    // POST: /Premises/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var premises = await _db.Premises.FindAsync(id);
            if (premises is null) return NotFound();

            _db.Premises.Remove(premises);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Premises deleted: {PremisesId}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting premises {PremisesId}", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
