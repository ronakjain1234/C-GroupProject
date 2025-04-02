using DatabaseHandler.Data.Models.Database;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class GetRolesInCompanyResponse 
{
    public required List<Role> roles { get; set; }
}

public class CreateRollResponse 
{
    public int roleId { get; set; }
}
