# Endpoints w .NET Core

## Wprowadzenie
W .NET Core 2.2 wprowadzony został mechanizm *endpoints* a w .NET Core 3 stał się zalecanym sposobem mapowania żądań.
Korzystają z niego chociażby technologie MVC i SignalR. 

Czyli obecnie zamiast:

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  app.UseMvc();
}
~~~

Piszemy:

~~~ csharp
app.UseEndpoints(endpoints =>
 {
     endpoints.MapControllerRoute(
         name: "default",
         pattern: "{controller=Home}/{action=Index}/{id?}");
 });
 ~~~ 

Dlaczego to zostało zmienione?

Otóż wcześniej każdy middleware miał własny sposób mapowania ścieżek, na przykład *UseMvc()*, *UseSignalR()*
To powodowało, że każdy framework był konfigurowany w nieco w inny sposób. 

Dzięki **endpoints** zostało to zunifikowane i teraz każdy programista może skorzystać z tego mechanizmu podczas tworzenia własnej warstwy pośredniej (middleware).

 
W takim razie w jaki sposób zastosować to we własnym rozwiązaniu? 

## Wymaganie

Chcemy utworzyć własny dasboard, który będzie podpinany pod url */mydashoard*

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapMyDashboard("/mydashboard");
     }
 }
~~~



## Warstwa pośrednia (middleware)

Na początek utwórzmy warstwę pośrednią

~~~ csharp
 public class MyDashboardMiddleware
    {
        private readonly RequestDelegate next;
  
        public MyDashboardMiddleware(RequestDelegate next)
        {
            this.next = next;     
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string title = "My dashboard";
            string content = "Hello World!";

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($@"<html><head><title>{title}</title><head><body>{content}</body></html>");
        }
    }
~~~

### Użycie
Teraz możemy podpiąć w klasie Startup z użyciem endpoints:

~~~ csharp
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
 {
     if (env.IsDevelopment())
     {
         app.UseDeveloperExceptionPage();
     }

     app.UseRouting();

     app.UseEndpoints(endpoints =>
     {
         endpoints.Map("/", async context => await context.Response.WriteAsync("Hello World!"));

         endpoints.Map("/mydashboard", endpoints.CreateApplicationBuilder()
             .UseMiddleware<MyDashboardMiddleware>()
             .Build());
            }
  ~~~
               

### Utworzenie metody rozszerzającej 

W celu ułatwienia korzystania z naszej warstwy utworzymy metodę rozszerzającą *MapMyDashboard()*

~~~ csharp
 public static class MyEndpointRouteBuilderExtensions
 {
     public static IEndpointConventionBuilder MapMyDashboard(
      this IEndpointRouteBuilder endpoints,
      string pattern = "/dashboard")
     {
         var app = endpoints.CreateApplicationBuilder();

         var pipeline = app
              .UsePathBase(pattern)
              .UseMiddleware<MyDashboardMiddleware>()
              .Build();

         // Glob patterns
         // https://docs.microsoft.com/pl-pl/aspnet/core/fundamentals/file-providers?view=aspnetcore-3.1
         return endpoints.Map(pattern + "/{**path}", pipeline);

     }
   }
~~~

Zastosowanie metody *MapMyDashboard()*

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.Map("/", async context => await context.Response.WriteAsync("Hello World!"));

        endpoints.MapMyDashboard("/mydashboard");
     }
 }
~~~


## Konfiguracja
W jaki sposób przekazać opcje na wzór MapHub?

### Opcje
Tworzymy klasę opcji:

~~~ csharp
public class MyDashboardOptions
{
     public string DashboardTitle { get; set; }
} 
~~~

### Warstwa pośrednia (middleware)

Przekazujemy ją poprzez konstruktor

~~~ csharp
public class MyDashboardMiddleware
    {
        private readonly RequestDelegate next;
        private readonly MyDashboardOptions options;

        public MyDashboardMiddleware(RequestDelegate next, MyDashboardOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string title = options.DashboardTitle;
            string content = "Hello World!";
            
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($@"<html><head><title>{title}</title><head><body>{content}</body></html>");
        }
    }
~~~

### Metoda rozszerzająca

Dodajemy parametr Action<T> gdzie T to nasza klasa z opcjami:

~~~ csharp
public static class MyEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapMyDashboard(
         this IEndpointRouteBuilder endpoints,
         string pattern = "/dashboard",
         Action<MyDashboardOptions> configureOptions = null
         )
        {
            var app = endpoints.CreateApplicationBuilder();

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
~~~

### Użycie

~~~ csharp
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
    }
}
~~~


## Wstrzykiwanie zależności (Dependency Injections)

### Utworzenie usługi

~~~ csharp
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
 ~~~
 
 ### Utworzenie warstwy pośredniej (middleware)
 
 ~~~ csharp
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
  ~~~
  
  ### Utworzenie metody rozszerzającej
  
  ~~~ csharp
  public static class MyServiceCollectionExtensions
    {
        public static IServiceCollection AddMyContentService([NotNull] this IServiceCollection services)
        {
            services.AddTransient<IContentService, MyContentService>();

            return services;
        }
    }
~~~

### Użycie

~~~ csharp
public void ConfigureServices(IServiceCollection services)
 {
     services.AddMyContentService();
 }
~~~

## Podsumowanie
Przedstawiony kod można potraktować jako szablon do tworzenia własnych rozwiązań.
Powstał w oparciu o analizę kodu źródłowego Signal-R 
https://github.com/dotnet/aspnetcore/tree/master/src/SignalR/server/SignalR/src
a w szczególności klasy 
https://github.com/dotnet/aspnetcore/blob/master/src/SignalR/server/SignalR/src/HubEndpointRouteBuilderExtensions.cs

 

