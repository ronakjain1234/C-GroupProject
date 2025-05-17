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

public class localCompany
{
    public DatabaseHandler.Data.Models.Database.Company company {get; set;}
    public bool isSelected {get; set;}
}

public class EndpointResponse
{
    public int endpointID { get; set; }
    public required string Name { get; set; }
    public required string Spec { get; set; }
}

public class LocalEndpoint
{
    public int endpointID { get; set; }
    public required string Name { get; set; }
    public required string Spec { get; set; }
    public bool isSelected {get; set;}
}
