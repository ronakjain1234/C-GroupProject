using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models;

public class DBCompany
{
    public DBCompany(string companyName)
    {
        CompanyName = companyName;
    }
    
    [Key]
    public string CompanyName { get; set; }
}