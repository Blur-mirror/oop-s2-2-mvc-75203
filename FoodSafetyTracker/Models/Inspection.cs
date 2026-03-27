using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace FoodSafetyTracker.Models;

public enum InspectionOutcome { Pass, Fail }

public class Inspection
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Please select a premises.")]
    public int PremisesId { get; set; }
    public DateTime InspectionDate { get; set; }
    public int Score { get; set; } // 0-100
    public InspectionOutcome Outcome { get; set; }
    public string? Notes { get; set; }

    // Navigation
    [ValidateNever]  // add this using Microsoft.AspNetCore.Mvc.ModelBinding.Validation
    public Premises Premises { get; set; } = null!;
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
