using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler.Data.Models.Database.ReferencingTables;
[PrimaryKey("UserID", "Password")]
public class UserPassword
{
    [ForeignKey("Users")]
    public int UserID { get; set; }
    public required string Password { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}