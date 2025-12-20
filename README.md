# KPS.Core

A .NET client library for Turkey's Population and Citizenship Affairs (KPS) v2 services. This library provides easy access to KPS services using WS-Trust authentication and HMAC-SHA1 signed SOAP requests.

## What is this library?

KPS.Core is a .NET implementation of the KPS that allows you to query Turkey's official population and citizenship database. It handles the complex authentication flow with WS-Trust (STS) and SOAP message signing automatically.

## Installation

```bash
dotnet add package KPS.Core
```

## Usage

### Basic Example (Minimal - Only Required Parameters)

```csharp
using KPS.Core;
using KPS.Core.Models.Request;

// Create client using builder pattern (only username and password required)
var client = new KpsClientBuilder()
    .WithUsername("YOUR_USERNAME")
    .WithPassword("YOUR_PASSWORD")
    .Build();

// Create citizen verification request
var request = new CitizenVerificationRequest
{
    TCNo = "12345678901",
    FirstName = "JOHN",
    LastName = "DOE",
    BirthYear = "1990",
    BirthMonth = "01",
    BirthDay = "01"
};

// Verify citizen identity
var result = await client.VerifyCitizenAsync(request);

if (result.Status)
{
    Console.WriteLine($"✅ Person validation successful! {result.Person?.FullName}");
}
else
{
    Console.WriteLine("❌ Person validation failed");
}
```

### Advanced Example (With Custom Options)

```csharp
// Create client with custom configuration (all optional)
var client = new KpsClientBuilder()
    .WithUsername("YOUR_USERNAME")
    .WithPassword("YOUR_PASSWORD")
    .WithTimeout(30)                    // Optional: Request timeout in seconds (default: 30)
    .WithRawResponse(false)             // Optional: Include raw SOAP response (default: false)
    .WithStsEndpoint("https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13")  // Optional: Custom STS endpoint
    .WithKpsEndpoint("https://kpsv2.nvi.gov.tr/Services/RoutingService.svc")  // Optional: Custom KPS endpoint
    .Build();
```

## License

MIT License