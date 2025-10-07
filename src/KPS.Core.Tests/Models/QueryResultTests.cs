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
        result.Person.Should().BeEmpty();
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

        // Act
        result.Status = true;
        result.Code = 1;
        result.Aciklama = "Success";
        result.Person = "tc_vatandasi";
        result.Extra = extraData;
        result.Raw = "<xml>response</xml>";

        // Assert
        result.Status.Should().BeTrue();
        result.Code.Should().Be(1);
        result.Aciklama.Should().Be("Success");
        result.Person.Should().Be("tc_vatandasi");
        result.Extra.Should().BeEquivalentTo(extraData);
        result.Raw.Should().Be("<xml>response</xml>");
    }

    [Theory]
    [InlineData(1, "Success")]
    [InlineData(2, "Error/NotFound")]
    [InlineData(3, "Deceased")]
    public void ResultCodes_ShouldHaveCorrectValues(int code, string expectedDescription)
    {
        // Act & Assert
        switch (code)
        {
            case 1:
                ResultCodes.Success.Should().Be(1);
                break;
            case 2:
                ResultCodes.ErrorOrNotFound.Should().Be(2);
                break;
            case 3:
                ResultCodes.Deceased.Should().Be(3);
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
