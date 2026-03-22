using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class FollowUpController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<FollowUpController> _logger;

    public FollowUpController(ApplicationDbContext db, ILogger<FollowUpController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /FollowUp/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var followUp = await _db.FollowUps
            .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (followUp is null) return NotFound();
        return View(followUp);
    }

    // GET: /FollowUp/Create?inspectionId=5
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(int inspectionId)
    {
        var inspection = await _db.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(i => i.Id == inspectionId);

        if (inspection is null) return NotFound();

        ViewBag.Inspection = inspection;
        return View(new FollowUp { InspectionId = inspectionId, DueDate = DateTime.Today.AddDays(7) });
    }

    // POST: /FollowUp/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(FollowUp model)
    {
        // Business rule: due date must be after inspection date
        var inspection = await _db.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(i => i.Id == model.InspectionId);

        if (inspection is null) return NotFound();

        if (model.DueDate.Date <= inspection.InspectionDate.Date)
        {
            _logger.LogWarning("FollowUp creation rejected: DueDate {DueDate} is not after InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                model.DueDate, inspection.InspectionDate, model.InspectionId);
            ModelState.AddModelError("DueDate", "Due date must be after the inspection date.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Inspection = inspection;
            return View(model);
        }

        try
        {
            _db.FollowUps.Add(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("FollowUp created: {FollowUpId} for InspectionId {InspectionId}, due {DueDate}",
                model.Id, model.InspectionId, model.DueDate.ToString("yyyy-MM-dd"));
            return RedirectToAction("Details", "Inspection", new { id = model.InspectionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating follow-up for InspectionId {InspectionId}", model.InspectionId);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            ViewBag.Inspection = inspection;
            return View(model);
        }
    }

    // GET: /FollowUp/Edit/5
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id)
    {
        var followUp = await _db.FollowUps
            .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (followUp is null) return NotFound();
        return View(followUp);
    }

    // POST: /FollowUp/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, FollowUp model)
    {
        if (id != model.Id) return BadRequest();

        // Business rule: closing requires a ClosedDate
        if (model.Status == FollowUpStatus.Closed && model.ClosedDate is null)
        {
            _logger.LogWarning("FollowUp close rejected: no ClosedDate provided for FollowUpId {FollowUpId}", id);
            ModelState.AddModelError("ClosedDate", "A closed date is required when marking as closed.");
        }

        // Business rule: open follow-ups should not have a closed date
        if (model.Status == FollowUpStatus.Open && model.ClosedDate is not null)
        {
            model.ClosedDate = null;
        }

        if (!ModelState.IsValid)
        {
            var followUp = await _db.FollowUps
                .Include(f => f.Inspection).ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(f => f.Id == id);
            return View(followUp);
        }

        try
        {
            _db.FollowUps.Update(model);
            await _db.SaveChangesAsync();
            _logger.LogInformation("FollowUp updated: {FollowUpId} - Status: {Status}",
                model.Id, model.Status);
            return RedirectToAction("Details", "Inspection", new { id = model.InspectionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating follow-up {FollowUpId}", id);
            ModelState.AddModelError("", "Unable to save. Please try again.");
            return View(model);
        }
    }

    // GET: /FollowUp/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var followUp = await _db.FollowUps
            .Include(f => f.Inspection).ThenInclude(i => i.Premises)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (followUp is null) return NotFound();
        return View(followUp);
    }

    // POST: /FollowUp/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var followUp = await _db.FollowUps.FindAsync(id);
            if (followUp is null) return NotFound();
            var inspectionId = followUp.InspectionId;
            _db.FollowUps.Remove(followUp);
            await _db.SaveChangesAsync();
            _logger.LogInformation("FollowUp deleted: {FollowUpId}", id);
            return RedirectToAction("Details", "Inspection", new { id = inspectionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting follow-up {FollowUpId}", id);
            return RedirectToAction("Index", "Inspection");
        }
    }
}
