using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Diagnostics;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly StartspelerContext _context;

        public EventController(UserManager<Gebruiker> userManager, StartspelerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/Event
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            var events = _context.Evenementen.ToList();
            return Ok(events);
        }

        // GET: api/Event/{id}
        [HttpGet("{id}")]
        public IActionResult GetEventById(int id)
        {
            var eventItem = _context.Evenementen.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return Ok(eventItem);
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Evenement eventItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Evenementen.Add(eventItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetEventById", new { id = eventItem.Id }, eventItem);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Evenement eventItem)
        {
            if (id != eventItem.Id)
            {
                return BadRequest();
            }
            _context.Entry(eventItem).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Evenementen.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Event/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Evenementen.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            _context.Evenementen.Remove(eventItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Patch: api/Event/registerToEvent
        [HttpPatch("registerToEvent")]
        public async Task<IActionResult> RegisterUserToEvent([FromBody] EventRegistrationDTO registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var evenement = await _context.Evenementen.FindAsync(registration.Evenement);
            var gebruiker = await _context.Gebruikers.FindAsync(registration.GebruikerId);

            if (evenement == null || gebruiker == null)
            {
                return NotFound("Evenement or Gebruiker not found.");
            }

            if (evenement.AantalDeelnemers >= evenement.MaxDeelnemers)
            {
                return BadRequest("Event is full.");
            }

            // Create a new registration
            var newRegistration = new EvenementenRegistratie
            {
                EvenementId = registration.Evenement,
                GebruikerId = registration.GebruikerId,
                AantalPersonen = registration.AantalPersonen
            };

            // Add the new registration to the database
            _context.EvenementenRegistraties.Add(newRegistration);

            // Update the number of participants in the event
            evenement.AantalDeelnemers += 1;
            await _context.SaveChangesAsync();

            return Ok("User registered to event successfully.");
        }

        // Patch: api/Event/unregisterFromEvent
        [HttpPatch("unregisterFromEvent/{userId}/{eventId}")]
        public async Task<IActionResult> UnregisterUserFromEvent(string userId, int eventId)
        {
            if (string.IsNullOrEmpty(userId) || eventId == 0)
            {
                return BadRequest("Invalid userId or eventId");
            }

            // Log de ontvangen gegevens
            Debug.WriteLine($"UnregisterUserFromEvent called with Evenement: {eventId}, GebruikerId: {userId}");

            // Find the existing registration
            var registrationToDelete = await _context.EvenementenRegistraties
                .FirstOrDefaultAsync(r => r.EvenementId == eventId && r.GebruikerId == userId);

            if (registrationToDelete == null)
            {
                return NotFound("Registration not found.");
            }

            // Remove the registration from the database
            _context.EvenementenRegistraties.Remove(registrationToDelete);

            // Update the number of participants in the event
            var evenement = await _context.Evenementen.FindAsync(eventId);
            if (evenement != null)
            {
                evenement.AantalDeelnemers -= registrationToDelete.AantalPersonen;
                await _context.SaveChangesAsync();
                return Ok("User unregistered from event successfully.");
            }
            else
            {
                return NotFound("Event not found.");
            }
        }

        // GET: api/Event/isUserRegistered?eventId=1&userId=123
        [HttpGet("isUserRegistered")]
        public async Task<IActionResult> IsUserRegistered(int eventId, string userId)
        {
            var isRegistered = await _context.EvenementenRegistraties
                .AnyAsync(r => r.EvenementId == eventId && r.GebruikerId == userId);

            return Ok(isRegistered);
        }

        public class EventRegistrationDTO
        {
            public int Evenement { get; set; }
            public string GebruikerId { get; set; } = default!;
            public int AantalPersonen { get; set; }
        }

        public class EvenementDto
        {
            public int Id { get; set; }
            public string Naam { get; set; }
            // Andere relevante velden, maar geen navigatie-eigenschappen zoals Community
        }

        [HttpGet("byCommunity/{communityId}")]
        public async Task<IActionResult> GetEventsByCommunity(int communityId)
        {
            Debug.WriteLine($"Fetching events for community ID: {communityId}");
            if (!_context.Communities.Any(c => c.Id == communityId))
            {
                Debug.WriteLine($"No community found with ID {communityId}");
                return NotFound($"No community found with ID {communityId}");
            }

            try
            {
                var events = await _context.Evenementen
                    .Include(e => e.Community)
                    .Where(e => e.CommunityId == communityId)
                    .ToListAsync();
                Debug.WriteLine($"Events found: {events.Count}");
                return Ok(events);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error retrieving events: " + ex.ToString());
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}