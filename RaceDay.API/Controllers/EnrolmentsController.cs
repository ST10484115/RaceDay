
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
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Participants see their own enrolments.
        // Organisers see enrolments for their own events.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Unauthorized();
            }

            var query = _context.Enrolments
                .AsNoTracking()
                .AsQueryable();

            if (User.IsInRole("Participant"))
            {
                query = query.Where(e => e.ParticipantId == userId);
            }
            else if (User.IsInRole("Organiser"))
            {
                query = query.Where(e => e.Event!.OrganiserId == userId);
            }
            else
            {
                return Forbid();
            }

            var enrolments = await query
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    EventName = e.Event!.EventName,
                    e.CategoryId,
                    CategoryName = e.Category!.CategoryName,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/5
        // Users may view only enrolments they are allowed to access.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == id)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    EventName = e.Event!.EventName,
                    OrganiserId = e.Event!.OrganiserId,
                    e.CategoryId,
                    CategoryName = e.Category!.CategoryName,
                    e.EnrolmentDate,
                    e.Status
                })
                .SingleOrDefaultAsync();

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (User.IsInRole("Participant") &&
                enrolment.ParticipantId != userId)
            {
                return Forbid();
            }

            if (User.IsInRole("Organiser") &&
                enrolment.OrganiserId != userId)
            {
                return Forbid();
            }

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        // Participants can enrol only themselves.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var participantId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .SingleOrDefaultAsync(e => e.EventId == request.EventId);

            if (raceEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (raceEvent.Status != "Upcoming" ||
                raceEvent.EventDate <= DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message = "You can only enrol in upcoming events."
                });
            }

            var category = await _context.Categories
                .SingleOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.EventId == request.EventId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "The category does not belong to the selected event."
                });
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.CategoryId == request.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new
                {
                    enrolment.EnrolmentId,
                    enrolment.ParticipantId,
                    enrolment.EventId,
                    enrolment.CategoryId,
                    enrolment.EnrolmentDate,
                    enrolment.Status,
                    message = "Enrolment created successfully."
                });
        }

        // DELETE: api/Enrolments/5
        // Participants can cancel their own enrolments.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var participantId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .SingleOrDefaultAsync(e =>
                    e.EnrolmentId == id &&
                    e.ParticipantId == participantId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found or you do not own it."
                });
            }

            var hasResult = await _context.Results
                .AnyAsync(r => r.EnrolmentId == id);

            if (hasResult)
            {
                return Conflict(new
                {
                    message = "An enrolment with a recorded result cannot be cancelled."
                });
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled successfully." });
        }
    }
}
