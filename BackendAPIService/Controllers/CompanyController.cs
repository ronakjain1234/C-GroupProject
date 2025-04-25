using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = DatabaseHandler.Data.Models.Web.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
using DatabaseHandler.Data.Models.Database;
using Microsoft.Extensions.Configuration.UserSecrets;
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
// Return Company Id
public ActionResult<List<Web.GetAllCompaniesResponse>> GetCompanies(int userID, int limit = 50, int offset = 0, string? searchString = null)
{
    try
    {
        // Check if user exists
        bool userExists = _dbContext.Users.Any(u => u.UserID == userID);
        if (!userExists)
        {
            return StatusCode(404, new Web.SimpleErrorResponse 
            { 
                Success = false, 
                Message = "User not found." 
            });
        }

        // Get companies for user
        var companies = from userCompany in _dbContext.UserCompanies
                        where userCompany.UserID == userID
                        select userCompany;

        var companyList = new List<Web.GetAllCompaniesResponse>();

        foreach (var userCompany in companies)
        {
            var company = _dbContext.Companies.Find(userCompany.CompanyID);
            if (company != null)
            {
                companyList.Add(new Web.GetAllCompaniesResponse
                {
                    companyID = company.CompanyID,
                    companyName = company.CompanyName
                });
            }
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            companyList = companyList
                .Where(c => c.companyName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Apply pagination
        var paginatedResult = companyList
            .Skip(offset)
            .Take(limit)
            .ToList();

        return Ok(paginatedResult);
    }
    catch (Exception e)
    {
        Console.WriteLine("An error occurred: {0}", e.Message);
        return StatusCode(500, new Web.SimpleErrorResponse 
        { 
            Message = "An error occurred while fetching companies." 
        });
    }
}



    [HttpPost]
    [Route("createCompany")]
    public ActionResult<Web.SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            // Check for duplicate company name (case-insensitive)
            bool companyExists = _dbContext.Companies.Any(c => c.CompanyName.ToLower() == companyName.ToLower());
            if (companyExists)
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "A company with this name already exists." });
            }

            var newCompany = new Database.Company { CompanyName = companyName };
            newCompany.LastChange = DateTime.UtcNow;
            _dbContext.Companies.Add(newCompany);
            _dbContext.SaveChanges();

            var userCompany = new UserCompany() { CompanyID = newCompany.CompanyID, UserID = userID };
            userCompany.LastChange = DateTime.UtcNow;
            _dbContext.UserCompanies.Add(userCompany);
            _dbContext.SaveChanges();

            var localAdminRole = new Database.Role() { Name = "CustomerAdmin" };
            localAdminRole.LastChange = DateTime.UtcNow;
            _dbContext.Roles.Add(localAdminRole);
            _dbContext.SaveChanges();

            var companyRole = new CompanyRole() { CompanyID = newCompany.CompanyID, RoleID = localAdminRole.RoleID };
            companyRole.LastChange = DateTime.UtcNow;
            _dbContext.CompanyRoles.Add(companyRole);
            _dbContext.SaveChanges();

            var userRole = new UserRole() { UserID = userID, RoleID = companyRole.RoleID };
            userRole.LastChange = DateTime.UtcNow;
            _dbContext.UserRoles.Add(userRole);

            _dbContext.SaveChanges();
            transaction.Commit();
            return Ok();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while creating the company" });
        }
    }


    [HttpPost]
    [Route("createUser")]
    public ActionResult<Web.SimpleErrorResponse> CreateUser(string user, string email)
    {
        using  (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(email))
            {
                return StatusCode(400, new Web.SimpleErrorResponse {Success = false, Message = "Parameters undefined."});
            }
            var newUser = new Database.User {Name = user};
            newUser.LastChange = DateTime.Now.ToUniversalTime();
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges(); 
            var userId = newUser.UserID;
            var newUserEmail = new Database.ReferencingTables.UserEmail {UserID = userId, Email = email};
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
    public ActionResult<Web.SimpleErrorResponse> ChangeCompanyName(int userID, int companyID, string newCompanyName)
    {
        try
        { 
            // Step 1: Get all role IDs for the main user
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == userID)
                .Select(ur => ur.RoleID)
                .ToList();

            // Step 2: Filter role IDs that belong to the company
            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            if (!hasCustomerAdminRole)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }

            if (string.IsNullOrEmpty(newCompanyName))
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            existingCompany.CompanyName = newCompanyName;

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
    public ActionResult<Web.SimpleErrorResponse> AddUser(int mainUserID, string email, int companyID)
    {
        try
        {
        // Step 1: Get all role IDs for the main user
        var userRoleIds = _dbContext.UserRoles
            .Where(ur => ur.UserID == mainUserID)
            .Select(ur => ur.RoleID)
            .ToList();

        // Step 2: Filter role IDs that belong to the company
        var companyRoleIds = _dbContext.CompanyRoles
            .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
            .Select(cr => cr.RoleID)
            .ToList();

        // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
        var hasCustomerAdminRole = _dbContext.Roles
            .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

        if (!hasCustomerAdminRole)
        {
            return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
        }

            // Find the userId based on the provided email
            var userEmailEntry = _dbContext.UserEmail.FirstOrDefault(ue => ue.Email == email);
            if (userEmailEntry == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "User with this email not found." });
            }

            int userId = userEmailEntry.UserID;

            // Check if the company exists
            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            // Check if the user is already added
            bool userExists = _dbContext.UserCompanies.Any(uc => uc.UserID == userId && uc.CompanyID == companyID);
            if (userExists)
            {
                return StatusCode(409, new Web.SimpleErrorResponse { Success = false, Message = "User is already associated with this company." });
            }

            // Add the user to the company
            var newUser = new Database.MixedTables.UserCompany
            {
                UserID = userId,
                CompanyID = companyID,
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
    public ActionResult<Web.SimpleErrorResponse> RemoveUser(int mainUserID, int userID, int companyID)
    {
        try
        {
           // Step 1: Get all role IDs for the main user
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            // Step 2: Filter role IDs that belong to the company
            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            if (!hasCustomerAdminRole)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }
                
            var userCompany = _dbContext.UserCompanies
                .FirstOrDefault(uc => uc.UserID == userID && uc.CompanyID == companyID);

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
    public ActionResult<Web.SimpleErrorResponse> AddRoleToUser(int mainUserID, int userID, int companyID, int roleID)
    {
        try 
        {
            // Step 1: Get all role IDs for the main user
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            // Step 2: Filter role IDs that belong to the company
            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            if (!hasCustomerAdminRole)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            var existingRole = _dbContext.CompanyRoles.Any(cr => cr.CompanyID == companyID && cr.RoleID == roleID);
            if (!existingRole)
            {
                return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "Role does not exist in company" });
            }

            var userInCompany = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
            if (!userInCompany)
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "User is not part of the company" });
            }

            var newUserRole = new Database.MixedTables.UserRole
            {
                UserID = userID,
                RoleID = roleID,
                LastChange = DateTime.UtcNow
            };

            _dbContext.UserRoles.Add(newUserRole);
            _dbContext.SaveChanges();

            return StatusCode(200, new Web.SimpleErrorResponse { Success = true, Message = "Successfully added user" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while adding role to user" });
        }
    }


    [HttpDelete]
    [Route("removeRoleFromUser")]
    public ActionResult<Web.SimpleErrorResponse> RemoveRoleFromUser(int mainUserID, int userID, int companyID, int roleID)
    {
        try
        {
            
            // Step 1: Get all role IDs for the main user
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            // Step 2: Filter role IDs that belong to the company
            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            if (!hasCustomerAdminRole)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }

        
            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            
            var existingRole = _dbContext.CompanyRoles.Any(cr => cr.CompanyID == companyID && cr.RoleID == roleID);
            if (!existingRole)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Role does not exist in company" });
            }

           
            var userRole = _dbContext.UserRoles.FirstOrDefault(ur => ur.UserID == userID && ur.RoleID == roleID);
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
public ActionResult<Web.GetRolesInCompanyResponse> GetCompanyRoles(int companyID, int userID)
{
    try
    {
        var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
        if (!hasAccess)
        {
            return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have access" });
        }

        var roles = (from cr in _dbContext.CompanyRoles
                     join r in _dbContext.Roles on cr.RoleID equals r.RoleID
                     where cr.CompanyID == companyID
                     select new Web.Role(r.RoleID, r.Name)).ToList();

        var response = new Web.GetRolesInCompanyResponse
        {
            roles = roles
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while fetching roles." });
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
                // Step 1: Check if user has CustomerAdmin role for the company
                var userRoleIds = _dbContext.UserRoles
                    .Where(ur => ur.UserID == userID)
                    .Select(ur => ur.RoleID)
                    .ToList();

                var companyRoleIds = _dbContext.CompanyRoles
                    .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                    .Select(cr => cr.RoleID)
                    .ToList();

                var hasCustomerAdminRole = _dbContext.Roles
                    .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

                if (!hasCustomerAdminRole)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User does not have CustomerAdmin access."
                    });
                }

                // Step 2: Validate role is associated with the company
                var companyRole = _dbContext.CompanyRoles
                    .FirstOrDefault(cr => cr.CompanyID == companyID && cr.RoleID == roleID);

                if (companyRole == null)
                {
                    return StatusCode(400, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Role not assigned to the given company."
                    });
                }

                // Step 3: Delete all UserRole entries with this RoleID
                var userRolesToDelete = _dbContext.UserRoles
                    .Where(ur => ur.RoleID == roleID)
                    .ToList();

                _dbContext.UserRoles.RemoveRange(userRolesToDelete);

                // Step 4: Delete the CompanyRole entry
                _dbContext.CompanyRoles.Remove(companyRole);

                // Step 5: If role not used by any other company, delete it from Roles table
                bool roleUsedElsewhere = _dbContext.CompanyRoles
                    .Any(cr => cr.RoleID == roleID && cr.CompanyID != companyID);

                if (!roleUsedElsewhere)
                {
                    var role = _dbContext.Roles.Find(roleID);
                    if (role != null)
                    {
                        _dbContext.Roles.Remove(role);
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();

                return Ok("Role successfully deleted.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the role."
                });
            }
        }
    }


   [HttpPost]
   [Route("createRole")]
   public ActionResult<int> CreateRole(int userID, int companyID, string name)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                // Step 1: Get all role IDs for the main user
                var userRoleIds = _dbContext.UserRoles
                    .Where(ur => ur.UserID == userID)
                    .Select(ur => ur.RoleID)
                    .ToList();

                // Step 2: Filter role IDs that belong to the company
                var companyRoleIds = _dbContext.CompanyRoles
                    .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                    .Select(cr => cr.RoleID)
                    .ToList();

                // Step 3: Check if any of the filtered roles has the name "CustomerAdmin"
                var hasCustomerAdminRole = _dbContext.Roles
                    .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

                if (!hasCustomerAdminRole)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
                }
                // Check if a role with the same name already exists in the same company
                var existingRoleIdInCompany = (from r in _dbContext.Roles
                                            join cr in _dbContext.CompanyRoles on r.RoleID equals cr.RoleID
                                            where cr.CompanyID == companyID && r.Name == name
                                            select r.RoleID).FirstOrDefault();

                if (existingRoleIdInCompany != 0)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "A role with the same name already exists in this company."
                    });
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
                return role.RoleID;
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

   [HttpGet] 
   [Route("getCompany")]
    public ActionResult<Web.CompanyInfoResponse> GetCompany(int userID, int companyID)
    {
        try
        {
            var hasAccess = _dbContext.UserCompanies
                .Any(uc => uc.UserID == userID && uc.CompanyID == companyID);

            if (!hasAccess)
            {
                return StatusCode(403, new Web.SimpleErrorResponse 
                {
                    Success = false,
                    Message = "User does not have access to this company."
                });
            }

            var company = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (company == null)
            {
                return NotFound(new Web.SimpleErrorResponse 
                {
                    Success = false,
                    Message = "Company not found."
                });
            }

            var userCompanies = _dbContext.UserCompanies
                .Where(uc => uc.CompanyID == companyID)
                .ToList();

            var userIDs = userCompanies.Select(uc => uc.UserID).Distinct().ToList();

            var users = _dbContext.Users
                .Where(u => userIDs.Contains(u.UserID))
                .ToList();

            var emails = _dbContext.UserEmail
                .Where(e => userIDs.Contains(e.UserID))
                .GroupBy(e => e.UserID)
                .ToDictionary(g => g.Key, g => g.First().Email);

            // Get roles only associated with the company
            var companyRoleIDs = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID)
                .Select(cr => cr.RoleID)
                .ToList();

            var userRoles = _dbContext.UserRoles
                .Where(ur => userIDs.Contains(ur.UserID) && companyRoleIDs.Contains(ur.RoleID))
                .ToList();

            var roleIDs = userRoles.Select(ur => ur.RoleID).Distinct().ToList();

            var roles = _dbContext.Roles
                .Where(r => roleIDs.Contains(r.RoleID))
                .ToDictionary(r => r.RoleID, r => r.Name);

            var response = new Web.CompanyInfoResponse
            {
                companyName = company.CompanyName,
                users = users.Select(u => new Web.User
                {
                    userID = u.UserID,
                    userName = u.Name,
                    email = emails.ContainsKey(u.UserID) ? emails[u.UserID] : "",
                    roles = userRoles
                        .Where(ur => ur.UserID == u.UserID)
                        .Select(ur => new Web.RoleInfo
                        {
                            roleID = ur.RoleID,
                            roleName = roles.ContainsKey(ur.RoleID) ? roles[ur.RoleID] : "Unknown"
                        }).ToList()
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse 
            {
                Message = "An error occurred while fetching the company data."
            });
        }
    }


    [HttpDelete]
    [Route("deleteCompany")]
    public ActionResult DeleteCompany(int companyID)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                var company = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
                if (company == null)
                {
                    return NotFound(new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Company not found."
                    });
                }

                
                var userCompanies = _dbContext.UserCompanies
                    .Where(uc => uc.CompanyID == companyID)
                    .ToList();

                var userIDs = userCompanies.Select(uc => uc.UserID).ToList();

                
                var userRolesToRemove = _dbContext.UserRoles
                    .Where(ur => userIDs.Contains(ur.UserID))
                    .ToList();
                _dbContext.UserRoles.RemoveRange(userRolesToRemove);

                
                var endPoints = _dbContext.CompanyEndPoints
                    .Where(cep => cep.CompanyID == companyID)
                    .ToList();
                _dbContext.CompanyEndPoints.RemoveRange(endPoints);

                
                var roles = _dbContext.CompanyRoles
                    .Where(cr => cr.CompanyID == companyID)
                    .ToList();
                _dbContext.CompanyRoles.RemoveRange(roles);

                
                _dbContext.UserCompanies.RemoveRange(userCompanies);

               
                _dbContext.Companies.Remove(company);

                _dbContext.SaveChanges();
                transaction.Commit();

                return Ok("Company and all related user roles, endpoints, and references deleted successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("Error while deleting company: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse
                {
                    Message = "An error occurred while deleting the company."
                });
            }
        }
    }

    [HttpPost]
    [Route("createRoleEndPoint")]
    public ActionResult CreateRoleEndPoint(int userID, int companyID, int roleID, int endPointID)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                
                var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
                if (!hasAccess)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User does not have access"
                    });
                }

                
                var roleExists = _dbContext.Roles.Any(r => r.RoleID == roleID);
                if (!roleExists)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Role does not exist"
                    });
                }

                
                var endpointExists = _dbContext.EndPoints.Any(ep => ep.EndPointID == endPointID);
                if (!endpointExists)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Endpoint does not exist"
                    });
                }

                
                var alreadyExists = _dbContext.Set<RoleEndPoint>()
                    .Any(rep => rep.RoleID == roleID && rep.EndPointID == endPointID);

                if (alreadyExists)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "This Role-Endpoint combination already exists"
                    });
                }

                
                var roleEndPoint = new RoleEndPoint
                {
                    RoleID = roleID,
                    EndPointID = endPointID,
                    LastChange = DateTime.UtcNow
                };
                _dbContext.Set<RoleEndPoint>().Add(roleEndPoint);
                _dbContext.SaveChanges();

                transaction.Commit();
                return Ok("Role-Endpoint successfully created.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse
                {
                    Message = "An error occurred while creating the Role-Endpoint mapping."
                });
            }
        }
    }
    [HttpPost]
