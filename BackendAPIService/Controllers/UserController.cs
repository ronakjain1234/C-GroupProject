using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    [HttpGet]
    [Route("/get")]
    public ActionResult<List<User>> GetUsers()
    {
        throw new NotImplementedException();
    }
}