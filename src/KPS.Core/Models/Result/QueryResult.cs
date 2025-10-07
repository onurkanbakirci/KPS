namespace KPS.Core.Models.Result;

/// <summary>
/// Represents the result of a KPS query
/// </summary>
public class QueryResult
{
    /// <summary>
    /// Indicates whether the query was successful
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Result code indicating the outcome of the operation
    /// </summary>
    public ResultCodes Code { get; set; }

    /// <summary>
    /// Description of the result
    /// </summary>
    public string Aciklama { get; set; } = string.Empty;

    /// <summary>
    /// Person type: tc_vatandasi, yabanci, mavi
    /// </summary>
    public string Person { get; set; } = string.Empty;

    /// <summary>
    /// Additional data returned by the service
    /// </summary>
    public Dictionary<string, object> Extra { get; set; } = new();

    /// <summary>
    /// Raw SOAP response for debugging purposes
    /// </summary>
    public string Raw { get; set; } = string.Empty;
}

/// <summary>
/// Person types returned by KPS service
/// </summary>
public static class PersonTypes
{
    public const string TurkishCitizen = "tc_vatandasi";
    public const string Foreigner = "yabanci";
    public const string BlueCard = "mavi";
}

/// <summary>
/// Result codes returned by KPS service
/// </summary>
public enum ResultCodes
{
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Success = 1,

    /// <summary>
    /// Error occurred or person not found
    /// </summary>
    ErrorOrNotFound = 2,

    /// <summary>
    /// Person is deceased
    /// </summary>
    Deceased = 3
}
