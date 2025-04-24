namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
using System.Text;

public class GetAllCompaniesResponse
{
    public int companyID { get; set; }
    public required string name { get; set; }
}

public class CreateCompanyResponse
{
    public int companyID { get; set; }
}

public class CompanyInfoResponse
{
    public string name { get; set; } = string.Empty;
    public List<User> users { get; set; } = new();
}