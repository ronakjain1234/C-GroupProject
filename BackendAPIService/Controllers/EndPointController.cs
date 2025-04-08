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




}