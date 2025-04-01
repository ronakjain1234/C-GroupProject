using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Company
{
    
    [Key]
    public int CompanyID { get; set; }
    public required string CompanyName { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}