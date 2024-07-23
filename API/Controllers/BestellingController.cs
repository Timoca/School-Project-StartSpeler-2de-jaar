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
    public class BestellingController : Controller
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly StartspelerContext _context;

        public BestellingController(UserManager<Gebruiker> userManager, StartspelerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/Bestelling
        [HttpGet]
        public IActionResult GetAllBestellingen()
        {
            var bestelling = _context.Bestellingen.ToList();
            return Ok(bestelling);
        }

        // GET: api/Bestelling/{id}
        [HttpGet("{id}")]
        public IActionResult GetBestellingById(int id)
        {
            var bestelling = _context.Bestellingen.FirstOrDefault(b => b.Id == id);
            if (bestelling == null)
            {
                return NotFound();
            }
            return Ok(bestelling);
        }

        // POST: api/Bestelling
        [HttpPost]
        public async Task<IActionResult> CreateBestelling([FromBody] Bestelling bestelling)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Model validation failed", Errors = errors });
            }
            _context.Bestellingen.Add(bestelling);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBestellingById", new { id = bestelling.Id }, bestelling);
        }

        // PATCH: api/Bestelling/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBestelling(int id, [FromBody] Bestelling bestelling)
        {
            if (id != bestelling.Id)
            {
                return BadRequest();
            }
            _context.Entry(bestelling).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Bestellingen.Any(b => b.Id == id))
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

        // DELETE: api/Bestelling/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBestelling(int id)
        {
            var bestelling = await _context.Bestellingen.FindAsync(id);
            if (bestelling == null)
            {
                return NotFound();
            }
            _context.Bestellingen.Remove(bestelling);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}