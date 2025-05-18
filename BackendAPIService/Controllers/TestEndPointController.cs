using Microsoft.AspNetCore.Mvc;
namespace BackendAPIService.Controllers;

[ApiController]
[Route("api/testendpoint")]
public class TestEndpointController : ControllerBase
{

    [HttpGet]
    [Route("testing")]
    public string TestEndpoint()
    {
        return "DTU is the best technical university in the EU";    
    }
}