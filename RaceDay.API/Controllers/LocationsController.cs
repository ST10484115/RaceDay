
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public LocationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        // Public: anyone can view event locations.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _context.EventLocations
                .AsNoTracking()
                .Select(l => new
                {
                    l.LocationId,
                    l.LocationName,
                    l.Address,
                    l.City,
                    l.Province,
                    l.Latitude,
                    l.Longitude
                })
                .ToListAsync();

            return Ok(locations);
        }

        // GET: api/Locations/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLocation(int id)
        {
            var location = await _context.EventLocations
                .AsNoTracking()
                .Where(l => l.LocationId == id)
                .Select(l => new
                {
                    l.LocationId,
                    l.LocationName,
                    l.Address,
                    l.City,
                    l.Province,
                    l.Latitude,
                    l.Longitude
                })
                .SingleOrDefaultAsync();

            if (location == null)
            {
                return NotFound(new { message = "Location not found." });
            }

            return Ok(location);
        }

        // POST: api/Locations
        // Only Organisers can create locations.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateLocation(
            [FromBody] CreateLocationRequest request)
        {
            if (request.Latitude.HasValue &&
                (request.Latitude.Value < -90 ||
                 request.Latitude.Value > 90))
            {
                return BadRequest(new { message = "Latitude must be between -90 and 90." });
            }

            if (request.Longitude.HasValue &&
                (request.Longitude.Value < -180 ||
                 request.Longitude.Value > 180))
            {
                return BadRequest(new { message = "Longitude must be between -180 and 180." });
            }

            var location = new EventLocation
            {
                LocationName = request.LocationName.Trim(),
                Address = request.Address.Trim(),
                City = request.City.Trim(),
                Province = request.Province.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            _context.EventLocations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLocation),
                new { id = location.LocationId },
                new
                {
                    location.LocationId,
                    location.LocationName,
                    location.Address,
                    location.City,
                    location.Province,
                    location.Latitude,
                    location.Longitude,
                    message = "Location created successfully."
                });
        }

        // PUT: api/Locations/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateLocation(
            int id,
            [FromBody] CreateLocationRequest request)
        {
            var location = await _context.EventLocations
                .SingleOrDefaultAsync(l => l.LocationId == id);

            if (location == null)
            {
                return NotFound(new { message = "Location not found." });
            }

            if (request.Latitude.HasValue &&
                (request.Latitude.Value < -90 ||
                 request.Latitude.Value > 90))
            {
                return BadRequest(new { message = "Latitude must be between -90 and 90." });
            }

            if (request.Longitude.HasValue &&
                (request.Longitude.Value < -180 ||
                 request.Longitude.Value > 180))
            {
                return BadRequest(new { message = "Longitude must be between -180 and 180." });
            }

            location.LocationName = request.LocationName.Trim();
            location.Address = request.Address.Trim();
            location.City = request.City.Trim();
            location.Province = request.Province.Trim();
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                location.LocationId,
                location.LocationName,
                location.Address,
                location.City,
                location.Province,
                location.Latitude,
                location.Longitude,
                message = "Location updated successfully."
            });
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.EventLocations
                .SingleOrDefaultAsync(l => l.LocationId == id);

            if (location == null)
            {
                return NotFound(new { message = "Location not found." });
            }

            var hasEvents = await _context.Events
                .AnyAsync(e => e.LocationId == id);

            if (hasEvents)
            {
                return Conflict(new
                {
                    message = "Locations used by events cannot be deleted."
                });
            }

            _context.EventLocations.Remove(location);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Location deleted successfully." });
        }
    }
}
