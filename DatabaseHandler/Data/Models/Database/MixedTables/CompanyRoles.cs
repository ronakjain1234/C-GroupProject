using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace DatabaseHandler.Data.Models.Database.MixedTables;


[PrimaryKey("CompanyID", "RoleID")]
public class CompanyRole
{
    
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ForeignKey("Roles")]
    public int RoleID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}