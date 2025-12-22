using KPS.Core;
using KPS.Core.Models.Request;

try
{
    // Create KPS client using builder pattern (minimal - only required parameters)
    var kpsClient = new KpsClientBuilder()
        .WithUsername("YOUR_USERNAME")
        .WithPassword("YOUR_PASSWORD")
        .Build();

    // Or with custom options (all optional):
    // var kpsClient = new KpsClientBuilder()
    //     .WithUsername("YOUR_USERNAME")
    //     .WithPassword("YOUR_PASSWORD")
    //     .WithTimeout(30)                    // Optional: Request timeout in seconds (default: 30)
    //     .WithRawResponse(false)             // Optional: Include raw SOAP response (default: false)
    //     .WithStsEndpoint("https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13")  // Optional: Custom STS endpoint
    //     .WithKpsEndpoint("https://kpsv2.nvi.gov.tr/Services/RoutingService.svc")  // Optional: Custom KPS endpoint
    //     .Build();

    // Create citizen verification request
    var request = new CitizenVerificationRequest
    {
        TCNo = "12345678901",              // Turkish Citizen Number
        FirstName = "JOHN",                // Must be uppercase
        LastName = "DOE",                  // Must be uppercase
        BirthYear = "1990",               
        BirthMonth = "01",                // Two digit month
        BirthDay = "01"                   // Two digit day
    };

    // Verify citizen identity
    Console.WriteLine("Verifying citizen identity...");
    var response = await kpsClient.VerifyCitizenAsync(request);

    // Handle the response
    if (response.Status)
    {
        Console.WriteLine($"✅ Person validation successful! {response.Person?.FullName}");
    }
    else
    {
        Console.WriteLine("❌ Person validation failed");
    }    
}
catch (Exception ex)
{
    Console.WriteLine($"❌ An error occurred: {ex.Message}");
    Environment.Exit(1);
}

