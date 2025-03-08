using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Module
{
    public Module(string name)
    {
        Name = name;
    }
    [Key]
    public int ModuleID { get; set; }
    public string Name { get; set; }
}