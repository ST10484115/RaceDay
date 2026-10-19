
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class RecordResultRequest
    {
        [Required]
        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? Position { get; set; }

        [Required]
        [RegularExpression(
            "^(Pending|Recorded|Disqualified|DidNotFinish)$",
            ErrorMessage = "Invalid result status.")]
        public string ResultStatus { get; set; } = "Recorded";
    }
}
