using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Roles
        string[] roles = ["Admin", "Inspector", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Default users
        await CreateUser(userManager, "admin@food.ie", "Admin123!", "Admin");
        await CreateUser(userManager, "inspector@food.ie", "Inspect123!", "Inspector");
        await CreateUser(userManager, "viewer@food.ie", "Viewer123!", "Viewer");

        // Premises (12 across 3 towns)
        if (await context.Premises.AnyAsync()) return; // already seeded

        var premises = new List<Premises>
        {
            // Dublin
            new() { Name = "The Golden Fork",    Address = "12 Main St",    Town = "Dublin", RiskRating = RiskRating.High },
            new() { Name = "Café Bloom",          Address = "3 Parnell Sq",  Town = "Dublin", RiskRating = RiskRating.Low },
            new() { Name = "Bites & Co",          Address = "88 Dame St",    Town = "Dublin", RiskRating = RiskRating.Medium },
            new() { Name = "The Sandwich Press",  Address = "44 Talbot St",  Town = "Dublin", RiskRating = RiskRating.Low },
            // Cork
            new() { Name = "Rebel Kitchen",       Address = "7 Patrick St",  Town = "Cork",   RiskRating = RiskRating.High },
            new() { Name = "The Marina Bistro",   Address = "2 Dock Rd",     Town = "Cork",   RiskRating = RiskRating.Medium },
            new() { Name = "Cork Spice House",    Address = "19 Oliver Plunkett St", Town = "Cork", RiskRating = RiskRating.Medium },
            new() { Name = "Leeside Diner",       Address = "5 Union Quay",  Town = "Cork",   RiskRating = RiskRating.Low },
            // Galway
            new() { Name = "The Claddagh Table",  Address = "1 Quay St",     Town = "Galway", RiskRating = RiskRating.High },
            new() { Name = "West Coast Wraps",    Address = "33 Shop St",    Town = "Galway", RiskRating = RiskRating.Low },
            new() { Name = "Salthill Suppers",    Address = "9 Salthill Rd", Town = "Galway", RiskRating = RiskRating.Medium },
            new() { Name = "The Eyre Eatery",     Address = "6 Eyre Sq",     Town = "Galway", RiskRating = RiskRating.High },
        };

        context.Premises.AddRange(premises);
        await context.SaveChangesAsync();

        // Inspections (25 across different dates)
        var now = DateTime.Today;
        var inspections = new List<Inspection>
        {
            new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-5),   Score = 45, Outcome = InspectionOutcome.Fail, Notes = "Poor refrigeration standards." },
            new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-60),  Score = 52, Outcome = InspectionOutcome.Fail, Notes = "Repeat hygiene issues." },
            new() { PremisesId = premises[1].Id,  InspectionDate = now.AddDays(-3),   Score = 91, Outcome = InspectionOutcome.Pass, Notes = "Excellent standards." },
            new() { PremisesId = premises[2].Id,  InspectionDate = now.AddDays(-10),  Score = 78, Outcome = InspectionOutcome.Pass, Notes = "Minor labelling issue noted." },
            new() { PremisesId = premises[2].Id,  InspectionDate = now.AddDays(-8),   Score = 60, Outcome = InspectionOutcome.Fail, Notes = "Temperature log missing." },
            new() { PremisesId = premises[3].Id,  InspectionDate = now.AddDays(-2),   Score = 88, Outcome = InspectionOutcome.Pass, Notes = "Well managed." },
            new() { PremisesId = premises[4].Id,  InspectionDate = now.AddDays(-15),  Score = 40, Outcome = InspectionOutcome.Fail, Notes = "Serious cross-contamination risk." },
            new() { PremisesId = premises[4].Id,  InspectionDate = now.AddDays(-90),  Score = 35, Outcome = InspectionOutcome.Fail, Notes = "Pest evidence found." },
            new() { PremisesId = premises[5].Id,  InspectionDate = now.AddDays(-20),  Score = 74, Outcome = InspectionOutcome.Pass, Notes = "Generally satisfactory." },
            new() { PremisesId = premises[6].Id,  InspectionDate = now.AddDays(-7),   Score = 69, Outcome = InspectionOutcome.Fail, Notes = "Hand washing facilities inadequate." },
            new() { PremisesId = premises[6].Id,  InspectionDate = now.AddDays(-4),   Score = 82, Outcome = InspectionOutcome.Pass, Notes = "Improvement noted." },
            new() { PremisesId = premises[7].Id,  InspectionDate = now.AddDays(-1),   Score = 95, Outcome = InspectionOutcome.Pass, Notes = "Top marks." },
            new() { PremisesId = premises[8].Id,  InspectionDate = now.AddDays(-12),  Score = 50, Outcome = InspectionOutcome.Fail, Notes = "Storage area disorganised." },
            new() { PremisesId = premises[8].Id,  InspectionDate = now.AddDays(-6),   Score = 55, Outcome = InspectionOutcome.Fail, Notes = "No improvement since last visit." },
            new() { PremisesId = premises[9].Id,  InspectionDate = now.AddDays(-9),   Score = 85, Outcome = InspectionOutcome.Pass, Notes = "Good overall." },
            new() { PremisesId = premises[10].Id, InspectionDate = now.AddDays(-14),  Score = 73, Outcome = InspectionOutcome.Pass, Notes = "Satisfactory." },
            new() { PremisesId = premises[10].Id, InspectionDate = now.AddDays(-3),   Score = 66, Outcome = InspectionOutcome.Fail, Notes = "Cleaning schedule not followed." },
            new() { PremisesId = premises[11].Id, InspectionDate = now.AddDays(-11),  Score = 42, Outcome = InspectionOutcome.Fail, Notes = "Multiple critical failures." },
            new() { PremisesId = premises[11].Id, InspectionDate = now.AddDays(-55),  Score = 38, Outcome = InspectionOutcome.Fail, Notes = "Previous closure order issued." },
            new() { PremisesId = premises[1].Id,  InspectionDate = now.AddDays(-45),  Score = 89, Outcome = InspectionOutcome.Pass, Notes = "Consistent high performer." },
            new() { PremisesId = premises[3].Id,  InspectionDate = now.AddDays(-30),  Score = 77, Outcome = InspectionOutcome.Pass, Notes = "Good documentation." },
            new() { PremisesId = premises[5].Id,  InspectionDate = now.AddDays(-50),  Score = 71, Outcome = InspectionOutcome.Pass, Notes = "Minor issues resolved." },
            new() { PremisesId = premises[7].Id,  InspectionDate = now.AddDays(-25),  Score = 93, Outcome = InspectionOutcome.Pass, Notes = "Outstanding." },
            new() { PremisesId = premises[9].Id,  InspectionDate = now.AddDays(-40),  Score = 80, Outcome = InspectionOutcome.Pass, Notes = "Well run operation." },
            new() { PremisesId = premises[0].Id,  InspectionDate = now.AddDays(-18),  Score = 48, Outcome = InspectionOutcome.Fail, Notes = "Ongoing non-compliance." },
        };

        context.Inspections.AddRange(inspections);
        await context.SaveChangesAsync();

        // Follow-ups (10: some overdue, some closed)
        var followUps = new List<FollowUp>
        {
            // Overdue + open
            new() { InspectionId = inspections[0].Id,  DueDate = now.AddDays(-3),  Status = FollowUpStatus.Open },
            new() { InspectionId = inspections[4].Id,  DueDate = now.AddDays(-10), Status = FollowUpStatus.Open },
            new() { InspectionId = inspections[6].Id,  DueDate = now.AddDays(-5),  Status = FollowUpStatus.Open },
            new() { InspectionId = inspections[12].Id, DueDate = now.AddDays(-7),  Status = FollowUpStatus.Open },
            new() { InspectionId = inspections[17].Id, DueDate = now.AddDays(-2),  Status = FollowUpStatus.Open },
            // Closed
            new() { InspectionId = inspections[1].Id,  DueDate = now.AddDays(-40), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-35) },
            new() { InspectionId = inspections[7].Id,  DueDate = now.AddDays(-60), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-55) },
            new() { InspectionId = inspections[9].Id,  DueDate = now.AddDays(-3),  Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-1) },
            // Future (open, not overdue)
            new() { InspectionId = inspections[13].Id, DueDate = now.AddDays(7),   Status = FollowUpStatus.Open },
            new() { InspectionId = inspections[24].Id, DueDate = now.AddDays(14),  Status = FollowUpStatus.Open },
        };

        context.FollowUps.AddRange(followUps);
        await context.SaveChangesAsync();
    }

    private static async Task CreateUser(
        UserManager<IdentityUser> userManager,
        string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}
