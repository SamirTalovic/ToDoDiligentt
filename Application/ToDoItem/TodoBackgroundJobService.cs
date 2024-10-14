using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add this for ILogger
using Persistance;

namespace ToDoDiligent.Services
{
    public class TodoBackgroundJobService
    {
        private readonly DataContext _context;
        private readonly ILogger<TodoBackgroundJobService> _logger;

        public TodoBackgroundJobService(DataContext context, ILogger<TodoBackgroundJobService> logger) 
        {
            _context = context;
            _logger = logger;
        }

        public async Task CleanupOldTodoItems()
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-30);

            var oldTodoItems = await _context.TodoItems
                .Where(t => t.IsCompleted && t.CompletedAt < thresholdDate)
                .ToListAsync();

            if (oldTodoItems.Any())
            {
                _context.TodoItems.RemoveRange(oldTodoItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} old completed todo items deleted.", oldTodoItems.Count); 
            }
            else
            {
                _logger.LogInformation("No old todo items found for cleanup."); 
            }
        }

        public void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate("cleanup-old-todo-items", () => CleanupOldTodoItems(), Cron.Daily);
            _logger.LogInformation("Scheduled the 'cleanup-old-todo-items' job to run daily."); 
        }
    }
}
