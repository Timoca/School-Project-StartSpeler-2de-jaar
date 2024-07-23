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
    public class DrankController : ControllerBase
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly StartspelerContext _context;

        public DrankController(UserManager<Gebruiker> userManager, StartspelerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/Drank
        //Product en Drank zijn dezelfde dingen alleen andere benaming dankzij de mobile app
        [HttpGet]
        public IActionResult GetAllDrank()
        {
            var producten = _context.Producten.ToList();
            return Ok(producten);
        }

        [HttpGet("{id}")]
        public IActionResult GetDrankById(int id)
        {
            var drankItem = _context.Producten.FirstOrDefault(e => e.Id == id);
            if (drankItem == null)
            {
                return NotFound();
            }
            return Ok(drankItem);
        }


    }

    
}
