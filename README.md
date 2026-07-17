// ... (rest of the file remains the same)

## ValidationExtensions

`ValidationExtensions` provides a comprehensive set of extension methods for validating common data types and formats. It includes utilities for checking if a string is a valid email, phone number, URL, IP address, GUID, or if it matches a specific pattern. Additionally, it offers methods for verifying if a value falls within a range, if a string has a certain length, or if a collection contains elements.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.Utilities;

class Program
{
    static void Main()
    {
        string email = "user@example.com";
        bool isValid = email.IsValidEmail();
        Console.WriteLine($"Is valid email: {isValid}");

        string phoneNumber = "+1 123-456-7890";
        bool isValidPhone = phoneNumber.IsValidPhoneNumber();
        Console.WriteLine($"Is valid phone number: {isValidPhone}");

        string url = "https://www.example.com";
        bool isValidUrl = url.IsValidUrl();
        Console.WriteLine($"Is valid URL: {isValidUrl}");

        string ipAddress = "192.168.1.1";
        bool isValidIp = ipAddress.IsValidIpAddress();
        Console.WriteLine($"Is valid IP address: {isValidIp}");

        string guid = "01234567-89ab-cdef-0123-456789abcdef";
        bool isValidGuid = guid.IsValidGuid();
        Console.WriteLine($"Is valid GUID: {isValidGuid}");

        string alphanumeric = "Hello123";
        bool isAlphanumeric = alphanumeric.IsAlphanumeric();
        Console.WriteLine($"Is alphanumeric: {isAlphanumeric}");

        string password = "P@ssw0rd!";
        bool isStrong = password.IsStrongPassword();
        Console.WriteLine($"Is strong password: {isStrong}");

        int value = 5;
        bool isInValueRange = value.IsInRange(1, 10);
        Console.WriteLine($"Is in range: {isInValueRange}");

        string lengthTest = "Hello";
        bool isLengthInRange = lengthTest.IsLengthInRange(5, 10);
        Console.WriteLine($"Is length in range: {isLengthInRange}");

        var collection = new[] { 1, 2, 3 };
        bool hasElements = collection.HasElements();
        Console.WriteLine($"Has elements: {hasElements}");
        bool hasExactlyTwo = collection.HasExactly(2);
        Console.WriteLine($"Has exactly 2: {hasExactlyTwo}");
        bool hasAtLeastTwo = collection.HasAtLeast(2);
        Console.WriteLine($"Has at least 2: {hasAtLeastTwo}");

        double positiveValue = 5.0;
        bool isPositive = positiveValue.IsPositive<double>();
        Console.WriteLine($"Is positive: {isPositive}");

        double negativeValue = -5.0;
        bool isNegative = negativeValue.IsNegative<double>();
        Console.WriteLine($"Is negative: {isNegative}");

        double nanValue = double.NaN;
        bool isNaN = nanValue.IsNaN();
        Console.WriteLine($"Is NaN: {isNaN}");

        double infinityValue = double.PositiveInfinity;
        bool isInfinite = infinityValue.IsInfinite();
        Console.WriteLine($"Is infinite: {isInfinite}");

        decimal percentage = 50m;
        bool isValidPercentage = percentage.IsValidPercentage();
        Console.WriteLine($"Is valid percentage: {isValidPercentage}");

        string pattern = "Hello*";
        bool matches = "Hello World!".MatchesPattern(pattern);
        Console.WriteLine($"Matches pattern: {matches}");
    }
}
```

## DateTimeExtensions

`DateTimeExtensions` offers a collection of helper methods for common date and time calculations, such as friendly time spans, period boundaries, rounding, range checks, age calculation, Unix timestamp conversion, and ISO‑8601 formatting. These extensions simplify working with `DateTime` values throughout the tracking application.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.Utilities;

class Program
{
    static void Main()
    {
        DateTime now = DateTime.UtcNow;

        // Friendly description of how long ago a timestamp occurred
        string friendly = now.AddMinutes(-5).ToFriendlyTimeSpan();
        Console.WriteLine($"Friendly: {friendly}");

        // Duration between two moments
        TimeSpan duration = now.Duration(now.AddHours(2));
        Console.WriteLine($"Duration (hours): {duration.TotalHours}");

        // Start / end of day
        DateTime startDay = now.StartOfDay();
        DateTime endDay = now.EndOfDay();
        Console.WriteLine($"Day starts at {startDay:O}, ends at {endDay:O}");

        // Start / end of week
        DateTime startWeek = now.StartOfWeek();
        DateTime endWeek = now.EndOfWeek();
        Console.WriteLine($"Week starts on {startWeek:yyyy-MM-dd}, ends on {endWeek:yyyy-MM-dd}");

        // Start / end of month
        DateTime startMonth = now.StartOfMonth();
        DateTime endMonth = now.EndOfMonth();
        Console.WriteLine($"Month starts {startMonth:yyyy-MM-dd}, ends {endMonth:yyyy-MM-dd}");

        // Start / end of year
        DateTime startYear = now.StartOfYear();
        DateTime endYear = now.EndOfYear();
        Console.WriteLine($"Year starts {startYear:yyyy-MM-dd}, ends {endYear:yyyy-MM-dd}");

        // Past / future / today checks
        Console.WriteLine($"Is past? {now.AddDays(-1).IsPast()}");
        Console.WriteLine($"Is future? {now.AddDays(1).IsFuture()}");
        Console.WriteLine($"Is today? {now.IsToday()}");

        // Rounding
        DateTime roundedMinute = now.RoundToNearestMinute(15);
        DateTime roundedSecond = now.RoundToNearestSecond();
        Console.WriteLine($"Rounded to 15‑minute interval: {roundedMinute:O}");
        Console.WriteLine($"Rounded to nearest second: {roundedSecond:O}");

        // Between check
        bool between = now.IsBetween(now.AddHours(-1), now.AddHours(1));
        Console.WriteLine($"Is now between one hour ago and one hour ahead? {between}");

        // Age calculation
        DateTime birthDate = new DateTime(1990, 4, 15);
        int age = birthDate.GetAgeInYears();
        Console.WriteLine($"Age: {age}");

        // Unix timestamp conversion
        long unix = now.ToUnixTimestamp();
        DateTime fromUnix = unix.FromUnixTimestamp();
        Console.WriteLine($"Unix: {unix}, back to DateTime: {fromUnix:O}");

        // ISO‑8601 string
        string iso = now.ToIso8601String();
        Console.WriteLine($"ISO‑8601: {iso}");
    }
}
```
