using BackendAPIService.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    [HttpGet]
    [Route("/getCompanies")]
    public ActionResult<List<Company>> Get(int userID, int limit = 50, int offset = 0, string? searchString = null)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/createCompany")]
    public ActionResult<SimpleErrorResponse> CreateCompany(int userID, string companyName)
    {
        throw new NotImplementedException();
    }

}