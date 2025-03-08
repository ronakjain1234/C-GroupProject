using BackendAPIService.RequestObjects;
using BackendAPIService.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
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
    
    [HttpPost]
    [Route("add")]
    public ActionResult Add(EndPointAddRequest endPointAddRequest)
    {
        throw new NotImplementedException();
    }
    
    [HttpGet]
    [Route("get")]
    public ActionResult Get(int userID, int companyID)
    {
        throw new NotImplementedException();
    }

}