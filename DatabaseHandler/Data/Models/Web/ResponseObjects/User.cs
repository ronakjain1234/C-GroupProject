using System.Text;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class User
{
    public string Name { get; set; } = string.Empty;
    public int userID {get; set;}
    public string Email { get; set; } = string.Empty;
    public List<RoleInfo> Roles { get; set; } = new();

    public string RolesToString()
    {
        return Roles != null && Roles.Any()
            ? string.Join(", ", Roles.Select(r => r.RoleName))
            : "No Roles";
    }
}

public class RoleInfo
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
