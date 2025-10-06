# KPS.Core

A .NET client library for Turkey's Population and Citizenship Affairs (KPS) v2 services. This library provides easy access to KPS services using WS-Trust authentication and HMAC-SHA1 signed SOAP requests.

## What is this library?

KPS.Core is a .NET implementation of the Go KPS client library that allows you to query Turkey's official population and citizenship database. It handles the complex authentication flow with WS-Trust (STS) and SOAP message signing automatically.

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
var result = await client.DoQueryAsync(request);

if (result.Status)
{
    Console.WriteLine("✅ Person validation successful!");
}
else
{
    Console.WriteLine("❌ Person validation failed");
}
```

## Development

### GitHub Actions Setup

This project uses GitHub Actions for automated releases and NuGet publishing. To set up the workflows:

1. **Release Please Workflow**: Automatically creates pull requests for version bumps based on conventional commits
2. **NuGet Publishing Workflow**: Automatically publishes packages to NuGet when releases are created

#### Required Secrets

Add the following secret to your GitHub repository settings:

- `NUGET_API_KEY`: Your NuGet API key for publishing packages
  - Get your API key from [NuGet.org](https://www.nuget.org/account/apikeys)
  - Go to your repository → Settings → Secrets and variables → Actions
  - Add a new repository secret named `NUGET_API_KEY`

#### Workflow Process

1. Make changes to the code and commit with conventional commit messages (e.g., `feat:`, `fix:`, `chore:`)
2. Push to the `main` branch
3. Release Please will create a pull request with version bump and changelog updates
4. Review and merge the pull request
5. A new release will be created automatically
6. The NuGet publishing workflow will trigger and publish the package to NuGet.org

## License

MIT License
