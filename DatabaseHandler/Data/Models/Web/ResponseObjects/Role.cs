namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Role
{
    public string Name { get; set; }

    public Role(string name)
    {
        Name = name;
    }
}