[Route("makeAdmin")]
public ActionResult<Web.SimpleErrorResponse> MakeAdmin(int mainUserID, int userID, int companyID)
{
    try
    {
        // Step 1: Check if mainUserID has CustomerAdmin role in the company
        var mainUserRoleIds = _dbContext.UserRoles
            .Where(ur => ur.UserID == mainUserID)
            .Select(ur => ur.RoleID)
            .ToList();

        var mainUserCompanyRoleIds = _dbContext.CompanyRoles
            .Where(cr => cr.CompanyID == companyID && mainUserRoleIds.Contains(cr.RoleID))
            .Select(cr => cr.RoleID)
            .ToList();

        var hasCustomerAdminAccess = _dbContext.Roles
            .Any(r => mainUserCompanyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

        if (!hasCustomerAdminAccess)
        {
            return StatusCode(403, new Web.SimpleErrorResponse 
            { 
                Success = false, 
                Message = "Main user does not have CustomerAdmin access." 
            });
        }

        // Step 2: Get the roleID for CustomerAdmin for this company
        var customerAdminRoleID = (from cr in _dbContext.CompanyRoles
                                   join r in _dbContext.Roles on cr.RoleID equals r.RoleID
                                   where cr.CompanyID == companyID && r.Name == "CustomerAdmin"
                                   select cr.RoleID).FirstOrDefault();

        if (customerAdminRoleID == 0)
        {
            return StatusCode(500, new Web.SimpleErrorResponse 
            { 
                Success = false, 
                Message = "CustomerAdmin role does not exist in the company." 
            });
        }

        // Step 3: Check if the user is in the company
        var isUserInCompany = _dbContext.UserCompanies
            .Any(uc => uc.CompanyID == companyID && uc.UserID == userID);

        if (!isUserInCompany)
        {
            return StatusCode(400, new Web.SimpleErrorResponse 
            { 
                Success = false, 
                Message = "User is not part of the company." 
            });
        }

        // Step 4: Check if the user already has the CustomerAdmin role
        bool alreadyHasRole = _dbContext.UserRoles
            .Any(ur => ur.UserID == userID && ur.RoleID == customerAdminRoleID);

        if (alreadyHasRole)
        {
            return StatusCode(400, new Web.SimpleErrorResponse 
            { 
                Success = false, 
                Message = "User already has the CustomerAdmin role." 
            });
        }

        // Step 5: Assign the role
        var newUserRole = new Database.MixedTables.UserRole
        {
            UserID = userID,
            RoleID = customerAdminRoleID,
            LastChange = DateTime.UtcNow
        };

        _dbContext.UserRoles.Add(newUserRole);
        _dbContext.SaveChanges();

        return Ok(new Web.SimpleErrorResponse 
        { 
            Success = true, 
            Message = "User has been made an admin successfully." 
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: {0}", ex.Message);
        return StatusCode(500, new Web.SimpleErrorResponse 
        { 
            Success = false, 
            Message = "An error occurred while assigning admin role." 
        });
    }
}


}

    
