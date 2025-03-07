using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Company
{
    public Company(string companyName)
    {
        CompanyName = companyName;
    }
    
    [Key]
    public int CompanyID { get; set; }
    public string CompanyName { get; set; }
}