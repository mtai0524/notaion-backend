using Microsoft.Extensions.DependencyInjection;
using Notaion.Application.Mappings;
using System.Reflection;

namespace Notaion.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddAutoMapper(typeof(ChatMappingProfile));
            services.AddAutoMapper(typeof(UserMappingProfile));

            return services;
        }
    }
}