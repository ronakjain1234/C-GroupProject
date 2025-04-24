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
    //Return Company Id
    public ActionResult<List<Web.GetAllCompaniesResponse>> GetCompanies(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        try
        {
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
                        name = company.CompanyName
                    });
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                companyList = companyList
                    .Where(c => c.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
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
            return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while fetching companies." });
        }
    }


    [HttpPost]
    [Route("createCompany")]
    public ActionResult<Web.SimpleErrorResponse> CreateCompany(int userID, string Name)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        try
        {
            if (string.IsNullOrEmpty(Name))
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            // Check for duplicate company name (case-insensitive)
            bool companyExists = _dbContext.Companies.Any(c => c.CompanyName.ToLower() == Name.ToLower());
            if (companyExists)
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "A company with this name already exists." });
            }

            var newCompany = new Database.Company { CompanyName = Name };
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

    public ActionResult<Web.SimpleErrorResponse> ChangeCompanyName (int userID, int companyID, string companyName)
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

            if (string.IsNullOrEmpty(companyName))
            {
                return StatusCode(400, new Web.SimpleErrorResponse { Success = false, Message = "Company name cannot be empty." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
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

            var userRoles = _dbContext.UserRoles
                .Where(ur => userIDs.Contains(ur.UserID))
                .ToList();

            var roleIDs = userRoles.Select(ur => ur.RoleID).Distinct().ToList();

            var roles = _dbContext.Roles
                .Where(r => roleIDs.Contains(r.RoleID))
                .ToDictionary(r => r.RoleID, r => r.Name);

            
            var response = new Web.CompanyInfoResponse
            {
                name = company.CompanyName,
                users = users.Select(u => new Web.User
            {
                userID = u.UserID,
                Name = u.Name,
                Email = emails.ContainsKey(u.UserID) ? emails[u.UserID] : "",
                Roles = userRoles
                    .Where(ur => ur.UserID == u.UserID)
                    .Select(ur => new Web.RoleInfo
                    {
                        RoleID = ur.RoleID,
                        RoleName = roles.ContainsKey(ur.RoleID) ? roles[ur.RoleID] : "Unknown"
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

}

    
