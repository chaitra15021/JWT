using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewProj.MyFirstApi.Models; // Reference the LoginDTO model
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace NewProj.MyFirstApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input data." });
            }

            var users = ReadUsersFromJson();
            if (users == null || !users.Any())
            {
                return StatusCode(500, new { Message = "No users found in the system." });
            }

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == loginDto.Password);

            if (user != null)
            {
                var token = GenerateJwtToken(user.Email);
                return Ok(new { Token = token });
            }

            return Unauthorized(new { Message = "Invalid email or password." });
        }

        private string GenerateJwtToken(string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_key"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private List<LoginDTO> ReadUsersFromJson()
        {
            try
            {
                // Use the absolute path to the users.json file
                var filePath = @"C:\Users\chaitra.m4\Documents\NewProj\myfirstapi\Models\users.json";
                Console.WriteLine($"Looking for users.json at: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return new List<LoginDTO>();
                }

                var jsonData = System.IO.File.ReadAllText(filePath);
                Console.WriteLine($"JSON Data: {jsonData}");

                return JsonSerializer.Deserialize<List<LoginDTO>>(jsonData) ?? new List<LoginDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading users.json: {ex.Message}");
                return new List<LoginDTO>();
            }
        }
    }
}