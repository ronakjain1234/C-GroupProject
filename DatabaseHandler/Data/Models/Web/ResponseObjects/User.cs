using System.Text;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class User
{
    public string name { get; set; } = string.Empty;
    public int userID {get; set;}
    public string Email { get; set; } = string.Empty;
    public List<RoleInfo> Roles { get; set; } = new();

    public string RolesToString()
    {
        return Roles != null && Roles.Any()
            ? string.Join(", ", Roles.Select(r => r.name))
            : "No Roles";
    }
}

public class RoleInfo
{
    public int roleID { get; set; }
    public string name { get; set; } = string.Empty;
}
