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

    /// <summary>
    /// Initializes a new instance of the <see cref="CitizenVerificationRequest"/> class
    /// </summary>
    public CitizenVerificationRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CitizenVerificationRequest"/> class with all required parameters
    /// </summary>
    /// <param name="tcNo">Turkish Republic Identity Number (11 digits)</param>
    /// <param name="firstName">First name of the person</param>
    /// <param name="lastName">Last name of the person</param>
    /// <param name="birthYear">Birth year (4 digits)</param>
    /// <param name="birthMonth">Birth month (2 digits, zero-padded)</param>
    /// <param name="birthDay">Birth day (2 digits, zero-padded)</param>
    public CitizenVerificationRequest(
        string tcNo,
        string firstName,
        string lastName,
        string birthYear,
        string birthMonth,
        string birthDay)
    {
        TCNo = tcNo;
        FirstName = firstName;
        LastName = lastName;
        BirthYear = birthYear;
        BirthMonth = birthMonth;
        BirthDay = birthDay;
    }
}
