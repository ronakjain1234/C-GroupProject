namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Role
{
    public int roleID {get; set;}
    public string roleName { get; set; }
    

    public Role(int roleID, string roleName)
    {   
        this.roleID = roleID;
        this.roleName = roleName;
    }
}