using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler.Data.Models.Database.ReferencingTables;

public class UserEmail
{
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [Key]
    public string Email { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}