using KPS.Core;
using KPS.Core.Models;

var kpsClient = new KpsClientBuilder()
    .WithUsername("YOUR_USERNAME")
    .WithPassword("YOUR_PASSWORD")
    .WithTimeout(30)
    .WithRawResponse(false)
    .Build();

var request = new QueryRequest
{
    TCNo = "12345678901",
    FirstName = "JOHN",
    LastName = "DOE",
    BirthYear = "1990",
    BirthMonth = "01",
    BirthDay = "01"
};

var httpResponse = await kpsClient.QueryAsync(request);

Console.WriteLine(httpResponse.Status);