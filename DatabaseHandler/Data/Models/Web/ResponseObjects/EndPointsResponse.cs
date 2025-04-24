namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class GetEndpointsForRolesResponse
{
    //public required List<Endpoint> endpoints { get; set; }
}

public class GetEndPointsForCompaniesResponse
{
    //public required List<Endpoint> endpoints { get; set; }
}

public class GetEndpoints
{
    //public required List<Endpoint>[] Modules { get; set; }
}

public class EndpointResponse
{
    public int endpointID { get; set; }
    public required string Path { get; set; }
}
