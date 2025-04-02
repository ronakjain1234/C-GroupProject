using System.Text;

namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
        Roles = new();
    }

    public string RolesToString()
    {
        StringBuilder sb = new();
        foreach (Role role in Roles)
        {
            sb.Append(role.Name);
            if (role != Roles.Last())
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }
}