using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.MixedTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("ParameterID", "EndPointID")]
public class EndPointParameter
{
    
    [ForeignKey("Parameters")]
    public int ParameterID { get; set; }
    [ForeignKey("EndPoints")]
    public int EndPointID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}