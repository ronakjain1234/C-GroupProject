using DatabaseHandler.Data.Models.Database;

namespace BackendAPIService.ResponseObjects;

public class GetRolesInCompanyResponse 
{
    public required List<Role> roles { get; set; }
}

public class CreateRollResponse 
{
    public int roleId { get; set; }
}
