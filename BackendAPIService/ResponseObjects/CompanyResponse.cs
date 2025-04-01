
using MyMudBlazorApp.Objects;
namespace BackendAPIService.ResponseObjects;

public class GetAllCompaniesResponse
{
    public int companyId { get; set; }
    public required string companyName { get; set; }

    public required string imageURL { get; set; }
}

public class CreateCompanyResponse 
{
    public int companyId { get; set; }
}



public class GetDetailedCompanyInfo
{
    public required string companyName { get; set; }
    public required List<User>  userInformation { get; set; }
}
