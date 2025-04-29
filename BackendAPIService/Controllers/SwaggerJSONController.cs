using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using DatabaseHandler;

[ApiController]
[Route("api/swagger")]
public class SwaggerJSONController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public SwaggerJSONController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetSwaggerJsonForUser()
    {
        int userID = 0;

        var roleIds = _dbContext.UserRoles
            .Where(ur => ur.UserID == userID)
            .Select(ur => ur.RoleID)
            .Distinct()
            .ToList();

        var endpointIds = _dbContext.RoleEndPoints
            .Where(re => roleIds.Contains(re.RoleID))
            .Select(re => re.EndPointID)
            .Distinct()
            .ToList();

        var endpoints = _dbContext.EndPoints
            .Where(ep => endpointIds.Contains(ep.EndPointID))
            .ToList();

        var swagger = new Dictionary<string, object>
        {
            ["openapi"] = "3.0.1",
            ["info"] = new
            {
                title = "BackendAPIService",
                version = "1.0"
            },
            ["paths"] = new Dictionary<string, object>()
        };

        var paths = (Dictionary<string, object>)swagger["paths"];

        foreach (var endpoint in endpoints)
        {
            var parameterIds = _dbContext.EndPointParameters
                .Where(p => p.EndPointID == endpoint.EndPointID)
                .Select(p => p.ParameterID)
                .ToList();

            var parameters = _dbContext.Parameters
                .Where(p => parameterIds.Contains(p.ParameterID))
                .Select(p => new
                {
                    name = p.ParameterName,
                    @in = "path",
                    required = true,
                    schema = new
                    {
                        type = MapToOpenApiType(p.ParameterType)
                    }
                })
                .ToList();

            var returnParameterIds = _dbContext.EndPointReturnValues
                .Where(r => r.EndPointID == endpoint.EndPointID)
                .Select(r => r.ParameterID)
                .ToList();

            var returnProperties = _dbContext.Parameters
                .Where(p => returnParameterIds.Contains(p.ParameterID))
                .Select(p => new
                {
                    Name = p.ParameterName,
                    Type = MapToOpenApiType(p.ParameterType)
                })
                .ToList();

            var responseSchema = new Dictionary<string, object>();
            foreach (var prop in returnProperties)
            {
                responseSchema[prop.Name] = new { type = prop.Type };
            }

            var method = endpoint.Type.ToLower();
            var methodObj = new Dictionary<string, object>
            {
                ["summary"] = "Auto-generated endpoint",
                ["parameters"] = parameters,
                ["responses"] = new Dictionary<string, object>
                {
                    ["200"] = new
                    {
                        description = "Success",
                        content = new Dictionary<string, object>
                        {
                            ["application/json"] = new
                            {
                                schema = new
                                {
                                    type = "object",
                                    properties = responseSchema
                                }
                            }
                        }
                    }
                }
            };

            if (!paths.ContainsKey(endpoint.Path))
            {
                paths[endpoint.Path] = new Dictionary<string, object>();
            }

            ((Dictionary<string, object>)paths[endpoint.Path])[method] = methodObj;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(swagger, options);
        return Content(json, "application/json");
    }

    private string MapToOpenApiType(string dbType)
    {
        return dbType.ToLower() switch
        {
            "int" or "integer" => "integer",
            "string" => "string",
            "bool" or "boolean" => "boolean",
            "float" or "double" => "number",
            _ => "string"
        };
    }
}
