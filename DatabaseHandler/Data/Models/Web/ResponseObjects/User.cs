using System.Text;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<RoleInfo> Roles { get; set; } = new();

    public string RolesToString()
    {
        StringBuilder sb = new();
        foreach (var role in Roles)
        {
            sb.Append(role.RoleName);
            if (role != Roles.Last())
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }
}

public class RoleInfo
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
}