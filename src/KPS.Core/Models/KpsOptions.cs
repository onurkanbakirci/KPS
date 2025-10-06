namespace KPS.Core.Models;

/// <summary>
/// Configuration options for the KPS client
/// </summary>
public class KpsOptions
{
    /// <summary>
    /// Username for KPS service authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for KPS service authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// STS (Security Token Service) endpoint URL
    /// </summary>
    public string StsEndpoint { get; set; } = "https://tckimlik.nvi.gov.tr/Service/STS";

    /// <summary>
    /// KPS service endpoint URL
    /// </summary>
    public string KpsEndpoint { get; set; } = "https://tckimlik.nvi.gov.tr/Service/KPSPublic";

    /// <summary>
    /// Request timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to include raw SOAP response in results (default: false)
    /// </summary>
    public bool IncludeRawResponse { get; set; } = false;
}
