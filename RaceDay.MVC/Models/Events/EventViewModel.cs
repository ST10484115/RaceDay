
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models.Events;

public class EventViewModel
{
    public int EventId { get; set; }

    public int OrganiserId { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(150)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an event date.")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Please select a location.")]
    public int LocationId { get; set; }

    [Range(0.01, 9999.99)]
    public decimal DistanceKm { get; set; }

    [Required]
    public string EventType { get; set; } = "Run";

    [Required]
    public string Status { get; set; } = "Upcoming";

    public string LocationName { get; set; } = string.Empty;
}
