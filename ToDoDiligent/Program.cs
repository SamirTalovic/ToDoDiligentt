using API.Extension;
using FluentAssertions.Common;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ToDoDiligent.Extension;
using ToDoDiligent.Middleware;
using ToDoDiligent.Services;
using static Application.ToDoItem.Create;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Rebus.Config;
using Rebus.RabbitMq;
using Rebus.ServiceProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(opt => {
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddHangfire(configuration =>
        configuration
        .UseRecommendedSerializerSettings()
        .UseSimpleAssemblyNameTypeSerializer()
        .UseSqlServerStorage(@"Server=.;Database=HangfireJobs;Trusted_Connection=True;TrustServerCertificate=True;User Id=appuser;Password=yourpassword")
        ); 
builder.Services.AddHangfireServer()
    ;
builder.Services.AddTransient<TodoBackgroundJobService>();
builder.Services.AutoRegisterHandlersFromAssemblyOf<MyMessageHandler>();
builder.Services.AddMediatR(typeof(CreateTodoItemCommand).Assembly);
builder.Services.AddSignalR();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRebus(configure => configure
    .Transport(t => t.UseRabbitMq("amqp://guest:guest@localhost", "myQueue"))
    .Logging(l => l.ColoredConsole()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TokenMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}
app.Services.StartRebus();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();
app.MapControllers();
app.MapHangfireDashboard();
app.MapHub<TodoHub>("/todoHub");
app.Run();
