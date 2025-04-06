using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = DatabaseHandler.Data.Models.Web.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
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
    public List<Web.GetAllCompaniesResponse> GetCompanies(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        try
        {
            var companies = from company in _dbContext.UserCompanies where company.UserID == userID select company;
            List<string> companyNameList = new List<string>();
            foreach (var company in companies)
            {
                companyNameList.Add(_dbContext.Companies.Find(company.CompanyID)!.CompanyName);
            }
            List<Web.GetAllCompaniesResponse> returnList = new();
            if (!string.IsNullOrEmpty(searchString))
            {
                
                foreach (var name in companyNameList)
                {
                    if (!name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        returnList.Add(new Web.GetAllCompaniesResponse() {companyName = name, imageURL = ""});
                    }

                    return returnList;
                }
            }
            foreach (var name in companyNameList)
            {
                returnList.Add(new Web.GetAllCompaniesResponse() {companyName = name, imageURL = ""});
            }
            return returnList;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occured: {0}", e.Message);
            return new();
        }
        
    }

    [HttpPost]
    [Route("createCompany")]
    public ActionResult<Web.SimpleErrorResponse> CreateCompany(int userID ,string companyName)
    {
        using  (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new Web.SimpleErrorResponse {Success = false, Message = "Company name cannot be empty."});
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
            return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "An error occurred while creating the company"});
        }
    }

    [HttpPost]
    [Route("createUser")]
    public ActionResult<Web.SimpleErrorResponse> CreateUser(string userName, string userEmail)
    {
        using  (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
            {
                return StatusCode(400, new Web.SimpleErrorResponse {Success = false, Message = "Parameters undefined."});
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
            return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "An error occurred while creating the user"});
        }
    }

    [HttpPut]
    [Route("changeCompanyName")]

    public ActionResult<Web.SimpleErrorResponse> ChangeCompanyName (int userId, int companyId, string companyName)
    {
        try
        { 
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == userId);
            if (!hasAccess)
            {
                return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }

            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            existingCompany.CompanyName = companyName;

            _dbContext.SaveChanges();

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "Successfully updated the company." });


        } catch(Exception ex)  
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while editing the company." });
        }
    }

    [HttpPost]
    [Route("addUser")]
    public ActionResult<Web.SimpleErrorResponse> AddUser(int mainUserId, string email, int companyId)
    {
        try
        {
            // Check if the main user has access to the company
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have access" });
            }

            // Find the userId based on the provided email
            var userEmailEntry = _dbContext.UserEmail.FirstOrDefault(ue => ue.Email == email);
            if (userEmailEntry == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "User with this email not found." });
            }

            int userId = userEmailEntry.UserID;

            // Check if the company exists
            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            // Check if the user is already added
            bool userExists = _dbContext.UserCompanies.Any(uc => uc.UserID == userId && uc.CompanyID == companyId);
            if (userExists)
            {
                return StatusCode(409, new Web.SimpleErrorResponse { Success = false, Message = "User is already associated with this company." });
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

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "User successfully added to company" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while adding the user." });
        }
    }


    [HttpDelete]
    [Route("removeUser")]
    public ActionResult<Web.SimpleErrorResponse> RemoveUser(int mainUserId, int userId, int companyId)
    {
        try
        {
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }
                
            var userCompany = _dbContext.UserCompanies
                .FirstOrDefault(uc => uc.UserID == userId && uc.CompanyID == companyId);

            if (userCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "User not found in company." });
            }

            _dbContext.UserCompanies.Remove(userCompany);
            _dbContext.SaveChanges();

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "User removed successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while removing the user." });
        }
    }


    [HttpPut]
    [Route("addRoletoUser")]
    public ActionResult<Web.SimpleErrorResponse> AddRoleToUser (int mainUserId, int userId, int companyId, int roleId)
    {
        try 
        {
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            var existingRole = _dbContext.CompanyRoles.Any(cr => cr.CompanyID == companyId && cr.RoleID == roleId);
            if (!existingRole)
            {
                return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "Role does not exist in compnay"});
            }

            var newUserRole = new Database.MixedTables.UserRole {UserID = userId, RoleID = roleId};
            newUserRole.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.UserRoles.Add(newUserRole);
            _dbContext.SaveChanges();

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "Successfully added user"});
        } catch (Exception ex)
        {
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "An error occurred while adding role to user"});
        }   
    }

    [HttpDelete]
    [Route("removeRoleFromUser")]
    public ActionResult<Web.SimpleErrorResponse> RemoveRoleFromUser(int mainUserId, int userId, int companyId, int roleId)
    {
        try
        {
            
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == mainUserId);
            if (!hasAccess)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have access" });
            }

        
            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyId);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            
            var existingRole = _dbContext.CompanyRoles.Any(cr => cr.CompanyID == companyId && cr.RoleID == roleId);
            if (!existingRole)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Role does not exist in company" });
            }

           
            var userRole = _dbContext.UserRoles.FirstOrDefault(ur => ur.UserID == userId && ur.RoleID == roleId);
            if (userRole == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "User does not have this role." });
            }

          
            _dbContext.UserRoles.Remove(userRole);
            _dbContext.SaveChanges();

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "Role removed from user successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while removing role from user." });
        }
    }

    [HttpGet]
    [Route("getRolesInCompany")]
    public ActionResult<List<string>> GetCompanyRoles(int companyId, int userId )
    {
        try
        {
            var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyId && uc.UserID == userId);
            if (!hasAccess)
            {
                return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "User does not have access"});
            }
            var roleNames = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyId)
                .Join(_dbContext.Roles, 
                    cr => cr.RoleID, 
                    r => r.RoleID, 
                    (cr, r) => r.Name)
                .Distinct()
                .ToList();

            return Ok(roleNames);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while fetching roles."});
        }
    }

   [HttpDelete]
   [Route("deleteRole")]
   public ActionResult DeleteRole(int userID, int companyID, int roleID)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
               
                var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
                if (!hasAccess)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "User does not have access" });
                }

                
                var companyRole = _dbContext.CompanyRoles
                    .FirstOrDefault(cr => cr.CompanyID == companyID && cr.RoleID == roleID);

                if (companyRole == null)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "Role not assigned to the given company." });
                }

                
                var isRoleAssignedToUser = _dbContext.UserRoles
                    .Any(ur => ur.RoleID == roleID);

                if (isRoleAssignedToUser)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "Role is currently assigned to one or more users and cannot be deleted." });
                }

                
                _dbContext.CompanyRoles.Remove(companyRole);
                _dbContext.SaveChanges();

                
                bool roleExistsInOtherCompanies = _dbContext.CompanyRoles
                    .Any(cr => cr.RoleID == roleID);

                if (!roleExistsInOtherCompanies)
                {
                    
                    var role = _dbContext.Roles.Find(roleID);
                    if (role != null)
                    {
                        _dbContext.Roles.Remove(role);
                        _dbContext.SaveChanges();
                    }
                }

                
                transaction.Commit();
                return Ok("Role successfully deleted.");
            }
            catch (Exception ex)
            {
                
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while fetching roles." });
            }
        }
    }

    

   [HttpPost]
   [Route("createRole")]
   public ActionResult CreateRole(int userID, int companyID, string name)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
                if (!hasAccess)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "User does not have access"});
                }
                var role = _dbContext.Roles.FirstOrDefault(r => r.Name == name);

                
                if (role == null)
                {
                    role = new Role
                    {
                        Name = name,
                        LastChange = DateTime.UtcNow
                    };
                    _dbContext.Roles.Add(role);
                    _dbContext.SaveChanges(); 
                }

                
                bool roleExistsInCompany = _dbContext.CompanyRoles
                    .Any(cr => cr.CompanyID == companyID && cr.RoleID == role.RoleID);

                if (roleExistsInCompany)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "Role exists in company"});
                }

                
                var companyRole = new CompanyRole
                {
                    CompanyID = companyID,
                    RoleID = role.RoleID,
                    LastChange = DateTime.UtcNow
                };
                _dbContext.CompanyRoles.Add(companyRole);
                _dbContext.SaveChanges();

                
                transaction.Commit();
                return Ok("Role successfully created and assigned to company.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occured: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while creating roles."});
            }
        }
    }

    [HttpPut]
    [Route("updateRoles")]
    public ActionResult UpdateRoleName(int userID, int roleID, string name)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                
                var role = _dbContext.Roles.Find(roleID);

                if (role == null)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse {Success = false, Message = "Role does not exist."});
                }

                
                role.Name = name;
                role.LastChange = DateTime.UtcNow;

                _dbContext.SaveChanges();
                transaction.Commit();

                return Ok("Role name updated successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occured: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while updating roles."});
            }
        }
    }

}

    
