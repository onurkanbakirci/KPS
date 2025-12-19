using FluentAssertions;
using KPS.Core.Models.Result;
using Xunit;

namespace KPS.Core.Tests.Models;

public class QueryResultTests
{
    [Fact]
    public void QueryResult_ShouldInitializeWithDefaultValues()
    {
        // Act
        var result = new QueryResult();

        // Assert
        result.Status.Should().BeFalse();
        result.Code.Should().Be(0);
        result.Aciklama.Should().BeEmpty();
        result.Person.Should().BeNull();
        result.Extra.Should().NotBeNull();
        result.Extra.Should().BeEmpty();
        result.Raw.Should().BeEmpty();
    }

    [Fact]
    public void QueryResult_ShouldAllowSettingProperties()
    {
        // Arrange
        var result = new QueryResult();
        var extraData = new Dictionary<string, object> { ["key"] = "value" };
        var personInfo = new PersonInfo
        {
            Type = PersonTypes.TurkishCitizen,
            IdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        result.Status = true;
        result.Code = ResultCodes.Success;
        result.Aciklama = "Success";
        result.Person = personInfo;
        result.Extra = extraData;
        result.Raw = "<xml>response</xml>";

        // Assert
        result.Status.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Success);
        result.Aciklama.Should().Be("Success");
        result.Person.Should().NotBeNull();
        result.Person!.Type.Should().Be(PersonTypes.TurkishCitizen);
        result.Person.IdentityNumber.Should().Be("12345678901");
        result.Person.FirstName.Should().Be("John");
        result.Person.LastName.Should().Be("Doe");
        result.Extra.Should().BeEquivalentTo(extraData);
        result.Raw.Should().Be("<xml>response</xml>");
    }

    [Theory]
    [InlineData(1, "Success")]
    [InlineData(2, "ErrorOrNotFound")]
    [InlineData(3, "Deceased")]
    public void ResultCodes_ShouldHaveCorrectValues(int code, string expectedDescription)
    {
        // Act & Assert
        switch (code)
        {
            case 1:
                ((int)ResultCodes.Success).Should().Be(1);
                ResultCodes.Success.ToString().Should().Be(expectedDescription);
                break;
            case 2:
                ((int)ResultCodes.ErrorOrNotFound).Should().Be(2);
                ResultCodes.ErrorOrNotFound.ToString().Should().Be(expectedDescription);
                break;
            case 3:
                ((int)ResultCodes.Deceased).Should().Be(3);
                ResultCodes.Deceased.ToString().Should().Be(expectedDescription);
                break;
        }
    }

    [Theory]
    [InlineData("tc_vatandasi")]
    [InlineData("yabanci")]
    [InlineData("mavi")]
    public void PersonTypes_ShouldHaveCorrectValues(string personType)
    {
        // Act & Assert
        switch (personType)
        {
            case "tc_vatandasi":
                PersonTypes.TurkishCitizen.Should().Be("tc_vatandasi");
                break;
            case "yabanci":
                PersonTypes.Foreigner.Should().Be("yabanci");
                break;
            case "mavi":
                PersonTypes.BlueCard.Should().Be("mavi");
                break;
        }
    }
}
