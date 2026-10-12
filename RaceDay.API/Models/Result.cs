
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        // Participant's finishing time.
        [Column(TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        // Position in the event, e.g. 1st, 2nd, 3rd.
        [Range(1, int.MaxValue)]
        public int? Position { get; set; }

        [Required]
        [StringLength(30)]
        public string ResultStatus { get; set; } = "Pending";

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Each result belongs to one enrolment.
        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }
    }
}
