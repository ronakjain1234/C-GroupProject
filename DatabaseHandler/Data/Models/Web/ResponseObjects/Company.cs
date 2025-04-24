namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Company
{
    public string Name { get; set; }
    public List<User> users;

    public Company(string name)
    {
        Name = name;
        users = new();
    }

    public void AddUser(User user)
    {
        users.Add(user);
    }

    public void RemoveUser(User user)
    {
        users.Remove(user);
    }

    public List<User> GetUsers()
    {
        return users;
    }
}