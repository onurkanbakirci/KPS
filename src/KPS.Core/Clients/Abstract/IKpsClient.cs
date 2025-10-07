using KPS.Core.Models.Request;
using KPS.Core.Models.Result;

namespace KPS.Core.Clients.Abstract;

/// <summary>
/// Interface for KPS (Population and Citizenship Affairs) client
/// </summary>
public interface IKpsClient
{
    /// <summary>
    /// Verifies a citizen's identity using the KPS service
    /// </summary>
    /// <param name="request">Citizen verification request containing person information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    Task<QueryResult> VerifyCitizenAsync(CitizenVerificationRequest request, CancellationToken cancellationToken = default);
}
