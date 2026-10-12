
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(
            RaceDayDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>Registers an account with the Organiser or Participant role.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(
            RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new { message = "An account with this email already exists." });
            }

            var role = request.Role.Trim();

            if (role != "Organiser" && role != "Participant")
            {
                return BadRequest(new { message = "Role must be Organiser or Participant." });
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new AuthResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Message = "Registration successful. Please log in."
            });
        }

        /// <summary>Authenticates a user and creates a secure login cookie.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(
            LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // Keep the user ID and role in the server-managed session too.
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new AuthResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Message = "Login successful."
            });
        }

        /// <summary>Returns the currently authenticated user's account details.</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> Me()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new AuthResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Message = "Authenticated user."
            });
        }

        /// <summary>Signs out the current user and clears the session.</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return Ok(new { message = "Logout successful." });
        }
    }
}
