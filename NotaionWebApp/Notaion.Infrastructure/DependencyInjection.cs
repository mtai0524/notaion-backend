using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notaion.Application.Common.Interfaces;
using Notaion.Application.Interfaces.Services;
using Notaion.Application.Services;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;
using Notaion.Infrastructure.Identity;
using Notaion.Infrastructure.Persistence;
using Notaion.Infrastructure.Repositories;
using Notaion.Infrastructure.Services;

namespace Notaion.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            IdentityConfiguration.ConfigureIdentityOptions(services, configuration);
            IdentityConfiguration.ConfigureAuthentication(services, configuration);

            // chat
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IChatService, ChatService>();

            // generic repo
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // uow
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // account user
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddSingleton<ChatModelTrainer>();

            // services
            services.AddSingleton<IEncryptionService, EncryptionService>();

            return services;
        }
    }
}
