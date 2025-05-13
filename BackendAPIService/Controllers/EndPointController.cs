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
                
                //bool allowed = _dbContext.UserRoles.Any(userRole => userRole.RoleID == 1 && userRole.UserID == userID);
                bool allowed = true;
                if (!allowed)
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
                
                //bool allowed = _dbContext.UserRoles.Any(userRole => userRole.RoleID == 1 && userRole.UserID == userID);
                bool allowed = true;
                if (!allowed)
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
                _dbContext.EndPoints.Update(newEndpoint);
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

                if (userCompany == null)
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

                if (userCompany == null)
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
    [Route("get")]
    public ActionResult<List<EndpointResponse>> GetAllEndpoints()
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
    
    [HttpGet]
    [Route("getEndpointsForRole")]
    public ActionResult<List<EndpointResponse>> GetEndpointsForRole(int roleID)
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
            
            var userCompanies = _dbContext.UserCompanies
                .Where(uc => uc.UserID == userID)
                .Select(uc => uc.CompanyID)
                .ToList();

            if (!userCompanies.Any())
            {
                return StatusCode(403, new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User does not belong to any companies."
                });
            }

            
            var roleAccessible = _dbContext.CompanyRoles
                .Any(cr => userCompanies.Contains(cr.CompanyID) && cr.RoleID == roleID);

            if (!roleAccessible)
            {
                return StatusCode(403, new SimpleErrorResponse
                {
                    Success = false,
                    Message = "User does not have access to this role."
                });
            }

            
            var endpointIDs = _dbContext.Set<RoleEndPoint>()
                .Where(re => re.RoleID == roleID)
                .Select(re => re.EndPointID)
                .ToList();

            var endpointList = _dbContext.EndPoints
                .Where(e => endpointIDs.Contains(e.EndPointID))
                .Select(e => new EndpointResponse
                {
                    endpointID = e.EndPointID,
                    Name = e.EndPointName,
                    Spec = e.Specification
                })
                .ToList();

            return Ok(endpointList);
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

            if (!hasAccess)
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

        if (_dbContext.UserRoles.Any(userRole => userRole.RoleID == 1 && userRole.UserID == userID))
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

        if (userID != 1)
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
    
}   