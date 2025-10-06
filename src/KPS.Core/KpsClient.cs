using Microsoft.Extensions.Logging;
using KPS.Core.Models;
using KPS.Core.Services;

namespace KPS.Core;

/// <summary>
/// Main KPS client implementation for querying Turkey's Population and Citizenship Affairs services
/// </summary>
public class KpsClient : IKpsClient
{
    private readonly IStsService _stsService;
    private readonly ISoapService _soapService;
    private readonly ILogger<KpsClient> _logger;
    private readonly KpsOptions _options;

    public KpsClient(
        IStsService stsService,
        ISoapService soapService,
        ILogger<KpsClient> logger,
        KpsOptions options)
    {
        _stsService = stsService;
        _soapService = soapService;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Performs a query to the KPS service
    /// </summary>
    public async Task<QueryResult> DoQueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting KPS query for TC: {TCNo}", request.TCNo);

            // Validate request
            ValidateRequest(request);

            // Get security token from STS
            _logger.LogDebug("Getting security token from STS");
            var samlToken = await _stsService.GetSecurityTokenAsync(
                _options.Username, 
                _options.Password, 
                cancellationToken);

            // Send SOAP request to KPS service
            _logger.LogDebug("Sending SOAP request to KPS service");
            var result = await _soapService.SendSoapRequestAsync(request, samlToken, cancellationToken);

            _logger.LogInformation("KPS query completed for TC: {TCNo}, Status: {Status}", 
                request.TCNo, result.Status);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("KPS query was cancelled for TC: {TCNo}", request.TCNo);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KPS query failed for TC: {TCNo}", request.TCNo);
            throw;
        }
    }

    /// <summary>
    /// Validates the query request
    /// </summary>
    private static void ValidateRequest(QueryRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.TCNo))
            throw new ArgumentException("TCNo is required", nameof(request.TCNo));

        if (request.TCNo.Length != 11)
            throw new ArgumentException("TCNo must be exactly 11 digits", nameof(request.TCNo));

        if (!request.TCNo.All(char.IsDigit))
            throw new ArgumentException("TCNo must contain only digits", nameof(request.TCNo));

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("FirstName is required", nameof(request.FirstName));

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("LastName is required", nameof(request.LastName));

        if (string.IsNullOrWhiteSpace(request.BirthYear))
            throw new ArgumentException("BirthYear is required", nameof(request.BirthYear));

        if (string.IsNullOrWhiteSpace(request.BirthMonth))
            throw new ArgumentException("BirthMonth is required", nameof(request.BirthMonth));

        if (string.IsNullOrWhiteSpace(request.BirthDay))
            throw new ArgumentException("BirthDay is required", nameof(request.BirthDay));

        // Validate birth date format
        if (request.BirthYear.Length != 4 || !request.BirthYear.All(char.IsDigit))
            throw new ArgumentException("BirthYear must be 4 digits", nameof(request.BirthYear));

        if (request.BirthMonth.Length != 2 || !request.BirthMonth.All(char.IsDigit))
            throw new ArgumentException("BirthMonth must be 2 digits", nameof(request.BirthMonth));

        if (request.BirthDay.Length != 2 || !request.BirthDay.All(char.IsDigit))
            throw new ArgumentException("BirthDay must be 2 digits", nameof(request.BirthDay));

        // Validate birth month range
        if (int.Parse(request.BirthMonth) < 1 || int.Parse(request.BirthMonth) > 12)
            throw new ArgumentException("BirthMonth must be between 01 and 12", nameof(request.BirthMonth));

        // Validate birth day range
        if (int.Parse(request.BirthDay) < 1 || int.Parse(request.BirthDay) > 31)
            throw new ArgumentException("BirthDay must be between 01 and 31", nameof(request.BirthDay));
    }
}
