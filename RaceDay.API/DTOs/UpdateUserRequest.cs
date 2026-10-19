
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class UpdateUserRequest
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

    [MinLength(8)]
    [MaxLength(100)]
    public string? NewPassword { get; set; }
}
