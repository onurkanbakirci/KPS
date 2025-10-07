using System.ComponentModel.DataAnnotations;

namespace KPS.Core.Models.Request;

/// <summary>
/// Represents a citizen verification request to the KPS service
/// </summary>
public class CitizenVerificationRequest
{
    /// <summary>
    /// Turkish Republic Identity Number (TC Kimlik No)
    /// </summary>
    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string TCNo { get; set; } = string.Empty;

    /// <summary>
    /// First name of the person
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the person
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Birth year (4 digits)
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string BirthYear { get; set; } = string.Empty;

    /// <summary>
    /// Birth month (2 digits, zero-padded)
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string BirthMonth { get; set; } = string.Empty;

    /// <summary>
    /// Birth day (2 digits, zero-padded)
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string BirthDay { get; set; } = string.Empty;
}
