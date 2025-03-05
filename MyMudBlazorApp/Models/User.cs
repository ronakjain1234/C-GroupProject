namespace MyMudBlazorApp.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string BankId { get; set; }
        public string UserType { get; set; } // "Intern", "Contractor", etc.
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 