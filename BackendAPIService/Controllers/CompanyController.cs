using BackendAPIService.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.ReferencingTables;
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
            StatusCode(200, new SimpleErrorResponse{Success = true, Message = "Successfully fetched all companies."});
            return Ok(allCompanies);
        } catch (Exception ex) 
        {   
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Message = "An error occurred while fetching companies."});
        }
        
    }

    [HttpPost]
    [Route("createCompany")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        try 
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "Company name cannot be empty."});
            }
            var newCopany = new Database.Company { CompanyName = companyName};
            _dbContext.Companies.Add(newCopany);
            _dbContext.SaveChanges();
            return StatusCode(200, new SimpleErrorResponse {Success = true, Message = "Succesfully created a new company."});
        } catch (Exception ex) 
        {
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating the company"});
        }
    }
    [HttpGet]
    [Route("getRoles")]
    public ActionResult<List<Web.Role>> Get (int userID, int limit = 50, int offset = 0)
    {
        try 
        {
            var allRoles = _dbContext.Roles;
            StatusCode(200, new SimpleErrorResponse{Success = true, Message = "Successfully fetched all roles."});
            return Ok(allRoles);
        } catch(Exception ex) 
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An eror occurred when fetching the roles"});
        }

    }

    [HttpPost]
    [Route("createRole")]
    public ActionResult<SimpleErrorResponse> CreateRole(int userID, string roleName)
    {
        try {
            if (string.IsNullOrEmpty(roleName))
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "Role name cannot be empty"});
            }

            var newRole = new Database.Role {Name = roleName};
            _dbContext.Roles.Add(newRole);
            _dbContext.SaveChanges();
            return StatusCode(200, new SimpleErrorResponse{Success = true, Message = "Successfully created a new role."});
        } catch(Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating a role."});
        }
    }
}