
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(
        RaceDayDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> GetMyProfile()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
            return NotFound();

        return Ok(new AuthResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Message = "Profile retrieved successfully."
        });
    }

    [HttpPut("me")]
    public async Task<ActionResult<AuthResponse>> UpdateMyProfile(
        [FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
            return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();

        var emailInUse = await _context.Users.AnyAsync(u =>
            u.Email == email && u.UserId != user.UserId);

        if (emailInUse)
            return Conflict(new { message = "That email address is already registered." });

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.NewPassword);
        }

        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Message = "Profile updated successfully."
        });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(claim, out var userId) ? userId : null;
    }
}

