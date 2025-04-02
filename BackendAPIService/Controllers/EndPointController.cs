using DatabaseHandler.Data.Models.Web.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Microsoft.AspNetCore.Mvc;
namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/endpoint/")]
public class EndPointController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public EndPointController(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    
    [HttpGet]
    [Route("get")]
    public ActionResult Get(int userID, int companyID)
    {
        throw new NotImplementedException();
    }

}