using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class EndPoint
{
    [Key]
    public int EndPointID { get; set; }
    public required string EndPointName { get; set; }
    public required string Specification { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}