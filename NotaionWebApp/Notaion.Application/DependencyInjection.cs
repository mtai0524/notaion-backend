using Microsoft.Extensions.DependencyInjection;
using Notaion.Application.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
