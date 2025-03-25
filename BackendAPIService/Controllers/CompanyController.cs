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
    public ActionResult<List<Web.Company>> GetComapnies( int UserID, int limit = 50, int offset = 0, string? searchString = null)
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
    public ActionResult<SimpleErrorResponse> CreateCompany(string userID, string companyName)
    {
        try 
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            // Check if the user exists
            var user = _dbContext.Users.FirstOrDefault(u => u.UserID.ToString() == userID);
            if (user == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "User not found." });
            }

            // Create a new company
            var newCompany = new Database.Company { CompanyName = companyName };
            _dbContext.Companies.Add(newCompany);
            _dbContext.SaveChanges();

            // Associate the user with the company
            var userCompany = new UserCompany
            {
                UserID = user.UserID,
                CompanyID = newCompany.CompanyID,
                ValidUntil = DateTime.MaxValue,
                LastChange = DateTime.UtcNow
            };
            _dbContext.UserCompanies.Add(userCompany);
            
            // Set user role to Admin
            user.Roles = "Admin";
            
            _dbContext.SaveChanges();

            return StatusCode(201, new SimpleErrorResponse { Success = true, Message = "Successfully created a new company and assigned the user as Admin." });
        } 
        catch (Exception ex) 
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while creating the company." });
        }
    }

    [HttpGet]
    [Route("getRoles")]
    public ActionResult<List<Web.Role>> GetRoles (int companyId, int limit = 50, int offset = 0)
    {
        try 
        {

            var allRoles = _dbContext.Roles.Find(companyId);
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
    public ActionResult<SimpleErrorResponse> CreateRole( string roleName)
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
   public ActionResult<List<Web.Role>> GetAllUsers(int companyId, int limit = 50, int offset = 0)
    {
        try
        {
            var usersInCompany = _dbContext.Users
                .Join(_dbContext.UserCompanies, // Join Users with UserCompany table
                    user => user.UserID,
                    userCompany => userCompany.UserID,
                    (user, userCompany) => new { user, userCompany })
                .Where(joined => joined.userCompany.CompanyID == companyId) // Filter by CompanyID
                .Select(joined => joined.user) // Select only user data
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Ok(usersInCompany);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while fetching users: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse
            {
                Success = false,
                Message = "An error occurred while fetching users."
            });
        }
    }

   
    [HttpPost]
    [Route("createUser")]
    public ActionResult<SimpleErrorResponse> CreateUser(string userName, string userEmail, string userRole, int companyID)
    {
        try 
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                return StatusCode(400, new SimpleErrorResponse { Success = false, Message = "Name, Email, and Role cannot be empty." });
            }

            var companyExists = _dbContext.Companies.Any(c => c.CompanyID == companyID);
            if (!companyExists)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "Company not found" });
            }

            var newUser = new Database.User { Name = userName, Email = userEmail, Roles = userRole };
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            var userID = newUser.UserID;

            var existingAssociation = _dbContext.UserCompanies.Any(uc => uc.UserID == userID && uc.CompanyID == companyID);
            if (existingAssociation)
            {
                return StatusCode(409, new SimpleErrorResponse { Success = false, Message = "User is already associated with this company." });
            }
            var userCompany = new UserCompany
            {
                UserID = userID,
                CompanyID = companyID,
                ValidUntil = DateTime.MaxValue,
                LastChange = DateTime.UtcNow
            };

            _dbContext.UserCompanies.Add(userCompany);
            _dbContext.SaveChanges();

            return StatusCode(201, new SimpleErrorResponse { Success = true, Message = "Successfully created a new user and associated with the company." });
        } 
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while creating the user." });
        }
    }


    [HttpPut]
    [Route("editCompany/{companyId}")]
    public ActionResult<SimpleErrorResponse> EditCompany(int companyId, string companyName)
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
    public ActionResult<SimpleErrorResponse> DeleteRole( int roleId)
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




    [HttpDelete]
    [Route("deleteCompany/{companyId}")]
    public ActionResult<SimpleErrorResponse> DeleteCompany(int companyId)
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

    [HttpPut]
    [Route("editUser/{userId}")]
    public ActionResult<SimpleErrorResponse> EditUser(int userId, string? userName = null, string? userEmail = null, string? userRole = null)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.UserID == userId);
            
            if (user == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "User not found." });
            }

            if (!string.IsNullOrEmpty(userName))
            {
                user.Name = userName;
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                user.Email = userEmail;
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                user.Roles = userRole;
            }

            _dbContext.SaveChanges();

            return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully updated the user." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while editing the user." });
        }
    }
    [HttpDelete]
    [Route("deleteUser/{userId}")]
    public ActionResult<SimpleErrorResponse> DeleteUser(int userId)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
            {
                return StatusCode(404, new SimpleErrorResponse { Success = false, Message = "User not found." });
            }

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();

            return StatusCode(200, new SimpleErrorResponse { Success = true, Message = "Successfully deleted the user." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse { Success = false, Message = "An error occurred while deleting the user." });
        }
    }

}