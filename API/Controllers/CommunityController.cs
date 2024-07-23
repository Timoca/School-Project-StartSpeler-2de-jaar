using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly StartspelerContext _context;

        public CommunityController(StartspelerContext context)
        {
            _context = context;
        }

        // GET: api/Community
        [HttpGet]
        public IActionResult GetAllCommunities()
        {
            var communities = _context.Communities.ToList();
            return Ok(communities);
        }
        // POST: api/Community
        [HttpPost]
        public async Task<IActionResult> CreateCommunity([FromBody] Community communityItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Communities.Add(communityItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCommunityById", new { id = communityItem.Id }, communityItem);
        }
        // GET: api/Community/{id}
        [HttpGet("{id}")]
        public IActionResult GetCommunityById(int id)
        {
            var communityItem = _context.Communities.FirstOrDefault(e => e.Id == id);
            if (communityItem == null)
            {
                return NotFound();
            }
            return Ok(communityItem);
        }
        // DELETE: api/Community/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommunity(int id)
        {
            var communityItem = await _context.Communities.FindAsync(id);
            if (communityItem == null)
            {
                return NotFound();
            }
            _context.Communities.Remove(communityItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
