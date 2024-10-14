using Application.Interfaces;
using Infrastracture.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Persistance;
using System.Collections.Generic;

namespace API.Extension
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.AddDbContext<DataContext>(opt =>
                opt.UseSqlServer(@"Server=.;Database=Falcet123;Trusted_Connection=True;TrustServerCertificate=True;", b => b.MigrationsAssembly("ToDoDiligent")));
            

            services.AddCors(opt => {
                opt.AddPolicy("CorsPolicy", policy => {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });
            services.AddScoped<IUserAccessor, UserAccessor>();


            return services;
        }
    }
}
