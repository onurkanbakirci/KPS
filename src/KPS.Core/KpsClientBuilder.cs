using Microsoft.Extensions.Logging;
using KPS.Core.Models;
using KPS.Core.Services;
using KPS.Core.Services.Abstract;
using KPS.Core.Clients;
using KPS.Core.Clients.Abstract;

namespace KPS.Core;

/// <summary>
/// Builder for creating KPS client instances with fluent configuration
/// </summary>
public class KpsClientBuilder
{
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _stsEndpoint = "https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13";
    private string _kpsEndpoint = "https://kpsv2.nvi.gov.tr/Services/RoutingService.svc";
    private int _timeoutSeconds = 30;
    private bool _includeRawResponse = false;
    private HttpClient? _httpClient;
    private ILogger<KpsClient>? _logger;

    /// <summary>
    /// Sets the username for KPS service authentication
    /// </summary>
    /// <param name="username">KPS service username</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        _username = username;
        return this;
    }

    /// <summary>
    /// Sets the password for KPS service authentication
    /// </summary>
    /// <param name="password">KPS service password</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        _password = password;
        return this;
    }

    /// <summary>
    /// Sets the STS (Security Token Service) endpoint URL
    /// </summary>
    /// <param name="endpoint">STS endpoint URL</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithStsEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("STS endpoint cannot be null or empty", nameof(endpoint));

        _stsEndpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the KPS service endpoint URL
    /// </summary>
    /// <param name="endpoint">KPS service endpoint URL</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithKpsEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("KPS endpoint cannot be null or empty", nameof(endpoint));

        _kpsEndpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the request timeout in seconds
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
            throw new ArgumentException("Timeout must be greater than 0", nameof(timeoutSeconds));

        _timeoutSeconds = timeoutSeconds;
        return this;
    }

    /// <summary>
    /// Sets whether to include raw SOAP response in results
    /// </summary>
    /// <param name="includeRawResponse">Whether to include raw response</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithRawResponse(bool includeRawResponse)
    {
        _includeRawResponse = includeRawResponse;
        return this;
    }

    /// <summary>
    /// Sets a custom HTTP client to use
    /// </summary>
    /// <param name="httpClient">Custom HTTP client</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        return this;
    }

    /// <summary>
    /// Sets a custom logger to use
    /// </summary>
    /// <param name="logger">Custom logger</param>
    /// <returns>Builder instance for method chaining</returns>
    public KpsClientBuilder WithLogger(ILogger<KpsClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        return this;
    }

    /// <summary>
    /// Builds and returns the configured KPS client
    /// </summary>
    /// <returns>Configured KPS client instance</returns>
    public IKpsClient Build()
    {
        if (string.IsNullOrWhiteSpace(_username))
            throw new InvalidOperationException("Username must be set before building the client");

        if (string.IsNullOrWhiteSpace(_password))
            throw new InvalidOperationException("Password must be set before building the client");

        var options = new KpsOptions
        {
            Username = _username,
            Password = _password,
            StsEndpoint = _stsEndpoint,
            KpsEndpoint = _kpsEndpoint,
            TimeoutSeconds = _timeoutSeconds,
            IncludeRawResponse = _includeRawResponse
        };

        var httpClient = _httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
        };

        var logger = _logger ?? new LoggerFactory().CreateLogger<KpsClient>();

        var stsService = new StsService(httpClient, options);
        var soapService = new SoapService(httpClient, options);

        return new KpsClient(stsService, soapService, logger, options);
    }
}
