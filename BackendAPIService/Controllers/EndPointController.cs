using DatabaseHandler.Data.Models.Web.ResponseObjects;
using DatabaseHandler;
using Database = DatabaseHandler.Data.Models.Database;
using Microsoft.AspNetCore.Mvc;
using DatabaseHandler.Data.Models.Database.MixedTables;
using DatabaseHandler.Data.Models.Database;
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
    public ActionResult CreateEndpoint(int userID,  string endPointPath, int companyID, int? moduleID = null)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                var hasAccess = _dbContext.UserCompanies.Any(uc => uc.CompanyID == companyID && uc.UserID == userID);
                if (!hasAccess)
                {
                    return StatusCode(403, new SimpleErrorResponse { Success = false, Message = "User does not have access" });
                }


                var newEndpoint = new EndPoint
                    {   
                        Path = endPointPath,
                        LastChange = DateTime.UtcNow
                    };
                _dbContext.EndPoints.Add(newEndpoint);
                 _dbContext.SaveChanges();
                transaction.Commit();
                return Ok("Endpoint successfully created and assigned to company.");
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
    public ActionResult AddEndpointToCompany(int userID, int companyID, int endpointID)
    {
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
    public ActionResult RemoveEndpointFromCompany(int userID, int companyID, int endpointID)
    {
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
    [Route("getEndpointsForRole")]
    public ActionResult<List<EndpointResponse>> GetEndpointsForRole(int userID, int roleID)
        {
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
                        Path = e.Path
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
    public ActionResult<List<EndpointResponse>> GetCompanyEndpoints(int userID, int companyID)
    {
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
                    Path = e.Path
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

    [HttpPost]
    [Route("createModule")]
    public ActionResult CreateModule(string moduleName)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                
                var newModule = new Module
                {
                    Name = moduleName,
                    LastChange = DateTime.UtcNow
                };

                
                _dbContext.Modules.Add(newModule);
                _dbContext.SaveChanges();

                
                transaction.Commit();

                return Ok("Module successfully created.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse
                {
                    Message = "An error occurred while creating the module."
                });
            }
        }
    }

    [HttpPost]
    [Route("associateEndpointWithModule")]
    public ActionResult AssociateEndpointWithModule(int moduleID, int endpointID)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                
                var module = _dbContext.Modules.FirstOrDefault(m => m.ModuleID == moduleID);
                if (module == null)
                {
                    return NotFound(new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Module not found."
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

                
                var existingAssociation = _dbContext.ModuleEndPoints
                    .Any(me => me.ModuleID == moduleID && me.EndPointID == endpointID);

                if (existingAssociation)
                {
                    return StatusCode(400, new SimpleErrorResponse
                    {
                        Success = false,
                        Message = "Endpoint is already associated with this module."
                    });
                }

                
                var moduleEndPoint = new ModuleEndPoint
                {
                    ModuleID = moduleID,
                    EndPointID = endpointID,
                    LastChange = DateTime.UtcNow
                };

                _dbContext.ModuleEndPoints.Add(moduleEndPoint);
                _dbContext.SaveChanges();

                
                transaction.Commit();

                return Ok("Endpoint successfully associated with the module.");
            }
            catch (Exception ex)
            {
                
                transaction.Rollback();
                Console.WriteLine("An error occurred: {0}", ex.Message);
                return StatusCode(500, new SimpleErrorResponse
                {
                    Message = "An error occurred while associating the endpoint with the module."
                });
            }
        }
    }

    [HttpDelete]
    [Route("deleteEndpoint")]
    public ActionResult DeleteEndpoint(int endpointID)
    {
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
}   