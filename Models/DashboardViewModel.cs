using FoodSafetyTracker.Models;

namespace FoodSafetyTracker.Models;

public class DashboardViewModel
{
    // Aggregates
    public int InspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueOpenFollowUps { get; set; }

    // Filter inputs
    public string? SelectedTown { get; set; }
    public RiskRating? SelectedRiskRating { get; set; }

    // Filter options (populated from DB)
    public List<string> Towns { get; set; } = new();

    // Recent failed inspections for the table
    public List<Inspection> RecentFailedInspections { get; set; } = new();

    // Overdue follow-ups for the table
    public List<FollowUp> OverdueFollowUpsList { get; set; } = new();
}
