using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
[PrimaryKey("CompanyID", "UserID")]
public class CompanyUser
{
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}