using KPS.Core.Models;

namespace KPS.Core.Services;

/// <summary>
/// Interface for SOAP service operations
/// </summary>
public interface ISoapService
{
    /// <summary>
    /// Sends a signed SOAP request to the KPS service
    /// </summary>
    /// <param name="request">Query request</param>
    /// <param name="samlToken">SAML token for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query result</returns>
    Task<QueryResult> SendSoapRequestAsync(QueryRequest request, string samlToken, CancellationToken cancellationToken = default);
}
