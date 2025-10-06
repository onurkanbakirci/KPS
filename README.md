# KPS.Core

A .NET client library for Turkey's Population and Citizenship Affairs (KPS) v2 services. This library provides easy access to KPS services using WS-Trust authentication and HMAC-SHA1 signed SOAP requests.

## What is this library?

KPS.Core is a .NET implementation of the KPS that allows you to query Turkey's official population and citizenship database. It handles the complex authentication flow with WS-Trust (STS) and SOAP message signing automatically.

## Installation

```bash
dotnet add package KPS.Core
```

## Minimal Usage Example

```csharp
using KPS.Core;
using KPS.Core.Models;

// Create client using builder pattern with all options
var client = new KpsClientBuilder()
    .WithUsername("YOUR_USERNAME")
    .WithPassword("YOUR_PASSWORD")
    .WithTimeout(30)                    // Request timeout in seconds
    .WithRawResponse(false)             // Include raw SOAP response
    .WithStsEndpoint("https://tckimlik.nvi.gov.tr/Service/STS")  // Custom STS endpoint
    .WithKpsEndpoint("https://tckimlik.nvi.gov.tr/Service/KPSPublic")  // Custom KPS endpoint
    .Build();

// Create query request
var request = new QueryRequest
{
    TCNo = "12345678901",
    FirstName = "JOHN",
    LastName = "DOE",
    BirthYear = "1990",
    BirthMonth = "01",
    BirthDay = "01"
};

// Perform the query
var result = await client.QueryAsync(request);

if (result.Status)
{
    Console.WriteLine("✅ Person validation successful!");
}
else
{
    Console.WriteLine("❌ Person validation failed");
}
```

## License

MIT License