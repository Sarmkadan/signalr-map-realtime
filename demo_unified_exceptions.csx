#!/usr/bin/env dotnet-script

// Demonstration script showing the unified exception-to-JSON envelope functionality
// This script demonstrates how the new IApiErrorSerializable interface provides
// consistent error envelope structure across different exception types

#r "System.Text.Json"

using System;
using System.Text.Json;
using SignalRMapRealtime.Exceptions;

Console.WriteLine("=== Unified Exception-to-JSON Envelope Demo ===\n");

// Demo 1: ValidationException with standardized error envelope
Console.WriteLine("1. ValidationException with standardized error envelope:");
try
{
    throw new ValidationException("Vehicle validation failed", new[] { "Vehicle plate is required", "Vehicle status must be valid" });
}
catch (ValidationException ex)
{
    var error = ex.ToApiError();
    var errorJson = error.ToErrorResponse();

    Console.WriteLine("Error Code: " + error.GetErrorCode());
    Console.WriteLine("HTTP Status: " + error.GetHttpStatusCode());
    Console.WriteLine("Message: " + error.GetMessage());
    Console.WriteLine("JSON Envelope:");
    Console.WriteLine(errorJson);
    Console.WriteLine();
}

// Demo 2: LocationTrackingException (VehicleNotFoundException) with standardized error envelope
Console.WriteLine("2. VehicleNotFoundException with standardized error envelope:");
try
{
    throw new VehicleNotFoundException(12345, "Vehicle with ID 12345 was not found in the tracking system");
}
catch (VehicleNotFoundException ex)
{
    var error = ex.ToApiError();
    var errorJson = error.ToErrorResponse();

    Console.WriteLine("Error Code: " + error.GetErrorCode());
    Console.WriteLine("HTTP Status: " + error.GetHttpStatusCode());
    Console.WriteLine("Message: " + error.GetMessage());
    Console.WriteLine("Details: vehicleId=" + (ex as VehicleNotFoundException)?.VehicleId);
    Console.WriteLine("JSON Envelope:");
    Console.WriteLine(errorJson);
    Console.WriteLine();
}

// Demo 3: InvalidLocationException with standardized error envelope
Console.WriteLine("3. InvalidLocationException with standardized error envelope:");
try
{
    throw new InvalidLocationException(185.5, -275.3);
}
catch (InvalidLocationException ex)
{
    var error = ex.ToApiError();
    var errorJson = error.ToErrorResponse();

    Console.WriteLine("Error Code: " + error.GetErrorCode());
    Console.WriteLine("HTTP Status: " + error.GetHttpStatusCode());
    Console.WriteLine("Message: " + error.GetMessage());
    Console.WriteLine("Details: latitude=" + ex.Latitude + ", longitude=" + ex.Longitude);
    Console.WriteLine("JSON Envelope:");
    Console.WriteLine(errorJson);
    Console.WriteLine();
}

// Demo 4: Generic Exception with standardized error envelope
Console.WriteLine("4. Generic ArgumentException with standardized error envelope:");
try
{
    throw new ArgumentException("Invalid vehicle ID format: 'ABC123XYZ'", nameof(vehicleId));
}
catch (ArgumentException ex)
{
    var error = ex.ToApiError();
    var errorJson = error.ToErrorResponse();

    Console.WriteLine("Error Code: " + error.GetErrorCode());
    Console.WriteLine("HTTP Status: " + error.GetHttpStatusCode());
    Console.WriteLine("Message: " + error.GetMessage());
    Console.WriteLine("JSON Envelope:");
    Console.WriteLine(errorJson);
    Console.WriteLine();
}

// Demo 5: Comparison of old vs new approach
Console.WriteLine("5. Comparison - Old approach (inconsistent) vs New approach (consistent):");
Console.WriteLine("OLD APPROACH - Different structures for different exception types:");
Console.WriteLine("- ValidationException used ExceptionJsonExtensions.ToJson()");
Console.WriteLine("- LocationTrackingException used ExceptionJsonExtensions.ToJson()");
Console.WriteLine("- No standardized envelope structure");
Console.WriteLine("- No consistent HTTP status code mapping");
Console.WriteLine("- No shared interface for unified error handling\n");

Console.WriteLine("NEW APPROACH - Consistent structure across all exception types:");
Console.WriteLine("- All exceptions implement IApiErrorSerializable");
Console.WriteLine("- Standardized envelope: { errorCode, message, statusCode, timestamp, traceId, details }");
Console.WriteLine("- Consistent HTTP status code mapping per exception type");
Console.WriteLine("- Unified interface for controller error handling");
Console.WriteLine("- Type-specific error codes and details");
Console.WriteLine();

Console.WriteLine("=== Demo Complete ===");
Console.WriteLine("\nBenefits:");
Console.WriteLine("✓ Consistent API error responses across all exception types");
Console.WriteLine("✓ Programmatic error handling via standardized error codes");
Console.WriteLine("✓ Type-specific details in structured format");
Console.WriteLine("✓ Consistent HTTP status code mapping");
Console.WriteLine("✓ Unified interface for controller error handling");
Console.WriteLine("✓ Backward compatibility with existing code");