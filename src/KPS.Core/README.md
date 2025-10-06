# KPS Client for .NET

A .NET client library for Turkey's Population and Citizenship Affairs (KPS) v2 services. This library provides a simple and easy-to-use interface for querying KPS services using WS-Trust authentication and HMAC-SHA1 signed SOAP requests.

## Features

- **WS-Trust (STS) Authentication**: Automatically handles the authentication flow with the Security Token Service
- **HMAC-SHA1 SOAP Signing**: Signs SOAP messages with HMAC-SHA1 (compatible with KPS services)
- **Response Parsing**: Parses SOAP responses into meaningful `QueryResult` structures
- **Dependency Injection Support**: Full integration with .NET's dependency injection container
- **Configuration Support**: Easy configuration through appsettings.json or code
- **Async/Await Support**: Modern async programming patterns
- **Logging Integration**: Built-in logging support with Microsoft.Extensions.Logging

## Installation

Add the package reference to your project:

```xml
<PackageReference Include="KPS.Core" Version="1.0.0" />
```

Or using the .NET CLI:

```bash
dotnet add package KPS.Core
```

## Quick Start

### Using the Builder Pattern

```csharp
using KPS.Core;
using KPS.Core.Models;

// Create a client instance using builder pattern
var client = new KpsClientBuilder()
    .WithUsername("YOUR_USERNAME")
    .WithPassword("YOUR_PASSWORD")
    .WithTimeout(30)
    .WithRawResponse(false)
    .Build();

// Create a query request
var request = new QueryRequest
{
    TCNo = "99999999999",
    FirstName = "JOHN",
    LastName = "DOE",
    BirthYear = "1990",
    BirthMonth = "01",
    BirthDay = "01"
};

// Perform the query
var result = await client.DoQueryAsync(request);

if (result.Status)
{
    Console.WriteLine($"Query successful: {result.Aciklama}");
}
else
{
    Console.WriteLine($"Query failed: {result.Aciklama}");
}
```

### Using Dependency Injection

1. **Register services in your Startup.cs or Program.cs:**

```csharp
using KPS.Client.Extensions;

// Option 1: Configure in code
builder.Services.AddKpsClient(options =>
{
    options.Username = "YOUR_USERNAME";
    options.Password = "YOUR_PASSWORD";
    options.TimeoutSeconds = 30;
    options.IncludeRawResponse = false;
});

// Option 2: Configure from appsettings.json
builder.Services.AddKpsClient(builder.Configuration);
```

2. **Add configuration to appsettings.json:**

```json
{
  "KpsClient": {
    "Username": "YOUR_USERNAME",
    "Password": "YOUR_PASSWORD",
    "TimeoutSeconds": 30,
    "IncludeRawResponse": false,
    "StsEndpoint": "https://tckimlik.nvi.gov.tr/Service/STS",
    "KpsEndpoint": "https://tckimlik.nvi.gov.tr/Service/KPSPublic"
  }
}
```

3. **Inject and use the client:**

```csharp
using KPS.Client;
using KPS.Client.Models;

public class MyService
{
    private readonly IKpsClient _kpsClient;

    public MyService(IKpsClient kpsClient)
    {
        _kpsClient = kpsClient;
    }

    public async Task<bool> ValidatePersonAsync(string tcNo, string firstName, string lastName, 
        string birthYear, string birthMonth, string birthDay)
    {
        var request = new QueryRequest
        {
            TCNo = tcNo,
            FirstName = firstName,
            LastName = lastName,
            BirthYear = birthYear,
            BirthMonth = birthMonth,
            BirthDay = birthDay
        };

        var result = await _kpsClient.DoQueryAsync(request);
        return result.Status;
    }
}
```

## API Reference

### QueryRequest

Represents a query request to the KPS service:

```csharp
public class QueryRequest
{
    public string TCNo { get; set; }        // Turkish Republic Identity Number (11 digits)
    public string FirstName { get; set; }   // First name
    public string LastName { get; set; }    // Last name
    public string BirthYear { get; set; }   // Birth year (4 digits)
    public string BirthMonth { get; set; }  // Birth month (2 digits, zero-padded)
    public string BirthDay { get; set; }    // Birth day (2 digits, zero-padded)
}
```

### QueryResult

Represents the result of a KPS query:

```csharp
public class QueryResult
{
    public bool Status { get; set; }                    // Whether the query was successful
    public int Code { get; set; }                       // Result code (1=Success, 2=Error/NotFound, 3=Deceased)
    public string Aciklama { get; set; }                // Description of the result
    public string Person { get; set; }                  // Person type (tc_vatandasi, yabanci, mavi)
    public Dictionary<string, object> Extra { get; set; } // Additional data
    public string Raw { get; set; }                     // Raw SOAP response (if enabled)
}
```

### KpsOptions

Configuration options for the KPS client:

```csharp
public class KpsOptions
{
    public string Username { get; set; }           // KPS service username
    public string Password { get; set; }           // KPS service password
    public string StsEndpoint { get; set; }        // STS endpoint URL
    public string KpsEndpoint { get; set; }        // KPS service endpoint URL
    public int TimeoutSeconds { get; set; }        // Request timeout in seconds
    public bool IncludeRawResponse { get; set; }   // Whether to include raw SOAP response
}
```

## Error Handling

The library throws appropriate exceptions for different error scenarios:

- `ArgumentException`: Invalid request parameters
- `InvalidOperationException`: Service communication errors
- `OperationCanceledException`: Request cancellation
- `HttpRequestException`: HTTP communication errors

## Security Notes

- The library uses HMAC-SHA1 for SOAP message signing as required by KPS services
- Store credentials securely and never hardcode them in your application
- Use environment variables or secure configuration management in production
- The `Raw` field in responses may contain sensitive information - handle with care

## Logging

The library integrates with Microsoft.Extensions.Logging. Configure logging in your application:

```csharp
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
