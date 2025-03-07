using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class User
{
    public User(string name)
    {
        Name = name;
    }
    [Key]
    public int UserID { get; set; }
    public string Name { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}