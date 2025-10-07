using KPS.Core.Models.Request;
using KPS.Core.Models.Result;

namespace KPS.Core.Services.Abstract;

/// <summary>
/// Interface for SOAP service operations
/// </summary>
public interface ISoapService
{
    /// <summary>
    /// Sends a signed SOAP request to the KPS service
    /// </summary>
    /// <param name="request">Citizen verification request</param>
    /// <param name="samlToken">SAML token for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    Task<QueryResult> SendSoapRequestAsync(CitizenVerificationRequest request, string samlToken, CancellationToken cancellationToken = default);
}
