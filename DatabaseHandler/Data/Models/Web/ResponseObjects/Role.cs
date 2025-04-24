namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Role
{
    public int roleID {get; set;}
    public string name { get; set; }
    

    public Role(int roleID, string name)
    {   
        this.roleID = roleID;
        this.name = name;
    }
}