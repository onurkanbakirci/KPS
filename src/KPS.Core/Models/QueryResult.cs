namespace KPS.Core.Models;

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
    /// Result code: 1 = Success, 2 = Error/Not Found, 3 = Deceased
    /// </summary>
    public int Code { get; set; }

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
public static class ResultCodes
{
    public const int Success = 1;
    public const int ErrorOrNotFound = 2;
    public const int Deceased = 3;
}
