using Hangfire;
using Microsoft.EntityFrameworkCore;
using Persistance;

namespace ToDoDiligent.Services
{
    public class TodoBackgroundJobService
    {
        private readonly DataContext _context;

        public TodoBackgroundJobService(DataContext context)
        {
            _context = context;
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

                Console.WriteLine($"{oldTodoItems.Count} old completed todo items deleted.");
            }
            else
            {
                Console.WriteLine("No old todo items found for cleanup.");
            }
        }
        public void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate("cleanup-old-todo-items", () => CleanupOldTodoItems(), Cron.Daily);
        }
    }
}
