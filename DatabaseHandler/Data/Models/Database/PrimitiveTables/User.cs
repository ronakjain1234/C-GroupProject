using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class User
{
    [Key]
    public int UserID { get; set; }
    public string Name { get; set; }
    public string Email {get; set; }
    public string Roles{get; set;}
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}