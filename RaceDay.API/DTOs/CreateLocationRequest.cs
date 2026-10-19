
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateLocationRequest
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Province { get; set; } = string.Empty;

        [Range(typeof(decimal), "-90", "90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180")]
        public decimal? Longitude { get; set; }
    }
}
