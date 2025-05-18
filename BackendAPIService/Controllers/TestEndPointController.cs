using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DatabaseHandler;
using DatabaseHandler.Data.Models.Database.ReferencingTables;
using DatabaseHandler.Data.Models.Web.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/testendpoint")]
public class TestEndpointController : ControllerBase
{
    private ApplicationDbContext _dbContext;

    public TestEndpointController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [Route("testing")]
    public string TestEndpoint()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            bool validToken = _dbContext.UserBearer.Any(e => e.BearerToken == token);

            if (validToken)
            {
                return "DTU is the best technical university in the EU";    
            }
        }
        return "No Authorization";
    }
    
    [HttpGet]
    [Route("generateBearerToken")]
    public ActionResult<string> GenerateBearerToken()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string userIDString = userID.ToString();

            var tokenData = $"{userIDString}:{timestamp}:{Convert.ToBase64String(randomBytes)}";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
                String bearer = Convert.ToBase64String(hashBytes);
                _dbContext.UserBearer.Add(new UserBearer
                {
                    UserID = userID,
                    BearerToken = bearer,
                    LastChange = DateTime.Now
                });
                _dbContext.SaveChanges();
                return Ok("Bearer " + Convert.ToBase64String(hashBytes));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occured {ex.Message}");
        }
    }
}