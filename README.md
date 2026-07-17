// ... (rest of the file remains the same)

## ValidationExtensions

`ValidationExtensions` provides a comprehensive set of extension methods for validating common data types and formats. It includes utilities for checking if a string is a valid email, phone number, URL, IP address, GUID, or if it matches a specific pattern. Additionally, it offers methods for verifying if a value falls within a range, if a string has a certain length, or if a collection contains elements.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new route
        var newRoute = new RouteDto
        {
            Name = "Downtown Delivery Route",
            Description = "Delivery route through city center"
        };

        var createResponse = await client.PostAsJsonAsync("api/route", newRoute);
        var createdRoute = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Created route: {createdRoute?.Data?.Name} with ID: {createdRoute?.Data?.Id}");

        // Get all routes with pagination
        var getAllResponse = await client.GetAsync("api/route?pageNumber=1&pageSize=10");
        var allRoutes = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<RouteDto>>>();
        Console.WriteLine($"Total routes: {allRoutes?.Data?.TotalCount}");

        // Get a specific route by ID
        var getByIdResponse = await client.GetAsync("api/route/1");
        var singleRoute = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Route: {singleRoute?.Data?.Name}");

        // Update a route
        var updateRoute = new RouteDto
        {
            Name = "Updated Downtown Delivery Route",
            Description = "Updated delivery route description"
        };
        var updateResponse = await client.PutAsJsonAsync("api/route/1", updateRoute);
        var updatedRoute = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Updated route: {updatedRoute?.Data?.Name}");

        // Calculate route metrics
        var calculateResponse = await client.PostAsync("api/route/1/calculate", null);
        var routeMetrics = await calculateResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Console.WriteLine($"Route calculation: {routeMetrics?.Message}");

        // Delete a route
        var deleteResponse = await client.DeleteAsync("api/route/1");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}

## DateTimeExtensions

`DateTimeExtensions` offers a collection of helper methods for common date and time calculations, such as friendly time spans, period boundaries, rounding, range checks, age calculation, Unix timestamp conversion, and ISO‑8601 formatting. These extensions simplify working with `DateTime` values throughout the tracking application.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new route
        var newRoute = new RouteDto
        {
            Name = "Downtown Delivery Route",
            Description = "Delivery route through city center"
        };

        var createResponse = await client.PostAsJsonAsync("api/route", newRoute);
        var createdRoute = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Created route: {createdRoute?.Data?.Name} with ID: {createdRoute?.Data?.Id}");

        // Get all routes with pagination
        var getAllResponse = await client.GetAsync("api/route?pageNumber=1&pageSize=10");
        var allRoutes = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<RouteDto>>>();
        Console.WriteLine($"Total routes: {allRoutes?.Data?.TotalCount}");

        // Get a specific route by ID
        var getByIdResponse = await client.GetAsync("api/route/1");
        var singleRoute = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Route: {singleRoute?.Data?.Name}");

        // Update a route
        var updateRoute = new RouteDto
        {
            Name = "Updated Downtown Delivery Route",
            Description = "Updated delivery route description"
        };
        var updateResponse = await client.PutAsJsonAsync("api/route/1", updateRoute);
        var updatedRoute = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Updated route: {updatedRoute?.Data?.Name}");

        // Calculate route metrics
        var calculateResponse = await client.PostAsync("api/route/1/calculate", null);
        var routeMetrics = await calculateResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Console.WriteLine($"Route calculation: {routeMetrics?.Message}");

        // Delete a route
        var deleteResponse = await client.DeleteAsync("api/route/1");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}

## HttpContextExtensions

`HttpContextExtensions` provides a collection of helper methods for working with `HttpContext` instances. It simplifies retrieving request information (such as client IP, user‑agent, URLs, and headers), checking request characteristics (secure, AJAX, JSON, form), and manipulating the response (setting headers, status codes, content type, and caching directives).

### Usage Example

