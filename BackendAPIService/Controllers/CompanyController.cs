using BackendAPIService.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
using Microsoft.AspNetCore.Mvc;
namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/company/")]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public CompanyController(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    
    [HttpGet]
    [Route("get")]
    public ActionResult<List<Web.Company>> Get(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        try
        {
            var allCompanies = _dbContext.Companies;
            return Ok(allCompanies);
        } catch (Exception ex) 
        {   
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Message = "An error occurred while fetching companies."});
        }
        
    }

    [HttpGet]
    [Route("create")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        _dbContext.Companies.Add(new Database.Company());
        _dbContext.SaveChanges();
        return Ok();
        throw new NotImplementedException();
    }

}