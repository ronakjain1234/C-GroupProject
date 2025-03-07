using Microsoft.AspNetCore.Mvc;
using MyMudBlazorApp.Objects;

namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    
    [HttpGet]
    [Route("/get")]
    public List<Company> Get(int userID)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/create")]
    public List<Company> CreateCompany(int userID, string companyName)
    {
        throw new NotImplementedException();
    }

}