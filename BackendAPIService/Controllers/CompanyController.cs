using BackendAPIService.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = MyMudBlazorApp.Objects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
using MyMudBlazorApp.Pages;
using DatabaseHandler.Data.Models.Database;
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
        using  (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new SimpleErrorResponse {Success = false, Message = "Company name cannot be empty."});
            }
            var newCompany = new Database.Company {CompanyName = companyName};
            newCompany.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Companies.Add(newCompany);
            _dbContext.SaveChanges(); 

            var userCompany = new UserCompany() {CompanyID = newCompany.CompanyID, UserID = userID};
            userCompany.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserCompanies.Add(userCompany);
            _dbContext.SaveChanges(); 

            var localAdminRole = new Database.Role() {Name = "CustomerAdmin"};
            localAdminRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Roles.Add(localAdminRole);
            _dbContext.SaveChanges(); 

            var companyRole = new CompanyRole() {CompanyID = newCompany.CompanyID, RoleID = localAdminRole.RoleID};
            companyRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.CompanyRoles.Add(companyRole);
            _dbContext.SaveChanges(); 

            var userRole = new UserRole() {UserID = userID, RoleID = companyRole.RoleID};
            userRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserRoles.Add(userRole);
            
            _dbContext.SaveChanges();
            transaction.Commit();
            return Ok();
        } 
        catch (Exception ex) 
        {
            transaction.Rollback();
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating the company"});
        }
    }

    [HttpPost]
    [Route("createUser")]
    public ActionResult<SimpleErrorResponse> CreateUser(string userName, string userEmail)
    {
        using  (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
            {
                return StatusCode(400, new SimpleErrorResponse {Success = false, Message = "Parameters undefined."});
            }
            var newUser = new Database.User {Name = userName};
            newUser.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges(); 
            var userId = newUser.UserID;
            var newUserEmail = new Database.ReferencingTables.UserEmail {UserID = userId, Email = userEmail};
            newUserEmail.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserEmail.Add(newUserEmail);
            _dbContext.SaveChanges();
            transaction.Commit();
            return Ok();
        } 
        catch (Exception ex) 
        {   
            transaction.Rollback();
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating the user"});
        }
    }

    [HttpPut]
    [Route("changeCompanyName")]

    public ActionResult<SimpleErrorResponse> ChangeCompanyName (int userId, int companyId, string companyName)
    {
        try
        { 
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == userId);
            if (!hasAccess)
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }

            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            existingCompany.CompanyName = companyName;

            _dbContext.SaveChanges();

            return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully updated the company." });


        } catch(Exception ex)  
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while editing the company." });
        }
    }

    [HttpPost]
[Route("addUser")]
public ActionResult<SimpleErrorResponse> AddUser(int mainUserId, string email, int companyId)
{
    try
    {
        // Check if the main user has access to the company
        var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
        if (!hasAccess)
        {
            return StatusCode(403, new SimpleErrorResponse { Success = false, Message = "User does not have access" });
        }

        // Find the userId based on the provided email
        var userEmailEntry = _dbContext.UserEmail.FirstOrDefault(ue => ue.Email == email);
        if (userEmailEntry == null)
        {
            return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "User with this email not found." });
        }

        int userId = userEmailEntry.UserID;

        // Check if the company exists
        var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
        if (existingCompany == null)
        {
            return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Company not found." });
        }

        // Check if the user is already added
        bool userExists = _dbContext.UserCompanies.Any(uc => uc.UserID == userId && uc.CompanyID == companyId);
        if (userExists)
        {
            return StatusCode(409, new SimpleErrorResponse { Success = false, Message = "User is already associated with this company." });
        }

        // Add the user to the company
        var newUser = new Database.MixedTables.UserCompany
        {
            UserID = userId,
            CompanyID = companyId,
            LastChange = DateTime.UtcNow
        };

        _dbContext.UserCompanies.Add(newUser);
        _dbContext.SaveChanges();

        return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "User successfully added to company" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while adding the user." });
    }
}


    [HttpDelete]
    [Route("removeUser")]
    public ActionResult<SimpleErrorResponse> RemoveUser(int mainUserId, int userId, int companyId)
    {
        try
        {
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }
                
            var userCompany = _dbContext.UserCompanies
                .FirstOrDefault(uc => uc.UserID == userId && uc.CompanyID == companyId);

            if (userCompany == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "User not found in company." });
            }

            _dbContext.UserCompanies.Remove(userCompany);
            _dbContext.SaveChanges();

            return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "User removed successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while removing the user." });
        }
    }


    [HttpPut]
    [Route("addRoletoUser")]
    public ActionResult<SimpleErrorResponse> AddRoleToUser (int mainUserId, int userId, int companyId, int roleId)
    {
        try 
        {
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            var existingRole = _dbContext.CompanyRoles.Any(cr => cr.CompanyID == companyId && cr.RoleID == roleId);
            if (!existingRole)
            {
                return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "Role does not exist in compnay"});
            }

            var newUserRole = new Database.MixedTables.UserRole {UserID = userId, RoleID = roleId};
            newUserRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserRoles.Add(newUserRole);
            _dbContext.SaveChanges();

            return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully added user"});
        } catch (Exception ex)
        {
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while adding role to user"});
        }   
    }

}

    
