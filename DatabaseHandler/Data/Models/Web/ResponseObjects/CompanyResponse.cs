namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
using System.Text;

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
    public List<User> Users { get; set; } = new();
}