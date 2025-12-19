namespace KPS.Core.Models.Result;

/// <summary>
/// Represents detailed information about a person from KPS service
/// </summary>
public class PersonInfo
{
    /// <summary>
    /// Type of person record: tc_vatandasi, yabanci, or mavi
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Turkish ID number (TC Kimlik No) or foreign identity number
    /// </summary>
    public string IdentityNumber { get; set; } = string.Empty;

    /// <summary>
    /// First name (Ad)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name (Soyad)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nationality (Uyruk) - for foreign nationals
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Birth date in ISO format (YYYY-MM-DD, YYYY-MM, or YYYY)
    /// </summary>
    public string? BirthDate { get; set; }

    /// <summary>
    /// Death date in ISO format (YYYY-MM-DD, YYYY-MM, or YYYY) - null if person is alive
    /// </summary>
    public string? DeathDate { get; set; }

    /// <summary>
    /// Full name convenience property
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Indicates if this is a Turkish citizen record
    /// </summary>
    public bool IsTurkishCitizen => Type == PersonTypes.TurkishCitizen;

    /// <summary>
    /// Indicates if this is a foreigner record
    /// </summary>
    public bool IsForeigner => Type == PersonTypes.Foreigner;

    /// <summary>
    /// Indicates if this is a blue card holder record
    /// </summary>
    public bool IsBlueCardHolder => Type == PersonTypes.BlueCard;

    /// <summary>
    /// Indicates if the person is deceased based on death date
    /// </summary>
    public bool IsDeceased => !string.IsNullOrEmpty(DeathDate);
}

