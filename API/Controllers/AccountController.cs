using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using API.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using API.Data;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<Gebruiker> _userManager;
    private readonly IConfiguration _configuration;
    private readonly StartspelerContext _context;

    public AccountController(UserManager<Gebruiker> userManager, IConfiguration configuration, StartspelerContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new Gebruiker
            {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                PhoneNumber = model.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Logica voor het versturen van een bevestigingsmail kan hier toegevoegd worden

                return Ok(new { message = "User registered successfully!" });
            }
            else
            {
                // Verzamel foutberichten
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors = errors });
            }
        }
        return BadRequest(ModelState);
    }

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string Voornaam { get; set; } = default!;

        [Required]
        public string Achternaam { get; set; } = default!;

        public string? PhoneNumber { get; set; } = default!;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            // Correcte inloggegevens, genereer token en stuur deze naar de gebruiker
            var token = GenerateJwtToken(user);

            // Haal de gebruikersrechten op
            var gebruikerRechten = await _context.GebruikerRechten
                .Include(gr => gr.Rechten)
                .Where(gr => gr.GebruikerId == user.Id)
                .ToListAsync();

            // Creëer een object dat zowel de token als de UserId bevat
            var loginResult = new
            {
                Token = token,
                UserId = user.Id, // Zorg ervoor dat je gebruiker een eigenschap Id heeft die je hier kunt gebruiken
                Email = user.Email,
                GebruikerRechten = gebruikerRechten.Select(gr => gr.Rechten.RechtNaam).ToList()
            };

            return Ok(loginResult);
        }

        return Unauthorized("Ongeldige inloggegevens.");
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

    [HttpGet("detailsByEmail")]
    public async Task<IActionResult> GetUserDetailsByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"User with email {email} not found.");
        }

        // Aanpassen welke gegevens je wilt teruggeven
        var userDetails = new
        {
            user.Id,
            user.Voornaam,
            user.Achternaam,
        };

        return Ok(userDetails);
    }

    [HttpGet("searchUsers")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query is required.");
        }

        var users = await _userManager.Users
            .Where(u => u.Voornaam.Contains(query) || u.Achternaam.Contains(query) || u.Email.Contains(query))
            .Select(u => new Gebruiker
            {
                Id = u.Id,
                Voornaam = u.Voornaam,
                Achternaam = u.Achternaam,
                Email = u.Email,
                // Vul andere benodigde velden in
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("deleteUser")]
    public async Task<IActionResult> DeleteUser([FromQuery] string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok("User deleted successfully.");
        }

        return BadRequest("Error deleting user.");
    }

    [HttpGet("getUserRoles")]
    public async Task<IActionResult> GetUserRoles([FromQuery] string userId)
    {
        var user = await _context.Gebruikers
            .Include(u => u.GebruikerRechten)
            .ThenInclude(gr => gr.Rechten)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var roles = user.GebruikerRechten.Select(gr => gr.Rechten.RechtNaam).ToList();
        return Ok(roles);
    }

    [HttpPatch("setUserRoles")]
    public async Task<IActionResult> SetUserRoles([FromBody] SetUserRolesModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Gebruikers
            .Include(u => u.GebruikerRechten)
            .FirstOrDefaultAsync(u => u.Id == model.UserId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Remove existing roles
        _context.GebruikerRechten.RemoveRange(user.GebruikerRechten);
        await _context.SaveChangesAsync();

        // Add new roles
        var roles = await _context.Rechten.Where(r => model.Roles.Contains(r.RechtNaam)).ToListAsync();
        foreach (var role in roles)
        {
            user.GebruikerRechten.Add(new GebruikerRechten { GebruikerId = user.Id, RechtenId = role.Id });
        }

        await _context.SaveChangesAsync();
        return Ok("User roles updated successfully.");
    }

    public class SetUserRolesModel
    {
        public string UserId { get; set; } = default!;
        public List<string> Roles { get; set; } = default!;
    }
}