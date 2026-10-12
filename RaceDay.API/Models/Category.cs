
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(6,2)")]
        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal EntryFee { get; set; }

        // Navigation property linking the category to its event.
        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }
    }
}
