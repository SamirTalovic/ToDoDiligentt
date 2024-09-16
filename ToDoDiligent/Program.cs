using API.Extension;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ToDoDiligent.Extension;
using ToDoDiligent.Middleware;
using ToDoDiligent.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(opt => {
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddHangfire(configuration =>
        configuration.UseSqlServerStorage(@"Server=.;Database=Falcet123;Trusted_Connection=True;TrustServerCertificate=True;")); 
builder.Services.AddHangfireServer()
    ;
builder.Services.AddSignalR();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHangfireDashboard();
app.MapHub<TodoHub>("/todoHub");
/*RecurringJob.AddOrUpdate<TodoBackgroundJobService>(
           "cleanup-old-todo-items",
           service => service.CleanupOldTodoItems(),
           Cron.Daily);*/
app.Run();
