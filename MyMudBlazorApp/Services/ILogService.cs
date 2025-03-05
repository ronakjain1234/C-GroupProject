using MyMudBlazorApp.Models;

namespace MyMudBlazorApp.Services
{
    public interface ILogService
    {
        Task LogAsync(string message, string level = "Information");
        Task<IEnumerable<LogEntry>> GetLogsAsync();
    }
} 