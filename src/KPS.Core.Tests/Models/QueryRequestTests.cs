using FluentAssertions;
using KPS.Core.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace KPS.Core.Tests.Models;

public class QueryRequestTests
{
    [Fact]
    public void QueryRequest_ShouldHaveRequiredAttributes()
    {
        // Arrange
        var request = new QueryRequest();

        // Act & Assert
        var tcNoProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.TCNo));
        tcNoProperty.Should().NotBeNull();
        tcNoProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        tcNoProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var firstNameProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.FirstName));
        firstNameProperty.Should().NotBeNull();
        firstNameProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();

        var lastNameProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.LastName));
        lastNameProperty.Should().NotBeNull();
        lastNameProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();

        var birthYearProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.BirthYear));
        birthYearProperty.Should().NotBeNull();
        birthYearProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthYearProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var birthMonthProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.BirthMonth));
        birthMonthProperty.Should().NotBeNull();
        birthMonthProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthMonthProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();

        var birthDayProperty = typeof(QueryRequest).GetProperty(nameof(QueryRequest.BirthDay));
        birthDayProperty.Should().NotBeNull();
        birthDayProperty!.GetCustomAttributes(typeof(RequiredAttribute), false).Should().NotBeEmpty();
        birthDayProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void QueryRequest_ShouldInitializeWithEmptyStrings()
    {
        // Act
        var request = new QueryRequest();

        // Assert
        request.TCNo.Should().BeEmpty();
        request.FirstName.Should().BeEmpty();
        request.LastName.Should().BeEmpty();
        request.BirthYear.Should().BeEmpty();
        request.BirthMonth.Should().BeEmpty();
        request.BirthDay.Should().BeEmpty();
    }

    [Fact]
    public void QueryRequest_ShouldAllowSettingProperties()
    {
        // Arrange
        var request = new QueryRequest();

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
}
