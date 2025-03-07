using DatabaseHandler.Data;
using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private ApplicationDbContext _dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [Route("/getUsers")]
    public ActionResult<List<User>> GetUsers()
    {
        throw new NotImplementedException();
    }
}