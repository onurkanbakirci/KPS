using KPS.Core.Models;

namespace KPS.Core.Services;

/// <summary>
/// Interface for Security Token Service operations
/// </summary>
public interface IStsService
{
    /// <summary>
    /// Gets a security token from the STS service
    /// </summary>
    /// <param name="username">Username for authentication</param>
    /// <param name="password">Password for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SAML token string</returns>
    Task<string> GetSecurityTokenAsync(string username, string password, CancellationToken cancellationToken = default);
}
