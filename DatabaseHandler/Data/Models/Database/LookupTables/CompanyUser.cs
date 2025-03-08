using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
[PrimaryKey("CompanyID", "UserID")]
public class CompanyUser
{
    [ForeignKey("Company")]
    public int CompanyID { get; set; }
    [ForeignKey("User")]
    public int UserID { get; set; }
}