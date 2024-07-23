using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GebruikerController : Controller
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly StartspelerContext _context;
        public GebruikerController(UserManager<Gebruiker> userManager, StartspelerContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // GET: api/Gebruiker
        [HttpGet]
        public IActionResult GetAllGebruikers()
        {
            var gebruiker = _context.Gebruikers.ToList();
            return Ok(gebruiker);
        }
        // GET: api/Gebruiker/{id}
        [HttpGet("{id}")]
        public IActionResult GetGebruikerById(string id)
        {
            var gebruiker = _context.Gebruikers.FirstOrDefault(g => g.Id == id);
            if (gebruiker == null)
            {
                return NotFound();
            }
            return Ok(gebruiker);
        }
        // POST: api/Gebruiker
        [HttpPost]
        public async Task<IActionResult> CreateGebruiker([FromBody] Gebruiker gebruiker)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Gebruikers.Add(gebruiker);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetGebruikerById", new { id = gebruiker.Id }, gebruiker);
        }
        // PUT: api/Gebruiker/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGebruiker(string id, [FromBody] Gebruiker gebruiker)
        {
            if (id != gebruiker.Id)
            {
                return BadRequest();
            }
            _context.Entry(gebruiker).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Gebruikers.Any(g => g.Id == id))
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
        // DELETE: api/Gebruiker/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGebruiker(string id)
        {
            var geruiker = await _context.Gebruikers.FindAsync(id);
            if (geruiker == null)
            {
                return NotFound();
            }
            _context.Gebruikers.Remove(geruiker);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
