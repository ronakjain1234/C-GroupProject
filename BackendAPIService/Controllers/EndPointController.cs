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
                var endpointID = newEndpoint.EndPointID;
                var companyEndPoint = new CompanyEndPoint
                {
                    CompanyID = companyID,
                    EndPointID = endpointID,
                    LastChange = DateTime.UtcNow
                };
                _dbContext.CompanyEndPoints.Add(companyEndPoint);
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


}