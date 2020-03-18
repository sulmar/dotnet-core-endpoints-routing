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

