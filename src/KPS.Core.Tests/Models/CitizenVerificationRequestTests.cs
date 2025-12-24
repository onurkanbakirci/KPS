using FluentAssertions;
using KPS.Core.Models.Request;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace KPS.Core.Tests.Models;

public class CitizenVerificationRequestTests
{
    [Fact]
    public void CitizenVerificationRequest_ShouldHaveRequiredAttributes()
    {
        // Arrange
        var request = new CitizenVerificationRequest();

        // Act & Assert
        var tcNoProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.TCNo));
        tcNoProperty.Should().NotBeNull();
        tcNoProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        tcNoProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var firstNameProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.FirstName));
        firstNameProperty.Should().NotBeNull();
        firstNameProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();

        var lastNameProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.LastName));
        lastNameProperty.Should().NotBeNull();
        lastNameProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();

        var birthYearProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.BirthYear));
        birthYearProperty.Should().NotBeNull();
        birthYearProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthYearProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var birthMonthProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.BirthMonth));
        birthMonthProperty.Should().NotBeNull();
        birthMonthProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthMonthProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var birthDayProperty = typeof(CitizenVerificationRequest).GetProperty(nameof(CitizenVerificationRequest.BirthDay));
        birthDayProperty.Should().NotBeNull();
        birthDayProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthDayProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void CitizenVerificationRequest_ShouldInitializeWithEmptyStrings()
    {
        // Act
        var request = new CitizenVerificationRequest();

        // Assert
        request.TCNo.Should().BeEmpty();
        request.FirstName.Should().BeEmpty();
        request.LastName.Should().BeEmpty();
        request.BirthYear.Should().BeEmpty();
        request.BirthMonth.Should().BeEmpty();
        request.BirthDay.Should().BeEmpty();
    }

    [Fact]
    public void CitizenVerificationRequest_ShouldAllowSettingProperties()
    {
        // Arrange
        var request = new CitizenVerificationRequest();

        // Act
        request.TCNo = "12345678901";
        request.FirstName = "John";
        request.LastName = "Doe";
        request.BirthYear = "1990";
        request.BirthMonth = "01";
        request.BirthDay = "15";

        // Assert
        request.TCNo.Should().Be("12345678901");
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
        request.BirthYear.Should().Be("1990");
        request.BirthMonth.Should().Be("01");
        request.BirthDay.Should().Be("15");
    }

    [Fact]
    public void CitizenVerificationRequest_ConstructorShouldSetAllProperties()
    {
        // Act
        var request = new CitizenVerificationRequest(
            tcNo: "12345678901",
            firstName: "John",
            lastName: "Doe",
            birthYear: "1990",
            birthMonth: "01",
            birthDay: "15"
        );

        // Assert
        request.TCNo.Should().Be("12345678901");
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
        request.BirthYear.Should().Be("1990");
        request.BirthMonth.Should().Be("01");
        request.BirthDay.Should().Be("15");
    }

    [Fact]
    public void CitizenVerificationRequest_ConstructorShouldAcceptAllParameters()
    {
        // Act
        var request = new CitizenVerificationRequest(
            "98765432109",
            "Jane",
            "Smith",
            "1985",
            "12",
            "31"
        );

        // Assert
        request.TCNo.Should().Be("98765432109");
        request.FirstName.Should().Be("Jane");
        request.LastName.Should().Be("Smith");
        request.BirthYear.Should().Be("1985");
        request.BirthMonth.Should().Be("12");
        request.BirthDay.Should().Be("31");
    }
}
