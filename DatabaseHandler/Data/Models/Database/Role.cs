using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Role
{
    public Role(string name)
    {
        Name = name;
    }
    [Key]
    public int RoleID { get; set; }
    
    public string Name { get; set; }
    
}