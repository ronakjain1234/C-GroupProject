using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler.Data.Models.Database.ReferencingTables;

public class UserActive
{
    [Key]
    [ForeignKey("Users")]
    public int UserID { get; set; }
    public bool Active { get; set; }
}