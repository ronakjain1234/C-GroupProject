using System.Text;

namespace MyMudBlazorApp.Objects;

public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }

    public User()
    {
        Name = "NOT INITIALIZED";
        Email = "NOT INITIALIZED";
        Roles = new();
    }

    public string RolesToString()
    {
        StringBuilder sb = new();
        foreach (var role in Roles)
        {
            sb.Append(role);
            if (role != Roles.Last())
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }
}