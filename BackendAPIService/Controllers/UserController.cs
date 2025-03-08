using BackendAPIService.RequestObjects;
using DatabaseHandler;
using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

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

    [HttpGet]
    [Route("get")]
    public ActionResult<List<User>> GetUsers()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("add-user")]
    public async Task<ActionResult> AddUserToBank([FromBody] UserToBankRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.BankName))
        {
            return BadRequest("Invalid request data.");
        }

        var bank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Name == request.BankName);
        if (bank == null)
        {
            return NotFound($"Bank {request.BankName} not found.");
        }

        var existingUser = await _dbContext.SelectedUsers
            .FirstOrDefaultAsync(u => u.BankId == bank.Id && u.UserName == request.UserName);

        if (existingUser != null)
        {
            return Conflict($"User {request.UserName} is already added to {request.BankName}.");
        }

        var newUser = new SelectedUser
        {
            BankId = bank.Id,
            UserName = request.UserName
        };
        
        _dbContext.SelectedUsers.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"User {request.UserName} added to {request.BankName}" });
    }
}