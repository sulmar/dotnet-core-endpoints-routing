using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet_core_routing
{
    public class MyDashboardOptions
    {
        public string DashboardTitle { get; set; }
    }

    public class MyDashboardMiddleware
    {
        private readonly RequestDelegate next;
        private readonly MyDashboardOptions options;
        private readonly IContentService contentService;

        public MyDashboardMiddleware(RequestDelegate next, MyDashboardOptions options, IContentService contentService)
        {
            this.next = next;
            this.options = options;
            this.contentService = contentService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string content = contentService.Get();

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($@"<html><head><title>{options.DashboardTitle}</title><head><body>{content}</body></html>");
        }
    }

    public static class MyEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapMyDashboard(
         this IEndpointRouteBuilder endpoints,
         string pattern = "/dashboard",
         Action<MyDashboardOptions> configureOptions = null
         )
        {
            var app = endpoints.CreateApplicationBuilder();

            IContentService contentService = endpoints.ServiceProvider.GetService<IContentService>();

            if (contentService == null)
            {
                throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling " +
                                                   "'IServiceCollection.AddMyContentService' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }

            // hint: get options
            var options = new MyDashboardOptions();
            configureOptions?.Invoke(options);

            var pipeline = app
                 .UsePathBase(pattern)
                 .UseMiddleware<MyDashboardMiddleware>(options)
                 .Build();

            // Glob patterns
            // https://docs.microsoft.com/pl-pl/aspnet/core/fundamentals/file-providers?view=aspnetcore-3.1
            return endpoints.Map(pattern + "/{**path}", pipeline);

        }

        public static IEndpointConventionBuilder MapMyDashboard(
          this IEndpointRouteBuilder endpoints,
          string pattern = "/dashboard",
          MyDashboardOptions configureOptions = null
          )
        {
            var app = endpoints.CreateApplicationBuilder();

            var services = app.ApplicationServices;

            configureOptions = configureOptions ?? services.GetService<MyDashboardOptions>() ?? new MyDashboardOptions();

            var pipeline = app
                 .UsePathBase(pattern)
                 .UseMiddleware<MyDashboardMiddleware>(configureOptions)
                 .Build();

            return endpoints.Map(pattern + "/{**path}", pipeline); ;
        }
    }

}
