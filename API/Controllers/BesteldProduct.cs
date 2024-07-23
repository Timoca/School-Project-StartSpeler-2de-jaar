using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BesteldProductController : ControllerBase
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly StartspelerContext _context;


        public BesteldProductController(UserManager<Gebruiker> userManager, StartspelerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/BesteldProduct
        [HttpGet]
        public IActionResult GetAllBesteldeProducten()
        {
            
            var BesteldeProducten = _context.BesteldeProducten.Include(x => x.Product).ToList();
            return Ok(BesteldeProducten);
        }

        // GET: api/BesteldProduct/{id}
        [HttpGet("{id}")]
        public IActionResult GetBesteldProductById(int id)
        {
            var BesteldProduct = _context.BesteldeProducten.FirstOrDefault(p => p.Id == id);
            if (BesteldProduct == null)
            {
                return NotFound();
            }
            return Ok(BesteldProduct);
        }

        // POST: api/BesteldProduct
        [HttpPost]
        public async Task<IActionResult> CreateBesteldProduct([FromBody] BesteldProduct BesteldProduct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.BesteldeProducten.Add(BesteldProduct);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBesteldProductById", new { id = BesteldProduct.Id }, BesteldProduct);
        }

        // PUT: api/BesteldProduct/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBesteldProduct(int id, [FromBody] BesteldProduct BesteldProduct)
        {
            if (id != BesteldProduct.Id)
            {
                return BadRequest();
            }
            _context.Entry(BesteldProduct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Producten.Any(p => p.Id == id))
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

        // DELETE: api/BesteldProduct/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBesteldProduct(int id)
        {
            var BesteldProduct = await _context.BesteldeProducten.FindAsync(id);
            if (BesteldProduct == null)
            {
                return NotFound();
            }
            _context.BesteldeProducten.Remove(BesteldProduct);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
