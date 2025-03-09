using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.MixedTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("RoleID", "EndPointID")]
public class RoleEndPoint
{
    
    [ForeignKey("Roles")]
    public int RoleID { get; set; }
    [ForeignKey("EndPoints")]
    public int EndPointID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}