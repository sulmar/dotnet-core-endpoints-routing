# Endpoints w .NET Core

## Wprowadzenie
W .NET Core 2.2 wprowadzony został mechanizm *endpoints* a w .NET Core 3 stał się zalecanym sposobem mapowania żądań.
Korzystają z niego technologie chociażby MVC i SignalR. W jaki sposób zastosować we własnym rozwiązaniu?
Ale zanim przedstawię rozwiązanie warto wyjaśnić dlaczego je wprowadzono.

Otóż wcześniej każdy middleware miał własny sposób mapowania ścieżek. 
np. UseMvc, UseSignalR

To powodowało, że każdy framework był mapowany nieco w inny sposób. 

Dzięki **endpoints** zostało to zunifikowane i teraz każdy programista może skorzystać z tego mechanizmu podczas tworzenia własnej warstwy pośredniej (middleware).




