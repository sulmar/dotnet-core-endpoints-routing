using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
                endpoints.MapVersion("/version");

                endpoints.MapGet("/", async context =>
                {
                    var customer = new { firstname = "Marcin", lastname = "Sulecki" };

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
