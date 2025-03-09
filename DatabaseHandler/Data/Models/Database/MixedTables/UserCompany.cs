using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
[PrimaryKey("CompanyID", "UserID")]
public class UserCompany
{
    
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}