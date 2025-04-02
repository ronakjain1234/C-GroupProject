using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database;

public class Parameter
{
    
    [Key]
    public int ParameterID { get; set; }
    public string ParameterName { get; set; }
    public string ParameterType { get; set; }
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}