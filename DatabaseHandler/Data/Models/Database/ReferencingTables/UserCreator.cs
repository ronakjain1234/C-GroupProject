using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler.Data.Models.Database;
[PrimaryKey("UserID", "CreatorUserID")]

public class UserCreator
{
    [ForeignKey("Users")]
    public int UserID { get; set; }
    [ForeignKey("Users")]
    public int CreatorUserID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}