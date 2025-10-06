using KPS.Core.Models;

namespace KPS.Core;

/// <summary>
/// Interface for KPS (Population and Citizenship Affairs) client
/// </summary>
public interface IKpsClient
{
    /// <summary>
    /// Performs a query to the KPS service
    /// </summary>
    /// <param name="request">Query request containing person information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query result</returns>
    Task<QueryResult> DoQueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
}
