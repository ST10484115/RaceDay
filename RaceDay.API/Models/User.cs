
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        // Stores the hashed password, never the original password.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Only Organiser or Participant will be accepted by the API.
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Participant";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for related records.
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
