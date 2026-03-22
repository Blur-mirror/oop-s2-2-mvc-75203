using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class InspectionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<InspectionController> _logger;

    public InspectionController(ApplicationDbContext db, ILogger<InspectionController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /Inspection
    public async Task<IActionResult> Index()
    {
        var inspections = await _db.Inspections
            .Include(i => i.Premises)
            .OrderByDescending(i => i.InspectionDate)
            .ToListAsync();
        return View(inspections);
    }

    // GET: /Inspection/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var inspection = await _db.Inspections
            .Include(i => i.Premises)
            .Include(i => i.FollowUps)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inspection is null) return NotFound();
        return View(inspection);
    }

    // GET: /Inspection/Create
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(int? premisesId)
    {
        ViewBag.PremisesList = new SelectList(
            await _db.Premises.OrderBy(p => p.Name).ToListAsync(),
            "Id", "Name", premisesId);
        return View(new Inspection { InspectionDate = DateTime.Today, PremisesId = premisesId ?? 0 });
    }

    // POST: /Inspection/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(Inspection model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.PremisesList = new SelectList(
                await _db.Premises.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", model.PremisesId);
            return View(model);
        }

        try
        {
            _db.Inspections.Add(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Inspection created: {InspectionId} for PremisesId {PremisesId} on {Date} - Outcome: {Outcome}",
                model.Id, model.PremisesId, model.InspectionDate.ToString("yyyy-MM-dd"), model.Outcome);
            return RedirectToAction("Details", "Premises", new { id = model.PremisesId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection for PremisesId {PremisesId}", model.PremisesId);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            ViewBag.PremisesList = new SelectList(
                await _db.Premises.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", model.PremisesId);
            return View(model);
        }
    }

    // GET: /Inspection/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var inspection = await _db.Inspections.FindAsync(id);
        if (inspection is null) return NotFound();

        ViewBag.PremisesList = new SelectList(
            await _db.Premises.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    // POST: /Inspection/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Inspection model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.PremisesList = new SelectList(
                await _db.Premises.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", model.PremisesId);
            return View(model);
        }

        try
        {
            _db.Inspections.Update(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Inspection updated: {InspectionId} for PremisesId {PremisesId}",
                model.Id, model.PremisesId);
            return RedirectToAction("Details", "Premises", new { id = model.PremisesId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inspection {InspectionId}", id);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            ViewBag.PremisesList = new SelectList(
                await _db.Premises.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", model.PremisesId);
            return View(model);
        }
    }

    // GET: /Inspection/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var inspection = await _db.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inspection is null) return NotFound();
        return View(inspection);
    }

    // POST: /Inspection/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var inspection = await _db.Inspections.FindAsync(id);
            if (inspection is null) return NotFound();
            var premisesId = inspection.PremisesId;
            _db.Inspections.Remove(inspection);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Inspection deleted: {InspectionId}", id);
            return RedirectToAction("Details", "Premises", new { id = premisesId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspection {InspectionId}", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
