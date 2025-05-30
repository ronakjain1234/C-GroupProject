using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Module
{
    [Key]
    public int ModuleID { get; set; }
    public required string Name { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}