# Endpoints w .NET Core

## Wprowadzenie
W .NET Core 2.2 wprowadzony został mechanizm *endpoints* a w .NET Core 3 stał się zalecanym sposobem mapowania żądań.
Korzystają z niego technologie chociażby MVC i SignalR. W jaki sposób zastosować we własnym rozwiązaniu?
Ale zanim przedstawię rozwiązanie warto wyjaśnić dlaczego je wprowadzono.

Otóż wcześniej każdy middleware miał własny sposób mapowania ścieżek, na przykład *UseMvc()*, *UseSignalR()*

To powodowało, że każdy framework był mapowany nieco w inny sposób. 

Dzięki **endpoints** zostało to zunifikowane i teraz każdy programista może skorzystać z tego mechanizmu podczas tworzenia własnej warstwy pośredniej (middleware).

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

### Metoda rozszerzająca z akcją do obsługi opcji

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

### Zastosowanie

~~~ csharp
  endpoints.MapMyDashboard("/mydashboard", options => options.DashboardTitle = "My dashboard");
~~~


## Wstrzykiwanie zależności (Dependency Injections)

