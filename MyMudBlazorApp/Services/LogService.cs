using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyMudBlazorApp.Models;

namespace MyMudBlazorApp.Services
{
    public class LogService : ILogService
    {
        private static readonly List<LogEntry> _logs = new();

        public Task LogAsync(string message, string level = "Information")
        {
            var logEntry = new LogEntry
            {
                Id = _logs.Count + 1,
                Message = message,
                Level = level,
                Timestamp = DateTime.UtcNow
            };

            _logs.Add(logEntry);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<LogEntry>> GetLogsAsync()
        {
            return Task.FromResult(_logs.OrderByDescending(l => l.Timestamp).AsEnumerable());
        }
    }
} 