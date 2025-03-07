using BackendAPIService.ResponseObjects;
using DatabaseHandler.Data;
using DatabaseHandler.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    private ApplicationDbContext _dbContext;
    
    public CompanyController(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    
    [HttpGet]
    [Route("/getCompanies")]
    public ActionResult<List<Company>> Get(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        return Ok(_dbContext.Companies.First().CompanyName);
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/createCompany")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        _dbContext.Companies.Add(new DBCompany(companyName));
        return Ok();
        throw new NotImplementedException();
    }

}