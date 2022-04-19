using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

// This example demonstrates advanced configuration, including API key authentication
// and custom hub connection options for robust real-time communication.

namespace SignalRMapRealtime.Examples
{
    public class AdvancedUsage
    {
        public static async Task RunAdvancedScenarioAsync(string vehicleId, string apiKey)
        {
            var baseUrl = "https://localhost:5001";

            // 1. Configure HttpClient with Authentication
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey); // Example header for custom Auth

            // 2. Configure SignalR Hub Connection with Authentication and Options
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/locationHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(apiKey); // Or use Auth header in options
                })
                .WithAutomaticReconnect()
                .Build();

            // 3. Set up event handlers
            hubConnection.On<object>("LocationUpdated", (location) => 
            {
                Console.WriteLine($"Advanced Update: {location}");
            });

            // 4. Start connection with error handling
            try
            {
                await hubConnection.StartAsync();
                Console.WriteLine("Hub connection established.");
                
                // Subscribe to a specific vehicle
                await hubConnection.InvokeAsync("SubscribeToVehicle", vehicleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
            }

            // 5. Perform authenticated REST operation
            try 
            {
                var response = await client.GetAsync($"/api/v1/vehicles/{vehicleId}");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Authenticated request successful.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Request failed: {ex.StatusCode} - {ex.Message}");
            }
        }
    }
}
