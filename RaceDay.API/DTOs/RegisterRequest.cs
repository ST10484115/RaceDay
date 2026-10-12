
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Organiser|Participant)$",
            ErrorMessage = "Role must be Organiser or Participant.")]
        public string Role { get; set; } = "Participant";
    }
}
