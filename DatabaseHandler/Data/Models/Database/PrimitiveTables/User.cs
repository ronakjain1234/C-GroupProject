using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class User
{
    [Key]
    public int UserID { get; set; }
    public required string Name { get; set; }
    public required string Email {get; set; }
    public required string  Roles {get; set;}
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}