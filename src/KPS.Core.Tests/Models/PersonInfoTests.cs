using FluentAssertions;
using KPS.Core.Models.Result;
using Xunit;

namespace KPS.Core.Tests.Models;

public class PersonInfoTests
{
    [Fact]
    public void PersonInfo_ShouldInitializeWithDefaultValues()
    {
        // Act
        var person = new PersonInfo();

        // Assert
        person.Type.Should().BeEmpty();
        person.IdentityNumber.Should().BeEmpty();
        person.FirstName.Should().BeEmpty();
        person.LastName.Should().BeEmpty();
        person.Nationality.Should().BeNull();
        person.BirthDate.Should().BeNull();
        person.DeathDate.Should().BeNull();
    }

    [Fact]
    public void PersonInfo_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var person = new PersonInfo
        {
            Type = PersonTypes.TurkishCitizen,
            IdentityNumber = "12345678901",
            FirstName = "Semih",
            LastName = "Atalay",
            BirthDate = "1998-07-02",
            DeathDate = null
        };

        // Assert
        person.Type.Should().Be(PersonTypes.TurkishCitizen);
        person.IdentityNumber.Should().Be("12345678901");
        person.FirstName.Should().Be("Semih");
        person.LastName.Should().Be("Atalay");
        person.BirthDate.Should().Be("1998-07-02");
        person.DeathDate.Should().BeNull();
    }

    [Fact]
    public void FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var person = new PersonInfo
        {
            FirstName = "Semih",
            LastName = "Atalay"
        };

        // Act & Assert
        person.FullName.Should().Be("Semih Atalay");
    }

    [Theory]
    [InlineData("", "Atalay", "Atalay")]
    [InlineData("Semih", "", "Semih")]
    [InlineData("", "", "")]
    public void FullName_ShouldHandleEmptyNames(string firstName, string lastName, string expected)
    {
        // Arrange
        var person = new PersonInfo
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act & Assert
        person.FullName.Should().Be(expected);
    }

    [Theory]
    [InlineData(PersonTypes.TurkishCitizen, true, false, false)]
    [InlineData(PersonTypes.Foreigner, false, true, false)]
    [InlineData(PersonTypes.BlueCard, false, false, true)]
    public void PersonTypeFlags_ShouldReturnCorrectValues(
        string type, 
        bool expectedTurkish, 
        bool expectedForeigner, 
        bool expectedBlueCard)
    {
        // Arrange
        var person = new PersonInfo { Type = type };

        // Act & Assert
        person.IsTurkishCitizen.Should().Be(expectedTurkish);
        person.IsForeigner.Should().Be(expectedForeigner);
        person.IsBlueCardHolder.Should().Be(expectedBlueCard);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("2020-01-15", true)]
    public void IsDeceased_ShouldReturnCorrectValue(string? deathDate, bool expected)
    {
        // Arrange
        var person = new PersonInfo { DeathDate = deathDate };

        // Act & Assert
        person.IsDeceased.Should().Be(expected);
    }

    [Fact]
    public void PersonInfo_ShouldSupportForeignNational()
    {
        // Arrange & Act
        var person = new PersonInfo
        {
            Type = PersonTypes.Foreigner,
            IdentityNumber = "99999999999",
            FirstName = "John",
            LastName = "Smith",
            Nationality = "USA",
            BirthDate = "1985-03-20"
        };

        // Assert
        person.IsForeigner.Should().BeTrue();
        person.Nationality.Should().Be("USA");
    }

    [Fact]
    public void PersonInfo_ShouldSupportBlueCardHolder()
    {
        // Arrange & Act
        var person = new PersonInfo
        {
            Type = PersonTypes.BlueCard,
            IdentityNumber = "88888888888",
            FirstName = "Ali",
            LastName = "Veli"
        };

        // Assert
        person.IsBlueCardHolder.Should().BeTrue();
        person.IsTurkishCitizen.Should().BeFalse();
    }

    [Fact]
    public void PersonInfo_ShouldSupportDeceasedPerson()
    {
        // Arrange & Act
        var person = new PersonInfo
        {
            Type = PersonTypes.TurkishCitizen,
            IdentityNumber = "12345678901",
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            BirthDate = "1950-01-01",
            DeathDate = "2020-12-31"
        };

        // Assert
        person.IsDeceased.Should().BeTrue();
        person.DeathDate.Should().Be("2020-12-31");
    }
}

