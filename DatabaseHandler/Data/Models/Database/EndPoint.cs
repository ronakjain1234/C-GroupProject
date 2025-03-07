using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class EndPoint
{
    public EndPoint(string path)
    {
        Path = path;
    }
    
    [Key]
    public int ID { get; set; }
    public string Path { get; set; }
}