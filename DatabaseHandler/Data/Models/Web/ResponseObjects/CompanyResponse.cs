namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class GetAllCompaniesResponse
{
    public int companyId { get; set; }
    public required string companyName { get; set; }

}

public class CreateCompanyResponse 
{
    public int companyId { get; set; }
}

public class CompanyInfoResponse
{
    public string CompanyName { get; set; } = string.Empty;
    public List<UserInfo> Users { get; set; } = new();
}

public class UserInfo
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<RoleInfo> Roles { get; set; } = new();
}

public class RoleInfo
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