```csharp
using System;
using Microsoft.AspNetCore.Http;
using SignalRMapRealtime.Utilities;

public class Example
{
    public void ProcessRequest(HttpContext context)
    {
        // Retrieve request information
        string ip = context.GetClientIpAddress();
        string userAgent = context.GetUserAgent();
        string? referer = context.GetReferer();
        string? origin = context.GetOrigin();
        bool isSecure = context.IsSecure();
        string fullUrl = context.GetFullUrl();
        string baseUrl = context.GetBaseUrl();

        // Header utilities
        bool hasAuthHeader = context.HasHeader("Authorization");
        string? authHeader = context.GetHeader("Authorization");
        bool isAjax = context.IsAjaxRequest();

        // Content‑type checks
        string? contentType = context.GetContentType();
        bool isJson = context.IsJsonRequest();
        bool isForm = context.IsFormRequest();

        // Method and query parameters
        string method = context.GetMethod();
        var parameters = context.GetParameters();

        // Manipulate response
        context.SetHeader("X-Custom-Header", "MyValue");
        context.SetStatusCode(StatusCodes.Status200OK);
        context.SetContentType("application/json");
        context.DisableCaching();               // or context.SetCacheControl(60);

        // Continue with response writing...
    }
}

## RouteController

`RouteController` is an API controller that manages route entities for vehicle tracking and delivery path optimization. It provides endpoints for creating, reading, updating, and deleting routes, as well as calculating route metrics like distance and estimated travel time. Routes consist of sequences of waypoints that vehicles follow to complete deliveries or service calls.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new route
        var newRoute = new RouteDto
        {
            Name = "Downtown Delivery Route",
            Description = "Delivery route through city center"
        };

        var createResponse = await client.PostAsJsonAsync("api/route", newRoute);
        var createdRoute = await createResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Created route: {createdRoute?.Data?.Name} with ID: {createdRoute?.Data?.Id}");

        // Get all routes with pagination
        var getAllResponse = await client.GetAsync("api/route?pageNumber=1&pageSize=10");
        var allRoutes = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<RouteDto>>>();
        Console.WriteLine($"Total routes: {allRoutes?.Data?.TotalCount}");

        // Get a specific route by ID
        var getByIdResponse = await client.GetAsync("api/route/1");
        var singleRoute = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Route: {singleRoute?.Data?.Name}");

        // Update a route
        var updateRoute = new RouteDto
        {
            Name = "Updated Downtown Delivery Route",
            Description = "Updated delivery route description"
        };
        var updateResponse = await client.PutAsJsonAsync("api/route/1", updateRoute);
        var updatedRoute = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<RouteDto>>();
        Console.WriteLine($"Updated route: {updatedRoute?.Data?.Name}");

        // Calculate route metrics
        var calculateResponse = await client.PostAsync("api/route/1/calculate", null);
        var routeMetrics = await calculateResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Console.WriteLine($"Route calculation: {routeMetrics?.Message}");

        // Delete a route
        var deleteResponse = await client.DeleteAsync("api/route/1");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}

## ClaimsExtensions

`ClaimsExtensions` provides a set of extension methods for working with security claims. It simplifies extracting user information from `ClaimsPrincipal`, such as user ID, name, email, roles, and custom claims. 

### Usage Example

```csharp
using System;
using System.Security.Claims;

class Program
{
    static void Main()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.Email, "john@example.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("CustomClaim", "CustomValue")
        }));

        string? userId = principal.GetUserId();
        string? userName = principal.GetUserName();
        string? userEmail = principal.GetUserEmail();
        var userRoles = principal.GetUserRoles();
        bool isAdmin = principal.HasRole("Admin");
        bool hasCustomClaim = principal.HasClaim("CustomClaim");
        string? customClaimValue = principal.GetClaimValue("CustomClaim");

        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"User Name: {userName}");
        Console.WriteLine($"User Email: {userEmail}");
        Console.WriteLine($"User Roles: {string.Join(", ", userRoles)}");
        Console.WriteLine($"Is Admin: {isAdmin}");
        Console.WriteLine($"Has Custom Claim: {hasCustomClaim}");
        Console.WriteLine($"Custom Claim Value: {customClaimValue}");
    }
}
```