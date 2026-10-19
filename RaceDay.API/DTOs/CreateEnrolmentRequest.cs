
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateEnrolmentRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
