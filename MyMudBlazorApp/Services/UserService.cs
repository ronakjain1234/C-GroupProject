using MyMudBlazorApp.Models;

namespace MyMudBlazorApp.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsersByBankAsync(string bankId);
        Task<IEnumerable<User>> GetUsersByTypeAsync(string bankId, string userType);
        Task<User> AddUserAsync(User user);
        Task<IEnumerable<Bank>> GetBanksAsync();
    }

    public class UserService : IUserService
    {
        private static readonly List<Bank> _banks = new()
        {
            new Bank { Name = "DNB Bank" },
            new Bank { Name = "Danske Bank" },
            new Bank { Name = "DeltaconiX" }
        };

        private static readonly List<User> _users = new()
        {
            new User { Name = "John Doe", Role = "Administrator", BankId = _banks[0].Id, UserType = "Intern" },
            new User { Name = "Jane Smith", Role = "Data Analyst", BankId = _banks[0].Id, UserType = "Intern" },
            new User { Name = "Alice Johnson", Role = "Product Owner", BankId = _banks[0].Id, UserType = "Contractor" },
            new User { Name = "Bob Wilson", Role = "Administrator", BankId = _banks[0].Id, UserType = "Contractor" }
        };

        public Task<IEnumerable<User>> GetUsersByBankAsync(string bankId)
        {
            var users = _users.Where(u => u.BankId == bankId && u.IsActive);
            return Task.FromResult(users);
        }

        public Task<IEnumerable<User>> GetUsersByTypeAsync(string bankId, string userType)
        {
            var users = _users.Where(u => u.BankId == bankId && u.UserType == userType && u.IsActive);
            return Task.FromResult(users);
        }

        public Task<User> AddUserAsync(User user)
        {
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<Bank>> GetBanksAsync()
        {
            return Task.FromResult(_banks.Where(b => b.IsActive));
        }
    }
} 