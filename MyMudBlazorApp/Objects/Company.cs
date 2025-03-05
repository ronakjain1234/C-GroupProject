namespace MyMudBlazorApp.Objects;

public class Company
{
    public string Name { get; set; }
    public List<User> Users;

    public Company(string name)
    {
        Name = name;
        Users = new();
    }

    public void AddUser(User user)
    {
        Users.Add(user);
    }

    public void RemoveUser(User user)
    {
        Users.Remove(user);
    }

    public List<User> GetUsers()
    {
        return Users;
    }
}