using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class User
{
    public User(string name)
    {
        Name = name;
    }
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
}