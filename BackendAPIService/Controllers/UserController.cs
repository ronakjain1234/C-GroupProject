using DatabaseHandler;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using DatabaseHandler.Data.Models.Database.ReferencingTables;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace BackendAPIService.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _config;

        public UserController(ApplicationDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string userEmail, string userPassword)
        {
            if (await _dbContext.UserEmail.AnyAsync(u => u.Email == userEmail))
                return BadRequest("Email already exists.");

            // generate salt + hash
            var salt  = RandomNumberGenerator.GetBytes(16);
            var hash  = new Rfc2898DeriveBytes(userPassword, salt, 100_000, HashAlgorithmName.SHA256)
                            .GetBytes(32);
            var combinedHash = Convert.ToBase64String(salt.Concat(hash).ToArray());

            // NOTE: you may want to change this to use a Guid directly rather than GetHashCode()
            var userId = Guid.NewGuid().GetHashCode();
            await _dbContext.UserEmail.AddAsync(new UserEmail {
                UserID     = userId,
                Email      = userEmail,
                LastChange = DateTime.UtcNow
            });
            await _dbContext.UserPassword.AddAsync(new UserPassword {
                UserID     = userId,
                Password   = combinedHash,
                LastChange = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            return Ok("User registered.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Email and password are required.");
            }

            var emailEntry = await _dbContext.UserEmail
                                            .FirstOrDefaultAsync(e => e.Email == email);
            if (emailEntry == null) 
                return Unauthorized();

            var pwdEntry = await _dbContext.UserPassword
                                        .FirstOrDefaultAsync(p => p.UserID == emailEntry.UserID);
            if (pwdEntry == null) 
                return Unauthorized();

            // split salt + hash
            var fullBytes   = Convert.FromBase64String(pwdEntry.Password);
            var salt        = fullBytes[..16];
            var storedHash  = fullBytes[16..];
            var enteredHash = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256)
                                .GetBytes(32);

            if (!CryptographicOperations.FixedTimeEquals(storedHash, enteredHash))
                return Unauthorized();

            // Check JWT configuration
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, "JWT configuration is missing. Please check the application configuration.");
            }

            // build claims
            var userId = emailEntry.UserID.ToString();
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            // create signing credentials
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // generate token
            var issuer = _config["Jwt:Issuer"] ?? "DefaultIssuer";
            var audience = _config["Jwt:Audience"] ?? "DefaultAudience";
            
            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token   = new JwtSecurityTokenHandler().WriteToken(token),
                expires = token.ValidTo,
                userId = emailEntry.UserID
            });
        }
    }
}