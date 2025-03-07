using BackendAPIService.ResponseObjects;
using DatabaseHandler.Data;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
using Microsoft.AspNetCore.Mvc;
namespace BackendAPIService.Controllers;

[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public CompanyController(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    
    [HttpGet]
    [Route("api/companies/getCompanies")]
    public ActionResult<List<Web.Company>> Get(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        return Ok(_dbContext.Companies);
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("api/companies/createCompany")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        _dbContext.Companies.Add(new Database.Company(companyName));
        _dbContext.SaveChanges();
        return Ok();
        throw new NotImplementedException();
    }

}