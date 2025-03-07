using DatabaseHandler.Data;
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
}