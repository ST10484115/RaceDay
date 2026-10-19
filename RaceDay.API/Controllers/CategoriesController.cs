
using Microsoft.AspNetCore.Authorization;
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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories/event/5
        // Public: view categories belonging to an event.
        [HttpGet("event/{eventId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.CategoryName,
                    c.DistanceKm,
                    c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.CategoryName,
                    c.DistanceKm,
                    c.EntryFee
                })
                .SingleOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            return Ok(category);
        }

        // POST: api/Categories/event/5
        // Only the event's Organiser can create its categories.
        [HttpPost("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            [FromBody] CreateCategoryRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .SingleOrDefaultAsync(e =>
                    e.EventId == eventId &&
                    e.OrganiserId == organiserId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found or you do not own this event."
                });
            }

            if (request.DistanceKm > raceEvent.DistanceKm)
            {
                return BadRequest(new
                {
                    message = "Category distance cannot exceed the event distance."
                });
            }

            var category = new Category
            {
                EventId = eventId,
                CategoryName = request.CategoryName.Trim(),
                DistanceKm = request.DistanceKm,
                EntryFee = request.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                new
                {
                    category.CategoryId,
                    category.EventId,
                    category.CategoryName,
                    category.DistanceKm,
                    category.EntryFee,
                    message = "Category created successfully."
                });
        }

        // PUT: api/Categories/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] CreateCategoryRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null ||
                category.Event == null ||
                category.Event.OrganiserId != organiserId)
            {
                return NotFound(new
                {
                    message = "Category not found or you do not own its event."
                });
            }

            if (request.DistanceKm > category.Event.DistanceKm)
            {
                return BadRequest(new
                {
                    message = "Category distance cannot exceed the event distance."
                });
            }

            category.CategoryName = request.CategoryName.Trim();
            category.DistanceKm = request.DistanceKm;
            category.EntryFee = request.EntryFee;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.CategoryId,
                category.EventId,
                category.CategoryName,
                category.DistanceKm,
                category.EntryFee,
                message = "Category updated successfully."
            });
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .SingleOrDefaultAsync(c => c.CategoryId == id);

            if (category == null ||
                category.Event == null ||
                category.Event.OrganiserId != organiserId)
            {
                return NotFound(new
                {
                    message = "Category not found or you do not own its event."
                });
            }

            var hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.CategoryId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "Categories with enrolments cannot be deleted."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}
