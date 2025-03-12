using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.MixedTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("ModuleID", "EndPointID")]
public class ModuleEndPoint
{
    
    [ForeignKey("Modules")]
    public int ModuleID { get; set; }
    [ForeignKey("EndPoints")]
    public int EndPointID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}