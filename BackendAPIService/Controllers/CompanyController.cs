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
    public ActionResult<List<Web.Company>> GetComapnies(int userID, int limit = 50, int offset = 0, string? searchString = null)
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
            int newCompanyId = GenerateUniqueId(true, false);
            var newCopany = new Database.Company { CompanyID = newCompanyId, CompanyName = companyName};
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
    public ActionResult<List<Web.Role>> GetRoles (int userID, int limit = 50, int offset = 0)
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
            int newRoleId = GenerateUniqueId(false, true);
            var newRole = new Database.Role {RoleID = newRoleId, Name = roleName};
            _dbContext.Roles.Add(newRole);
            _dbContext.SaveChanges();
            return StatusCode(200, new SimpleErrorResponse{Success = true, Message = "Successfully created a new role."});
        } catch(Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse {Success = false, Message = "An error occurred while creating a role."});
        }
    }
    [HttpGet]
    [Route("getAllUsers")]
    public ActionResult<List<Web.Role>> GetAllUsers (int userID, int limit = 50, int offset = 0)
    {
        try 
        {
            var allUsers = _dbContext.Users;
            StatusCode(200, new SimpleErrorResponse{Success = true, Message = "Successfully fetched all users."});
            return Ok(allUsers);
        } catch(Exception ex) 
        {
            Console.WriteLine("An error occurred while fetching users: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An eror occurred while fetching users."});
        }

    }
   
    [HttpPost]
    [Route("createUser")]
    public ActionResult<SimpleErrorResponse> CreateUser(int userID, string userName, string userEmail, string userRole)
    {
        try 
        {
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                return StatusCode(500, new SimpleErrorResponse{Message = "Name, Email and Role can not be empty."});
            }
            int newUserId = GenerateUniqueId(false, false);
            var newUser = new Database.User{UserID = newUserId, Name = userName,  Email = userEmail, Roles = userRole};
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
            return StatusCode(200, new SimpleErrorResponse{Success = true, Message =" Successfully created a new user"});
        } catch(Exception ex)
        {
            Console.WriteLine("An error occured: {0}",  ex.Message);
            return StatusCode(500, new SimpleErrorResponse{Success = false, Message = "An error occured while creating an user."});
        }

    }

[HttpPut]
[Route("editCompany/{companyId}")]
public ActionResult<SimpleErrorResponse> EditCompany(int companyId, int userID, string companyName)
{
    try
    {
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
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while editing the company." });
    }
}

[HttpDelete]
[Route("deleteRole/{roleId}")]
public ActionResult<SimpleErrorResponse> DeleteRole(int userID, int roleId)
{
    try
    {
        var role = _dbContext.Roles.FirstOrDefault(r => r.RoleID == roleId);

        
        if (role == null)
        {
            return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Role not found." });
        }

    
        _dbContext.Roles.Remove(role);
        _dbContext.SaveChanges();

        return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully deleted the role." });
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while deleting the role." });
    }
}



    private int GenerateUniqueId(Boolean isCompany, Boolean isRole)
    {
        Random random = new Random();
        int Id;
        bool isUnique = false;
        if (isCompany)
        {
            do
            {
                Id = random.Next(1, int.MaxValue); 
                isUnique = !_dbContext.Companies.Any(c => c.CompanyID == Id);
            } while (!isUnique);

            return Id;
        }
        else if(isRole)
        {
            do
            {
                Id = random.Next(1, int.MaxValue); 
                isUnique = !_dbContext.Roles.Any(c => c.RoleID == Id);
            } while (!isUnique);
            return Id;
        }

        else
        {
            do
            {
                Id = random.Next(1, int.MaxValue); 
                isUnique = !_dbContext.Users.Any(c => c.UserID == Id);
            } while (!isUnique);
            return Id; 
        }
    }

[HttpDelete]
[Route("deleteCompany/{companyId}")]
public ActionResult<SimpleErrorResponse> DeleteCompany(int companyId, int userID)
{
    try
    {
        var company = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);

        if (company == null)
        {
            return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Company not found." });
        }

        _dbContext.Companies.Remove(company);
        _dbContext.SaveChanges();

        return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully deleted the company." });
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while deleting the company." });
    }
}

}