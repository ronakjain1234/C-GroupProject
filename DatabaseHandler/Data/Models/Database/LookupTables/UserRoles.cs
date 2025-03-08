using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("UserID", "RoleID")]
public class UserRole
{
    
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [ForeignKey("Roles")]
    public int RoleID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}