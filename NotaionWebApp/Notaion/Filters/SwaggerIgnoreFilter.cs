using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notaion.Filters
{
    public class SwaggerIgnoreFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;

            var ignoredParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.GetCustomAttributes(typeof(Attributes.SwaggerIgnoreAttributes), false).Any());

            foreach (var param in ignoredParams)
            {
                var toRemove = operation.Parameters.SingleOrDefault(p => p.Name == param.Name);
                if (toRemove != null)
                    operation.Parameters.Remove(toRemove);
            }
        }
    }
}
