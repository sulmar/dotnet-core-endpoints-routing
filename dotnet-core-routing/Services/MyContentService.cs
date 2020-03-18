using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace dotnet_core_routing
{
    public class MyContentService : IContentService
    {
        public string Get()
        {
            return "Hello Dashboard!";
        }
    }

    public static class MyServiceCollectionExtensions
    {
        public static IServiceCollection AddMyContentService([NotNull] this IServiceCollection services)
        {
            services.AddTransient<IContentService, MyContentService>();

            return services;
        }
    }

}
