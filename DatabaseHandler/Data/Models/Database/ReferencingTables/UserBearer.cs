using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler.Data.Models.Database.ReferencingTables;

public class UserBearer
{
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [Key]
    public required string BearerToken { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}