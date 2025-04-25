using System.Text;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class User
{
    public string userName { get; set; } = string.Empty;
    public int userID {get; set;}
    public string email { get; set; } = string.Empty;
    public List<RoleInfo> roles { get; set; } = new();

    public string RolesToString()
    {
        return roles != null && roles.Any()
            ? string.Join(", ", roles.Select(r => r.roleName))
            : "No Roles";
    }
}

public class RoleInfo
{
    public int roleID { get; set; }
    public string roleName { get; set; } = string.Empty;
}
