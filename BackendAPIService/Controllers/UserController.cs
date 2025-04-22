using DatabaseHandler;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Web.ResponseObjects;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using DatabaseHandler.Data.Models.Database.ReferencingTables;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private ApplicationDbContext _dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    


    [HttpPost("register")]
    public async Task<IActionResult> Register(string userEmail, string userPassword)
    {
        if (await _dbContext.UserEmail.AnyAsync(u => u.Email == userEmail))
            return BadRequest("Email already exists.");

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = new Rfc2898DeriveBytes(userPassword, salt, 100000, HashAlgorithmName.SHA256).GetBytes(32);
        var combinedHash = Convert.ToBase64String(salt.Concat(hash).ToArray());

        var userId = Guid.NewGuid().GetHashCode();
        await _dbContext.UserEmail.AddAsync(new UserEmail {
            UserID = userId,
            Email = userEmail,
            LastChange = DateTime.UtcNow
        });
        await _dbContext.UserPassword.AddAsync(new UserPassword {
            UserID = userId,
            Password = combinedHash,
            LastChange = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return Ok("User registered.");
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var emailEntry = await _dbContext.UserEmail.FirstOrDefaultAsync(e => e.Email == email);
        if (emailEntry == null) return Unauthorized();

        var userPassword = await _dbContext.UserPassword.FirstOrDefaultAsync(p => p.UserID == emailEntry.UserID);
        if (userPassword == null) return Unauthorized();

        var fullBytes = Convert.FromBase64String(userPassword.Password);
        var salt = fullBytes[..16];
        var storedHash = fullBytes[16..];

        var enteredHash = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256).GetBytes(32);
        if (!CryptographicOperations.FixedTimeEquals(storedHash, enteredHash))
            return Unauthorized();

        return Ok("Login successful.");
    }
}