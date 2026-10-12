
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class EventLocation
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        [StringLength(150)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        // One location can host multiple events.
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
