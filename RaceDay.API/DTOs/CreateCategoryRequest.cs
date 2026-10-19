
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        [Range(0, 99999999.99)]
        public decimal EntryFee { get; set; }
    }
}
