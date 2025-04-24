namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Role
{
    public int roleId {get; set;}
    public string Name { get; set; }
    

    public Role(int roleid, string name)
    {   
        roleId = roleid;
        Name = name;
    }
}