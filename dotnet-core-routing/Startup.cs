using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using dotnet_core_routing.IServices;
using dotnet_core_routing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dotnet_core_routing
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ICustomerRepository, FakeCustomerRepository>();

            services.AddMyContentService();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMyDashboard("/mydashboard", options => options.DashboardTitle = "My dashboard");

                endpoints.Map("/version", endpoints.CreateApplicationBuilder()
                    .UseMiddleware<VersionMiddleware>()
                    .Build())
                    .WithDisplayName("Version number");

                // endpoints.MapVersion("/version");

                endpoints.Map("/", async context => await context.Response.WriteAsync("Hello"));

                endpoints.MapGet("/customers/{id:int}", async context =>
                {
                    int id = Convert.ToInt32(context.Request.RouteValues["id"]);
                   
                    ICustomerRepository customerRepository = context.RequestServices.GetRequiredService<ICustomerRepository>();
                    var customer = customerRepository.Get(id);
                  
                    context.Response.Headers.Add("Content-Type", "application/json");
                    await JsonSerializer.SerializeAsync(context.Response.Body, customer);
                    // await context.Response.WriteAsync("Hello World!");
                });

             

                endpoints.MapPost("A", async context =>
                {
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        var customer = await JsonSerializer.DeserializeAsync<Models.Customer>(context.Request.Body);

                        context.Response.StatusCode = 201;
                        context.Response.Headers.Add("Content-Type", "application/json");
                        await JsonSerializer.SerializeAsync(context.Response.Body, customer);
                    }
                });

                endpoints.MapPost("B", async context =>
                {
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        var json = await streamReader.ReadToEndAsync();

                        await context.Response.WriteAsync(json);
                    }
                });
            });
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


            // hint: gathering options
            var options = new MyDashboardOptions();
            configureOptions?.Invoke(options);

            var pipeline = app
                 .UsePathBase(pattern)
                 .UseMiddleware<MyDashboardMiddleware>(options)
                 .Build();

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

    public class MyDashboardOptions
    {
        public string DashboardTitle { get; set; }
    }

    public interface IContentService
    {
        string Get();
    }

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

    public static class VersionEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapVersion(this IEndpointRouteBuilder endpoints, string pattern)
        {
            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<VersionMiddleware>()
                .Build();

            return endpoints.Map(pattern, pipeline).WithDisplayName("Version number");
        }
    }

}
