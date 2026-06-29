using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRMapRealtime.Integration; // Adjust based on your actual namespace

// This example shows how to register the necessary services in a standard
// ASP.NET Core application's Dependency Injection container.

namespace SignalRMapRealtime.Examples
{
    public class IntegrationExample
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // 1. Register base services (assuming extension method exists)
            // services.AddSignalRMapRealtime(); 

            // 2. Register custom Http client for API interaction
            services.AddHttpClient("TrackerApiClient", client =>
            {
                client.BaseAddress = new System.Uri("https://localhost:5001");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // 3. Register a background worker or service that uses the tracker
            // services.AddHostedService<MyCustomTrackingWorker>();
        }
    }
}
