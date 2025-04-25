namespace DatabaseHandler.Data.Models.Web.ResponseObjects;
public class Company
{
    public string name { get; set; }
    public List<User> users;

    public Company(string name)
    {
        this.name = name;
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