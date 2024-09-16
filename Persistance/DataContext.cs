using Domains;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistance
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<TodoItem> TodoItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppUser>()
                            .HasMany(u => u.TodoItems)
                            .WithOne(t => t.AppUser)
                            .HasForeignKey(t => t.AppUserId)
                            .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
