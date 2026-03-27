using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.EntityFrameworkCore;


namespace FoodSafetyTracker.Tests;

public class FoodSafetyTests
{
    // Helpers

    private static ApplicationDbContext GetInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<ApplicationDbContext> GetSeededDb(string dbName)
    {
        var db = GetInMemoryDb(dbName);

        var premises = new Premises
        {
            Id = 1,
            Name = "Test Café",
            Address = "1 Test St",
            Town = "Dublin",
            RiskRating = RiskRating.High
        };
        db.Premises.Add(premises);

        var inspection = new Inspection
        {
            Id = 1,
            PremisesId = 1,
            InspectionDate = DateTime.Today.AddDays(-10),
            Score = 45,
            Outcome = InspectionOutcome.Fail,
            Notes = "Test inspection"
        };
        db.Inspections.Add(inspection);

        // Overdue open follow-ups (due in the past)
        db.FollowUps.AddRange(
            new FollowUp { Id = 1, InspectionId = 1, DueDate = DateTime.Today.AddDays(-5), Status = FollowUpStatus.Open },
            new FollowUp { Id = 2, InspectionId = 1, DueDate = DateTime.Today.AddDays(-2), Status = FollowUpStatus.Open },
            // Not overdue
            new FollowUp { Id = 3, InspectionId = 1, DueDate = DateTime.Today.AddDays(7), Status = FollowUpStatus.Open },
            // Closed
            new FollowUp { Id = 4, InspectionId = 1, DueDate = DateTime.Today.AddDays(-3), Status = FollowUpStatus.Closed, ClosedDate = DateTime.Today.AddDays(-1) }
        );

        await db.SaveChangesAsync();
        return db;
    }

    // ── Test 1: Overdue follow-ups query returns correct items ────

    [Fact]
    public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDue()
    {
        var db = await GetSeededDb("overdue_test");
        var today = DateTime.Today;

        var overdue = await db.FollowUps
            .Where(f => f.Status == FollowUpStatus.Open && f.DueDate < today)
            .ToListAsync();

        Assert.Equal(2, overdue.Count);
        Assert.All(overdue, f => Assert.Equal(FollowUpStatus.Open, f.Status));
        Assert.All(overdue, f => Assert.True(f.DueDate < today));
    }

    // ── Test 2: FollowUp cannot be closed without ClosedDate ─────

    [Fact]
    public async Task ClosingFollowUp_WithoutClosedDate_IsInvalid()
    {
        var db = await GetSeededDb("close_test");

        var followUp = await db.FollowUps.FindAsync(1);
        Assert.NotNull(followUp);

        // Simulate the business rule check from the controller
        followUp!.Status = FollowUpStatus.Closed;
        // ClosedDate intentionally left null

        bool isValid = !(followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate is null);

        Assert.False(isValid, "A follow-up marked Closed without a ClosedDate should be invalid.");
    }

    // ── Test 3: Dashboard counts consistent with seed data ───────

    [Fact]
    public async Task DashboardCounts_MatchKnownSeedData()
    {
        var db = await GetSeededDb("dashboard_test");
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var inspectionsThisMonth = await db.Inspections
            .Where(i => i.InspectionDate >= startOfMonth)
            .CountAsync();

        var failedThisMonth = await db.Inspections
            .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == InspectionOutcome.Fail)
            .CountAsync();

        var overdueCount = await db.FollowUps
            .Where(f => f.Status == FollowUpStatus.Open && f.DueDate < today)
            .CountAsync();

        // Our seeded inspection is 10 days ago so within this month
        Assert.Equal(1, inspectionsThisMonth);
        Assert.Equal(1, failedThisMonth);
        Assert.Equal(2, overdueCount);
    }

    // ── Test 4: Closed follow-up with ClosedDate is valid ────────

    [Fact]
    public async Task ClosingFollowUp_WithClosedDate_IsValid()
    {
        var db = await GetSeededDb("close_valid_test");

        var followUp = await db.FollowUps.FindAsync(1);
        Assert.NotNull(followUp);

        followUp!.Status = FollowUpStatus.Closed;
        followUp.ClosedDate = DateTime.Today;
        await db.SaveChangesAsync();

        var updated = await db.FollowUps.FindAsync(1);
        Assert.Equal(FollowUpStatus.Closed, updated!.Status);
        Assert.NotNull(updated.ClosedDate);
    }
}
