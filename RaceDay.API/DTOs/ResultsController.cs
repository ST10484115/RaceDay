
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Participants see only their own results.
        // Organisers see results for their own events.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Unauthorized();
            }

            var query = _context.Results
                .AsNoTracking()
                .AsQueryable();

            if (User.IsInRole("Participant"))
            {
                query = query.Where(r =>
                    r.Enrolment!.ParticipantId == userId);
            }
            else if (User.IsInRole("Organiser"))
            {
                query = query.Where(r =>
                    r.Enrolment!.Event!.OrganiserId == userId);
            }
            else
            {
                return Forbid();
            }

            var results = await query
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    ParticipantId = r.Enrolment!.ParticipantId,
                    ParticipantName =
                        r.Enrolment.Participant!.FirstName + " " +
                        r.Enrolment.Participant.LastName,
                    EventId = r.Enrolment.EventId,
                    EventName = r.Enrolment.Event!.EventName,
                    r.FinishTime,
                    r.Position,
                    r.ResultStatus,
                    r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResult(int id)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Unauthorized();
            }

            var result = await _context.Results
                .AsNoTracking()
                .Where(r => r.ResultId == id)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    ParticipantId = r.Enrolment!.ParticipantId,
                    ParticipantName =
                        r.Enrolment.Participant!.FirstName + " " +
                        r.Enrolment.Participant.LastName,
                    EventId = r.Enrolment.EventId,
                    EventName = r.Enrolment.Event!.EventName,
                    OrganiserId = r.Enrolment.Event.OrganiserId,
                    r.FinishTime,
                    r.Position,
                    r.ResultStatus,
                    r.RecordedAt
                })
                .SingleOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new { message = "Result not found." });
            }

            if (User.IsInRole("Participant") &&
                result.ParticipantId != userId)
            {
                return Forbid();
            }

            if (User.IsInRole("Organiser") &&
                result.OrganiserId != userId)
            {
                return Forbid();
            }

            return Ok(result);
        }

        // POST: api/Results/5
        // The ID is the enrolment ID.
        // Only the Organiser who owns the event can record a result.
        [HttpPost("{enrolmentId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> RecordResult(
            int enrolmentId,
            [FromBody] RecordResultRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .SingleOrDefaultAsync(e =>
                    e.EnrolmentId == enrolmentId &&
                    e.Event!.OrganiserId == organiserId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found or you do not own its event."
                });
            }

            var existingResult = await _context.Results
                .AnyAsync(r => r.EnrolmentId == enrolmentId);

            if (existingResult)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            if (request.FinishTime.HasValue &&
                request.FinishTime.Value < TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time cannot be negative."
                });
            }

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                ResultStatus = request.ResultStatus,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultId },
                new
                {
                    result.ResultId,
                    result.EnrolmentId,
                    result.FinishTime,
                    result.Position,
                    result.ResultStatus,
                    result.RecordedAt,
                    message = "Result recorded successfully."
                });
        }

        // PUT: api/Results/5
        // Update a result only when the Organiser owns the event.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] RecordResultRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
                .SingleOrDefaultAsync(r =>
                    r.ResultId == id &&
                    r.Enrolment!.Event!.OrganiserId == organiserId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found or you do not own its event."
                });
            }

            if (request.FinishTime.HasValue &&
                request.FinishTime.Value < TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time cannot be negative."
                });
            }

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.ResultStatus = request.ResultStatus;
            result.RecordedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.ResultStatus,
                result.RecordedAt,
                message = "Result updated successfully."
            });
        }
    }
}
