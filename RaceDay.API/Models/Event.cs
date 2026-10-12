
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(150)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Upcoming";

        // Required by the Part 2 brief.
        [Column(TypeName = "decimal(6,2)")]
        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        // Allowed values will be validated by the API: Run, Walk, Cycle.
        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = "Run";

        // Related records.
        [ForeignKey(nameof(OrganiserId))]
        public User? Organiser { get; set; }

        [ForeignKey(nameof(LocationId))]
        public EventLocation? Location { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
