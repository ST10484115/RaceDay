
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateEventRequest
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        [Required]
        [RegularExpression(
            "^(Run|Walk|Cycle)$",
            ErrorMessage = "Event type must be Run, Walk, or Cycle.")]
        public string EventType { get; set; } = "Run";

        [RegularExpression(
            "^(Upcoming|Active|Completed|Cancelled)$",
            ErrorMessage = "Invalid event status.")]
        public string Status { get; set; } = "Upcoming";
    }
}
