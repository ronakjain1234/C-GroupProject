using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.MixedTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("CompanyID", "EndPointID")]
public class CompanyEndPoint
{
    
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ForeignKey("EndPoints")]
    public int EndPointID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}