using DatabaseHandler;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Web.ResponseObjects;

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