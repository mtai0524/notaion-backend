using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notaion.Application.Common.Interfaces;
using Notaion.Application.Interfaces;
using Notaion.Application.Services;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;
using Notaion.Infrastructure.Persistence;
using Notaion.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); 
            }, ServiceLifetime.Singleton, ServiceLifetime.Transient);

            // chat
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IChatService, ChatService>();

            // account user
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();

            return services;
        }
    }
}
