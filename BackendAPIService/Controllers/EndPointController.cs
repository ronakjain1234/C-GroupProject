using DatabaseHandler.Data.Models.Web.ResponseObjects;
using DatabaseHandler;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
using DatabaseHandler.Data.Models.Database;
using System.Security.Claims;
namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/endpoint/")]
public class EndPointController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public EndPointController(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    
    [HttpPost]
    [Route("createEndpoint")]
    public ActionResult CreateEndpoint([FromBody] EndpointResponse body)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                string name = body.Name;
                string spec = body.Spec;
                
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                if (!isSystemAdmin)
                {
                    return Unauthorized(new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User is not admin"
                    });
                }
                var newEndpoint = new EndPoint
                    {
                        EndPointName = name,
                        Specification = spec,
                        LastChange = DateTime.UtcNow
                    };
                _dbContext.EndPoints.Add(newEndpoint);
                 _dbContext.SaveChanges();
                transaction.Commit();
                return Ok("Endpoint successfully created");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse { Message = "An error occurred while creating the endpoint." });
            }
        }
    }
    [HttpPost]
    [Route("updateEndpoint")]
    public ActionResult UpdateEndpoint([FromBody] EndpointResponse body)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                string name = body.Name;
                string spec = body.Spec;
                int endpointID = body.endpointID;
                
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                if (!isSystemAdmin)
                {
                    return Unauthorized(new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User is not admin."
                    });
                }
                var newEndpoint = new EndPoint
                {
                    EndPointID = endpointID,
                    EndPointName = name,
                    Specification = spec,
                    LastChange = DateTime.UtcNow
                };
                var item = _dbContext.EndPoints.Where(e => e.EndPointID == endpointID).FirstOrDefault();
                item.EndPointName = newEndpoint.EndPointName;
                item.Specification = spec;
                _dbContext.EndPoints.Update(item);
                _dbContext.SaveChanges();
                transaction.Commit();
                return Ok("Endpoint successfully Updated");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse { Message = "An error occurred while creating the endpoint." });
            }
        }
    }

    [HttpPost]
    [Route("addEndpointToCompany")]
    public ActionResult AddEndpointToCompany(int companyID, int endpointID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                var userCompany = _dbContext.UserCompanies
                    .FirstOrDefault(uc => uc.CompanyID == companyID && uc.UserID == userID);
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

                if (userCompany == null && !isSystemAdmin)
                {
                    return StatusCode(403, new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "User does not have access to the company or access has expired." 
                    });
                }

                var endpoint = _dbContext.EndPoints.FirstOrDefault(e => e.EndPointID == endpointID);
                if (endpoint == null)
                {
                    return NotFound(new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "Endpoint does not exist." 
                    });
                }

                bool alreadyAssigned = _dbContext.CompanyEndPoints
                    .Any(ce => ce.CompanyID == companyID && ce.EndPointID == endpointID);

                if (alreadyAssigned)
                {
                    return Conflict(new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "Endpoint is already assigned to the company." 
                    });
                }

                var companyEndPoint = new CompanyEndPoint
                {
                    CompanyID = companyID,
                    EndPointID = endpointID,
                    LastChange = DateTime.UtcNow
                };
                _dbContext.CompanyEndPoints.Add(companyEndPoint);
                _dbContext.SaveChanges();

                transaction.Commit();
                return Ok("Endpoint successfully assigned to company.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse 
                { 
                    Message = "An error occurred while assigning the endpoint to the company." 
                });
            }
        }
    }

    [HttpDelete]
    [Route("removeEndpointFromCompany")]
    public ActionResult RemoveEndpointFromCompany(int companyID, int endpointID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                var userCompany = _dbContext.UserCompanies
                    .FirstOrDefault(uc => uc.CompanyID == companyID && uc.UserID == userID);
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                if (userCompany == null && !isSystemAdmin)
                {
                    return StatusCode(403, new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "User does not have access to the company or access has expired." 
                    });
                }

                var endpoint = _dbContext.EndPoints.FirstOrDefault(e => e.EndPointID == endpointID);
                if (endpoint == null)
                {
                    return NotFound(new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "Endpoint does not exist." 
                    });
                }

                var companyEndpoint = _dbContext.CompanyEndPoints
                    .FirstOrDefault(ce => ce.CompanyID == companyID && ce.EndPointID == endpointID);

                if (companyEndpoint == null)
                {
                    return NotFound(new SimpleErrorResponse 
                    { 
                        Success = false, 
                        Message = "This endpoint is not assigned to the company." 
                    });
                }

                _dbContext.CompanyEndPoints.Remove(companyEndpoint);
                _dbContext.SaveChanges();

                transaction.Commit();
                return Ok("Endpoint successfully removed from the company.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse 
                { 
                    Message = "An error occurred while removing the endpoint from the company." 
                });
            }
        }
    }
    
    [HttpGet]
    [Route("getEndpointsForRole")]
    public ActionResult<List<int>> GetEndpointsForRole(int roleID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {
            var list = _dbContext.RoleEndPoints.Where(e => e.RoleID == roleID).Select(e => e.EndPointID).ToList();

            return Ok(list);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse
            {
                Message = "An error occurred while retrieving endpoints for the role."
            });
        }
    }

    [HttpGet]
    [Route("getCompanyEndpoints")]
    public ActionResult<List<EndpointResponse>> GetCompanyEndpoints(int companyID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        try
        {
            
            var hasAccess = _dbContext.UserCompanies
                .Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
            
            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);

            if (!hasAccess && !isSystemAdmin)
            {
                return StatusCode(403, new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User does not have access to this company."
                });
            }

            
            var endpointIds = _dbContext.CompanyEndPoints
                .Where(ce => ce.CompanyID == companyID)
                .Select(ce => ce.EndPointID)
                .ToList();

            if (!endpointIds.Any())
            {
                return Ok(new List<EndpointResponse>()); 
            }

            
            var endpoints = _dbContext.EndPoints
                .Where(e => endpointIds.Contains(e.EndPointID))
                .Select(e => new EndpointResponse
                {
                    endpointID = e.EndPointID,
                    Name = e.EndPointName,
                    Spec = e.Specification
                })
                .ToList();

            return Ok(endpoints);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
            return StatusCode(500, new SimpleErrorResponse
            {
                Message = "An error occurred while retrieving endpoints for the company."
            });
        }
    }


    [HttpDelete]
    [Route("deleteEndpoint")]
    public ActionResult DeleteEndpoint(int endpointID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
                if (!isSystemAdmin)
                {
                    return StatusCode(500, new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "User is not admin."
                    });
                }
                
                var endpoint = _dbContext.EndPoints.FirstOrDefault(e => e.EndPointID == endpointID);
                if (endpoint == null)
                {
                    return NotFound(new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Endpoint not found."
                    });
                }

                
                var companyEndPoints = _dbContext.CompanyEndPoints
                    .Where(ce => ce.EndPointID == endpointID)
                    .ToList();
                _dbContext.CompanyEndPoints.RemoveRange(companyEndPoints);

                
                var moduleEndPoints = _dbContext.ModuleEndPoints
                    .Where(me => me.EndPointID == endpointID)
                    .ToList();
                _dbContext.ModuleEndPoints.RemoveRange(moduleEndPoints);

                
                var roleEndPoints = _dbContext.RoleEndPoints
                    .Where(re => re.EndPointID == endpointID)
                    .ToList();
                _dbContext.RoleEndPoints.RemoveRange(roleEndPoints);

                
                _dbContext.EndPoints.Remove(endpoint);

                
                _dbContext.SaveChanges();

               
                transaction.Commit();

                return Ok("Endpoint and all associated references successfully deleted.");
            }
            catch (Exception ex)
            {
                // Rollback the transaction if any error occurs
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse
                {
                    Message = "An error occurred while deleting the endpoint and its references."
                });
            }
        }
    }
    [HttpGet]
    [Route("getAll")]
    public ActionResult<List<EndpointResponse>> GetEndpoints()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }
        bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
        if (isSystemAdmin)
        { 
            List<EndpointResponse> endpoints = _dbContext.EndPoints
                .Select(e => new EndpointResponse
                {
                    endpointID = e.EndPointID,
                    Name = e.EndPointName,
                    Spec = e.Specification
                })
                .ToList();
            return endpoints;
        }

        List<int> roleIDs = _dbContext.UserRoles.Where(role => role.UserID == userID).Select(e => e.RoleID).ToList();
        List<int> endpointIds = new();
        foreach (var roleID in roleIDs)
        {
            var list = _dbContext.RoleEndPoints.Where(roleEndpoint => roleEndpoint.RoleID == roleID).Select(e => e.EndPointID)
                .ToList();
            endpointIds.AddRange(list);
        }

        List<EndpointResponse> endpointts = new();
        foreach (var endpointID in endpointIds)
        {
            List<EndpointResponse> list = _dbContext.EndPoints.Where(endpoint => endpoint.EndPointID == endpointID)
                .Select(e => new EndpointResponse
                {
                    endpointID = e.EndPointID,
                    Name = e.EndPointName,
                    Spec = e.Specification
                })
                .ToList();
            endpointts.AddRange(list);
        }
        
        return endpointts;
    }

    [HttpGet]
    [Route("getSpec")]
    public ActionResult<String> getSpec(int endpointID)
    {
        List<EndpointResponse> list = _dbContext.EndPoints.Where(endpoint => endpoint.EndPointID == endpointID)
            .Select(e => new EndpointResponse
            {
                endpointID = e.EndPointID,
                Name = e.EndPointName,
                Spec = e.Specification
            })
            .ToList();

        return list.First().Spec;
    }

    [HttpGet]
    [Route("getCompanies")]
    public ActionResult<List<int>> getCompanies(int endpointID)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "Invalid or missing authentication token."
            });
        }

        bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
        if (!isSystemAdmin)
        {
            return Unauthorized(new SimpleErrorResponse
            {
                Success = false,
                Message = "User is not admin"
            });
        }

        List<int> companies = _dbContext.CompanyEndPoints.Where(companyEndpoint => companyEndpoint.EndPointID == endpointID)
            .Select(companyEndpoint => companyEndpoint.CompanyID).ToList();
        return companies;
    }
    
    [HttpPost]
    [Route("setCompanyState")]
    public ActionResult setCompanyState([FromBody]List<localCompany> localCompanies, [FromQuery] int endpointID)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }

            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
            if (!isSystemAdmin)
            {
                return Unauthorized(new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User is not admin"
                });
            }

            foreach (var company in localCompanies)
            {
                var foundObjects = _dbContext.CompanyEndPoints.Where(e =>
                    e.CompanyID == company.company.CompanyID && e.EndPointID == endpointID).ToList();
                bool alreadyExists = false;
                foreach (var companyEndpoint in foundObjects)
                {
                    if (companyEndpoint.CompanyID == company.company.CompanyID && companyEndpoint.EndPointID == endpointID)
                    {
                        alreadyExists = true;
                    }
                }
                if (!company.isSelected)
                {
                    if (alreadyExists)
                    {
                        var valueInDB = _dbContext.CompanyEndPoints.Where(e =>
                            e.CompanyID == company.company.CompanyID && e.EndPointID == endpointID).ToList();
                        _dbContext.CompanyEndPoints.Remove(valueInDB[0]);
                    }
                }
                if (company.isSelected)
                {
                    if (!alreadyExists)
                    {
                        _dbContext.CompanyEndPoints.Add(new CompanyEndPoint()
                        {
                            CompanyID = company.company.CompanyID,
                            EndPointID = endpointID,
                            LastChange = DateTime.Now
                        });
                    }
                }
            }

            _dbContext.SaveChanges();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("setRoleState")]
    public ActionResult setCompanyState([FromBody] List<LocalEndpoint> localCompanies, [FromQuery] int roleID)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }
            var companyID = _dbContext.CompanyRoles.Where(e=> e.CompanyID == roleID).Select(e => e.CompanyID).FirstOrDefault();

            var companyRoleIDs = _dbContext.CompanyRoles.Where(e => companyID == e.CompanyID).Select(e => e.RoleID).ToList();

            companyRoleIDs.Sort();
            bool isCustomerAdmin = _dbContext.UserRoles.Any(e => e.UserID == userID && e.RoleID == companyRoleIDs[0]);
            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
            if (!isSystemAdmin && !isCustomerAdmin)
            {
                return StatusCode(403, new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User is not admin"
                });
            }
            foreach (var company in localCompanies)
            {
                var foundObjects = _dbContext.RoleEndPoints.Where(e =>
                    e.RoleID == roleID && e.EndPointID == company.endpointID).ToList();
                bool alreadyExists = false;
                foreach (var companyEndpoint in foundObjects)
                {
                    if (companyEndpoint.EndPointID == company.endpointID && companyEndpoint.RoleID == roleID)
                    {
                        alreadyExists = true;
                    }
                }
                if (!company.isSelected)
                {
                    if (alreadyExists)
                    {
                        var valueInDB = _dbContext.RoleEndPoints.Where(e =>
                            e.EndPointID == company.endpointID && e.RoleID == roleID).ToList();
                        _dbContext.RoleEndPoints.Remove(valueInDB[0]);
                    }
                }
                if (company.isSelected)
                {
                    if (!alreadyExists)
                    {
                        _dbContext.RoleEndPoints.Add(new RoleEndPoint()
                        {
                            RoleID = roleID,
                            EndPointID = company.endpointID,
                            LastChange = DateTime.Now
                        });
                    }
                }
            }

            _dbContext.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet]
    [Route("getAllCompanyEndpoints")]
    public ActionResult<List<LocalEndpoint>> getAllCompanyEndpoints(int companyID)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized(new SimpleErrorResponse
                {
                    Success = false,
                    Message = "Invalid or missing authentication token."
                });
            }
            
            var companyRoleIDs = _dbContext.CompanyRoles.Where(e => companyID == e.CompanyID).Select(e => e.RoleID).ToList();

            companyRoleIDs.Sort();
            bool isCustomerAdmin = _dbContext.UserRoles.Any(e => e.UserID == userID && e.RoleID == companyRoleIDs[0]);
            bool isSystemAdmin = Utility.IsUserInCompany(userID, 1, _dbContext);
            if (!isSystemAdmin && !isCustomerAdmin)
            {
                return StatusCode(403, new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User is not admin"
                });
            }

            var list = _dbContext.CompanyEndPoints.Where(e => e.CompanyID == companyID).Select(e => new LocalEndpoint
            {
                endpointID = e.EndPointID,
                Name = _dbContext.EndPoints.Where(innerE=>innerE.EndPointID==e.EndPointID).Select(e=>e.EndPointName).FirstOrDefault(),
                Spec = ""
            }).ToList();
            
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
        
    }
    
}   