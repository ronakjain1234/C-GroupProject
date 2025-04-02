using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Role
{
    [Key]
    public int RoleID { get; set; }
    
    public required string Name { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}