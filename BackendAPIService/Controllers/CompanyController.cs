using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Web = DatabaseHandler.Data.Models.Web.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
using DatabaseHandler.Data.Models.Database;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
    public ActionResult<List<Web.GetAllCompaniesResponse>> GetCompanies(int limit = 50, int offset = 0, string? searchString = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }


            var companyIds = _dbContext.UserCompanies
                .Where(uc => uc.UserID == userID)
                .Select(uc => uc.CompanyID)
                .Distinct()
                .ToList();

            if (!companyIds.Any())
            {
                return Ok(new List<Web.GetAllCompaniesResponse>()); // No companies
            }
            
            var companiesQuery = _dbContext.Companies
                .Where(c => companyIds.Contains(c.CompanyID));
            
            if (Utility.IsUserInCompany(userID, 1, _dbContext))
            {
                companiesQuery = _dbContext.Companies;
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                companiesQuery = companiesQuery
                    .Where(c => c.CompanyName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }


            var result = companiesQuery
                .OrderBy(c => c.CompanyName)
                .Skip(offset)
                .Take(limit)
                .Select(c => new Web.GetAllCompaniesResponse
                {
                    companyID = c.CompanyID,
                    companyName = c.CompanyName
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse
            {
                Message = "An error occurred while fetching companies.",
                Success = false
            });
        }
    }

    [HttpGet]
    [Route("getAllForEndpointPage")]
    public ActionResult<List<Company>> GetAllCompanies()
    {
        try
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }

            if (Utility.IsUserInCompany(userID, 1, _dbContext))
            {
                var result = _dbContext.Companies.ToList();
                return Ok(result);
            }
            return StatusCode(401, new Web.SimpleErrorResponse()
            {
                Success = false,
                Message = "User is not admin"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse
            {
                Message = "An error occurred while fetching companies.",
                Success = false
            });
        }
    }


    public class CreateCompanyRequest
    {
        public required string companyName { get; set; }
    }

    [HttpPost]
    [Route("createCompany")]
    public ActionResult<Web.SimpleErrorResponse> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        string companyName = request.companyName;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest(new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "Company name cannot be empty."
                });
            }

            bool companyExists = _dbContext.Companies
                .Any(c => c.CompanyName.ToLower() == companyName.ToLower());

            if (companyExists)
            {
                return BadRequest(new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "A company with this name already exists."
                });
            }

            var newCompany = new Database.Company
            {
                CompanyName = companyName,
                LastChange = DateTime.UtcNow
            };
            _dbContext.Companies.Add(newCompany);
            _dbContext.SaveChanges();

            var userCompany = new UserCompany
            {
                CompanyID = newCompany.CompanyID,
                UserID = userID,
                LastChange = DateTime.UtcNow
            };
            _dbContext.UserCompanies.Add(userCompany);

            var adminRole = new Database.Role
            {
                Name = "CustomerAdmin",
                LastChange = DateTime.UtcNow
            };
            _dbContext.Roles.Add(adminRole);
            _dbContext.SaveChanges();

            var companyRole = new CompanyRole
            {
                CompanyID = newCompany.CompanyID,
                RoleID = adminRole.RoleID,
                LastChange = DateTime.UtcNow
            };
            _dbContext.CompanyRoles.Add(companyRole);
            _dbContext.SaveChanges();

            var userRole = new UserRole
            {
                UserID = userID,
                RoleID = adminRole.RoleID,
                LastChange = DateTime.UtcNow
            };
            _dbContext.UserRoles.Add(userRole);
            _dbContext.SaveChanges();

            transaction.Commit();

            return Ok(new Web.SimpleErrorResponse
            {
                Success = true,
                Message = "Company created successfully."
            });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "An error occurred while creating the company."
            });
        }
    }


    [HttpPut]
    [Route("changeCompanyName")]
    public ActionResult<Web.SimpleErrorResponse> ChangeCompanyName(int companyID, string newCompanyName)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {
            bool companyNameExists = _dbContext.Companies.Any(c => c.CompanyName.ToLower() == newCompanyName.ToLower());

            if (companyNameExists)
            {
                return StatusCode(404, new Web.SimpleErrorResponse()
                {
                    Success = false,
                    Message = "Company name already exists."
                });
            }

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

            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
            if (!hasCustomerAdminRole && !isSystemAdmin)
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


        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "An error occurred while editing the company." });
        }
    }

    [HttpPost]
    [Route("addUser")]
    public ActionResult<Web.SimpleErrorResponse> AddUser(string email, int companyID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int mainUserID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();


            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            bool isSystemAdmin = Utility.IsUserInCompany(mainUserID, 1, _dbContext);
            
            if (!hasCustomerAdminRole && !isSystemAdmin)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }


            var userEmailEntry = _dbContext.UserEmail.FirstOrDefault(ue => ue.Email == email);
            if (userEmailEntry == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "User with this email not found." });
            }

            int userId = userEmailEntry.UserID;

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }


            bool userExists = _dbContext.UserCompanies.Any(uc => uc.UserID == userId && uc.CompanyID == companyID);
            if (userExists)
            {
                return StatusCode(409, new Web.SimpleErrorResponse { Success = false, Message = "User is already associated with this company." });
            }


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
    public ActionResult<Web.SimpleErrorResponse> RemoveUser(int userID, int companyID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int mainUserID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        if (mainUserID == userID)
        {
            return BadRequest(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "You cannot remove yourself from the company."
            });
        }

        try
        {

            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == userID)
                .Select(ur => ur.RoleID)
                .ToList();
            foreach (var roleId in userRoleIds)
            {
                var result = _dbContext.UserRoles.Where(e => e.RoleID == roleId && e.UserID == userID).ToList();
                foreach (var userRole in result)
                {
                    _dbContext.UserRoles.Remove(userRole);
                }
            }
            var mainUserRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();


            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && mainUserRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();


            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");
            
            bool isSystemAdmin = Utility.IsUserInCompany(mainUserID, 1, _dbContext);

            if (!hasCustomerAdminRole && !isSystemAdmin)
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
    public ActionResult<Web.SimpleErrorResponse> AddRoleToUser(int userID, int companyID, int roleID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int mainUserID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {

            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");
            
            bool isSystemAdmin = Utility.IsUserInCompany(mainUserID, 1, _dbContext);

            if (!hasCustomerAdminRole && !isSystemAdmin)
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
    public ActionResult<Web.SimpleErrorResponse> RemoveRoleFromUser(int userID, int companyID, int roleID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int mainUserID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        try
        {
            var userRoleIds = _dbContext.UserRoles
                .Where(ur => ur.UserID == mainUserID)
                .Select(ur => ur.RoleID)
                .ToList();

            var companyRoleIds = _dbContext.CompanyRoles
                .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                .Select(cr => cr.RoleID)
                .ToList();

            var hasCustomerAdminRole = _dbContext.Roles
                .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");

            bool isSystemAdmin = Utility.IsUserInCompany(mainUserID, 1, _dbContext);
            
            if (!hasCustomerAdminRole && !isSystemAdmin)
            {
                return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
            }

            var existingCompany = _dbContext.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (existingCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Company not found." });
            }

            var existingRoleInCompany = _dbContext.CompanyRoles.FirstOrDefault(cr => cr.CompanyID == companyID && cr.RoleID == roleID);
            if (existingRoleInCompany == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Role does not exist in company" });
            }

            var role = _dbContext.Roles.FirstOrDefault(r => r.RoleID == roleID);
            if (role == null)
            {
                return StatusCode(404, new Web.SimpleErrorResponse { Success = false, Message = "Role not found." });
            }

            // Block self-removal of CustomerAdmin role
            if (userID == mainUserID && role.Name == "CustomerAdmin")
            {
                return StatusCode(403, new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "You cannot remove the CustomerAdmin role from yourself."
                });
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
    public ActionResult<Web.GetRolesInCompanyResponse> GetCompanyRoles(int companyID, bool showCustomerAdminRole)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        try
        {
            var hasAccess = _dbContext.UserCompanies
                .Any(uc => uc.UserID == userID && uc.CompanyID == companyID);

            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
            
            if (!hasAccess && !isSystemAdmin)
            {
                return StatusCode(403, new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "User does not have access to this company."
                });
            }

            var rolesQuery = from cr in _dbContext.CompanyRoles
                             join r in _dbContext.Roles on cr.RoleID equals r.RoleID
                             where cr.CompanyID == companyID
                             select new { r.RoleID, r.Name };


            if (!showCustomerAdminRole)
            {
                rolesQuery = rolesQuery.Where(role => role.Name != "CustomerAdmin");
            }

            var roles = rolesQuery
                .Select(role => new Web.Role(role.RoleID, role.Name))
                .ToList();

            var response = new Web.GetRolesInCompanyResponse
            {
                roles = roles
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new Web.SimpleErrorResponse
            {
                Message = "An error occurred while fetching roles."
            });
        }
    }




    [HttpDelete]
    [Route("deleteRole")]
    public ActionResult DeleteRole(int companyID, int roleID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {

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

                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                
                if (!hasCustomerAdminRole && !isSystemAdmin)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User does not have CustomerAdmin access."
                    });
                }


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


                var userRolesToDelete = _dbContext.UserRoles
                    .Where(ur => ur.RoleID == roleID)
                    .ToList();

                _dbContext.UserRoles.RemoveRange(userRolesToDelete);


                _dbContext.CompanyRoles.Remove(companyRole);

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
    public ActionResult<int> CreateRole(int companyID, string name)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
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
                
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

                if (!hasCustomerAdminRole && !isSystemAdmin)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
                }

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
                    return StatusCode(500, new Web.SimpleErrorResponse { Success = false, Message = "Role exists in company" });
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
                return StatusCode(500, new Web.SimpleErrorResponse { Message = "An error occurred while creating roles." });
            }
        }
    }

    [HttpPut]
    [Route("updateRoles")]
    public ActionResult UpdateRoleName(int roleID, int companyID, string name)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                // Validate that this role actually belongs to the company
                bool roleBelongsToCompany = _dbContext.CompanyRoles
                    .Any(cr => cr.CompanyID == companyID && cr.RoleID == roleID);

                if (!roleBelongsToCompany)
                {
                    return BadRequest(new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Role does not belong to specified company."
                    });
                }

                // Get user's roles
                var userRoleIds = _dbContext.UserRoles
                    .Where(ur => ur.UserID == userID)
                    .Select(ur => ur.RoleID)
                    .ToList();

                // Get roles within the company the user has
                var companyRoleIds = _dbContext.CompanyRoles
                    .Where(cr => cr.CompanyID == companyID && userRoleIds.Contains(cr.RoleID))
                    .Select(cr => cr.RoleID)
                    .ToList();

                var hasCustomerAdminRole = _dbContext.Roles
                    .Any(r => companyRoleIds.Contains(r.RoleID) && r.Name == "CustomerAdmin");
                
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

                if (!hasCustomerAdminRole && !isSystemAdmin)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User does not have CustomerAdmin access."
                    });
                }

                var role = _dbContext.Roles.Find(roleID);
                if (role == null)
                {
                    return StatusCode(500, new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Role does not exist."
                    });
                }

                if (string.Equals(name.Trim(), "CustomerAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Cannot rename role to 'CustomerAdmin'."
                    });
                }

                // Check for duplicate name within the same company
                bool nameExists = (from cr in _dbContext.CompanyRoles
                                   join r in _dbContext.Roles on cr.RoleID equals r.RoleID
                                   where cr.CompanyID == companyID &&
                                           r.RoleID != roleID &&
                                           r.Name.ToLower() == name.Trim().ToLower()
                                   select r).Any();

                if (nameExists)
                {
                    return BadRequest(new Web.SimpleErrorResponse
                    {
                        Success = false,
                        Message = "A role with this name already exists in the same company."
                    });
                }

                // Proceed with update
                role.Name = name.Trim();
                role.LastChange = DateTime.UtcNow;

                _dbContext.SaveChanges();
                transaction.Commit();

                return Ok("Role name updated successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while updating roles."
                });
            }
        }
    }




    [HttpGet]
    [Route("getCompany")]
    public ActionResult<Web.CompanyInfoResponse> GetCompany(int companyID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {
            var hasAccess = _dbContext.UserCompanies
                .Any(uc => uc.UserID == userID && uc.CompanyID == companyID);
            
            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

            if (!hasAccess && !isSystemAdmin)
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
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
                
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

                if (!hasCustomerAdminRole && !isSystemAdmin)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
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
    public ActionResult CreateRoleEndPoint(int companyID, int roleID, int endPointID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
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

                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                
                if (!hasCustomerAdminRole && !isSystemAdmin)
                {
                    return StatusCode(403, new Web.SimpleErrorResponse { Success = false, Message = "User does not have CustomerAdmin access." });
                }

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
    public ActionResult<Web.SimpleErrorResponse> MakeAdmin(int userID, int companyID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int mainUserID))
        {
            return Unauthorized(new Web.SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {

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
            
            bool isSystemAdmin = Utility.IsUserInCompany(mainUserID, 1, _dbContext);

            if (!hasCustomerAdminAccess && !isSystemAdmin)
            {
                return StatusCode(403, new Web.SimpleErrorResponse
                {
                    Success = false,
                    Message = "Main user does not have CustomerAdmin access."
                });
            }


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

    [HttpGet]
    [Route("getInitials")]
    public ActionResult<string> GetUserInitials()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized("Invalid or missing authentication token.");
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.UserID == userID);

            if (user == null || string.IsNullOrWhiteSpace(user.Name))
            {
                return NotFound("User not found or name is missing.");
            }

            var nameParts = user.Name
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length == 0)
            {
                return BadRequest("Name format is invalid.");
            }

            var initials = string.Concat(nameParts.Select(part => char.ToUpper(part[0])));

            return Ok(initials);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching initials: " + ex.Message);
            return StatusCode(500, "An error occurred while fetching user initials.");
        }
    }
    
}

    
