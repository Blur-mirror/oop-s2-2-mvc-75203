using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext db, ILogger<DashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? town, RiskRating? riskRating)
    {
        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Base inspections query — apply filters if provided
        var inspectionsQuery = _db.Inspections
            .Include(i => i.Premises)
            .AsQueryable();

        if (!string.IsNullOrEmpty(town))
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);

        if (riskRating.HasValue)
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating.Value);

        // Aggregates
        var inspectionsThisMonth = await inspectionsQuery
            .Where(i => i.InspectionDate >= startOfMonth)
            .CountAsync();

        var failedThisMonth = await inspectionsQuery
            .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == InspectionOutcome.Fail)
            .CountAsync();

        // Overdue follow-ups — filtered through inspection -> premises chain
        var overdueQuery = _db.FollowUps
            .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
            .Where(f => f.Status == FollowUpStatus.Open && f.DueDate < now)
            .AsQueryable();

        if (!string.IsNullOrEmpty(town))
            overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town == town);

        if (riskRating.HasValue)
            overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == riskRating.Value);

        var overdueCount = await overdueQuery.CountAsync();
        var overdueList = await overdueQuery.OrderBy(f => f.DueDate).ToListAsync();

        // Recent failed inspections for display
        var recentFailed = await inspectionsQuery
            .Where(i => i.Outcome == InspectionOutcome.Fail)
            .OrderByDescending(i => i.InspectionDate)
            .Take(10)
            .ToListAsync();

        // Towns for filter dropdown
        var towns = await _db.Premises
            .Select(p => p.Town)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        _logger.LogInformation(
            "Dashboard viewed by {User} — filters: Town={Town}, Risk={Risk} — Results: {Monthly} inspections, {Failed} failed, {Overdue} overdue",
            User.Identity?.Name ?? "anonymous",
            town ?? "all",
            riskRating?.ToString() ?? "all",
            inspectionsThisMonth,
            failedThisMonth,
            overdueCount);

        var vm = new DashboardViewModel
        {
            InspectionsThisMonth = inspectionsThisMonth,
            FailedInspectionsThisMonth = failedThisMonth,
            OverdueOpenFollowUps = overdueCount,
            SelectedTown = town,
            SelectedRiskRating = riskRating,
            Towns = towns,
            RecentFailedInspections = recentFailed,
            OverdueFollowUpsList = overdueList
        };

        return View(vm);
    }
}
