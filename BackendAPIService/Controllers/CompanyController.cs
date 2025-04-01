using BackendAPIService.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
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
    public ActionResult<List<string>> GetCompanies(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        try
        {
            var companies = from company in _dbContext.UserCompanies where company.UserID == userID select company;
            List<string> companyNameList = new List<string>();
            foreach (var company in companies)
            {
                companyNameList.Add(_dbContext.Companies.Find(company.CompanyID)!.CompanyName);
            }
            
            if (!string.IsNullOrEmpty(searchString))
            {
                List<string> returnList = new List<string>();
                foreach (var name in companyNameList)
                {
                    if (!name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        returnList.Add(name);
                    }

                    return returnList;
                }
            }
            
            return Ok(companyNameList);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occured: {0}", e.Message);
            return StatusCode(500, new SimpleErrorResponse { Message = "An error occurred while fetching companies."});
        }
        
    }

    [HttpPost]
    [Route("createCompany")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID ,string companyName)
    {
        try
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new SimpleErrorResponse {Success = false, Message = "Company name cannot be empty."});
            }
            var newCompany = new Database.Company {CompanyName = companyName};
            newCompany.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Companies.Add(newCompany);
            
            var userCompany = new UserCompany() {CompanyID = newCompany.CompanyID, UserID = userID};
            userCompany.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserCompanies.Add(userCompany);
            
            var localAdminRole = new Database.Role() {Name = "CustomerAdmin"};
            localAdminRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Roles.Add(localAdminRole);
            
            var companyRole = new CompanyRole() {CompanyID = newCompany.CompanyID, RoleID = localAdminRole.RoleID};
            companyRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.CompanyRoles.Add(companyRole);
            
            var userRole = new UserRole() {UserID = userID, RoleID = companyRole.RoleID};
            userRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserRoles.Add(userRole);
            
            _dbContext.SaveChanges();
            return Ok();
        } 
        catch (Exception ex) 
        {
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating the company"});
        }
    }

}