using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("CompanyID", "RoleID")]
public class CompanyRoles
{
    
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ForeignKey("Roles")]
    public int RoleID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}