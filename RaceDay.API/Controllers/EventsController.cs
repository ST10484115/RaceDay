
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Public: anyone can view upcoming events.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.LocationId,
                    e.DistanceKm,
                    e.EventType,
                    e.Status,
                    e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.LocationId,
                    e.DistanceKm,
                    e.EventType,
                    e.Status,
                    e.OrganiserId
                })
                .SingleOrDefaultAsync();

            if (raceEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(raceEvent);
        }


        // POST: api/Events
        // Only an authenticated Organiser can create an event.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(
            [FromBody] RaceDay.API.DTOs.CreateEventRequest request)
        {
            var organiserIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            // Verify that the selected location exists.
            var locationExists = await _context.EventLocations
                .AnyAsync(l => l.LocationId == request.LocationId);

            if (!locationExists)
            {
                return BadRequest(new
                {
                    message = "The selected event location does not exist."
                });
            }

            // Validate the event type and status on the server.
            if (!new[] { "Run", "Walk", "Cycle" }
                .Contains(request.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            if (!new[] { "Upcoming", "Active", "Completed", "Cancelled" }
                .Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid event status."
                });
            }

            if (request.EventDate <= DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message = "The event date must be in the future."
                });
            }

            var raceEvent = new RaceDay.API.Models.Event
            {
                OrganiserId = organiserId,
                EventName = request.EventName.Trim(),
                Description = request.Description.Trim(),
                EventDate = request.EventDate,
                LocationId = request.LocationId,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                Status = request.Status
            };

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventId },
                new
                {
                    raceEvent.EventId,
                    raceEvent.EventName,
                    raceEvent.Description,
                    raceEvent.EventDate,
                    raceEvent.LocationId,
                    raceEvent.DistanceKm,
                    raceEvent.EventType,
                    raceEvent.Status,
                    raceEvent.OrganiserId,
                    message = "Event created successfully."
                });
        }


        // PUT: api/Events/5
        // Organisers can update only their own events.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] RaceDay.API.DTOs.CreateEventRequest request)
        {
            var organiserIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == id && e.OrganiserId == organiserId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found or you do not own this event."
                });
            }

            var locationExists = await _context.EventLocations
                .AnyAsync(l => l.LocationId == request.LocationId);

            if (!locationExists)
            {
                return BadRequest(new
                {
                    message = "The selected event location does not exist."
                });
            }

            if (!new[] { "Run", "Walk", "Cycle" }
                .Contains(request.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            if (!new[] { "Upcoming", "Active", "Completed", "Cancelled" }
                .Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid event status."
                });
            }

            if (request.EventDate <= DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message = "The event date must be in the future."
                });
            }

            raceEvent.EventName = request.EventName.Trim();
            raceEvent.Description = request.Description.Trim();
            raceEvent.EventDate = request.EventDate;
            raceEvent.LocationId = request.LocationId;
            raceEvent.DistanceKm = request.DistanceKm;
            raceEvent.EventType = request.EventType;
            raceEvent.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                raceEvent.EventId,
                raceEvent.EventName,
                raceEvent.EventDate,
                raceEvent.LocationId,
                raceEvent.DistanceKm,
                raceEvent.EventType,
                raceEvent.Status,
                message = "Event updated successfully."
            });
        }

        // DELETE: api/Events/5
        // Organisers can delete only their own events.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == id && e.OrganiserId == organiserId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found or you do not own this event."
                });
            }

            var hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.EventId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "Events with enrolments cannot be deleted."
                });
            }

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }



        // GET: api/Events/mine
        // Organisers can view only events they own.
        [HttpGet("mine")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var organiserId))
            {
                return Unauthorized();
            }

            var events = await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganiserId == organiserId)
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.LocationId,
                    e.DistanceKm,
                    e.EventType,
                    e.Status
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}
