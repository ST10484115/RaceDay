
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models.Events;

public class CategoryViewModel
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an age group.")]
    [StringLength(50)]
    public string AgeGroup { get; set; } = "All Ages";

    [Range(0.01, 9999.99, ErrorMessage = "Enter a valid category distance.")]
    public decimal DistanceKm { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "Entry fee cannot be negative.")]
    public decimal EntryFee { get; set; }
}
