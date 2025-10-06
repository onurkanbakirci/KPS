using FluentAssertions;
using KPS.Core;
using KPS.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KPS.Core.Tests;

public class KpsClientBuilderTests
{
    [Fact]
    public void WithUsername_ShouldSetUsername()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var username = "testuser";

        // Act
        var result = builder.WithUsername(username);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithUsername_WithNullUsername_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithUsername(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be null or empty*");
    }

    [Fact]
    public void WithUsername_WithEmptyUsername_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithUsername("");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be null or empty*");
    }

    [Fact]
    public void WithPassword_ShouldSetPassword()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var password = "testpass";

        // Act
        var result = builder.WithPassword(password);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithPassword_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithPassword(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty*");
    }

    [Fact]
    public void WithPassword_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithPassword("");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty*");
    }

    [Fact]
    public void WithStsEndpoint_ShouldSetStsEndpoint()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var endpoint = "https://test-sts.com";

        // Act
        var result = builder.WithStsEndpoint(endpoint);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithStsEndpoint_WithNullEndpoint_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithStsEndpoint(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("STS endpoint cannot be null or empty*");
    }

    [Fact]
    public void WithKpsEndpoint_ShouldSetKpsEndpoint()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var endpoint = "https://test-kps.com";

        // Act
        var result = builder.WithKpsEndpoint(endpoint);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithKpsEndpoint_WithNullEndpoint_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithKpsEndpoint(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("KPS endpoint cannot be null or empty*");
    }

    [Fact]
    public void WithTimeout_ShouldSetTimeout()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var timeout = 60;

        // Act
        var result = builder.WithTimeout(timeout);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithTimeout_WithZeroTimeout_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithTimeout(0);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Timeout must be greater than 0*");
    }

    [Fact]
    public void WithTimeout_WithNegativeTimeout_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithTimeout(-1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Timeout must be greater than 0*");
    }

    [Fact]
    public void WithRawResponse_ShouldSetRawResponse()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act
        var result = builder.WithRawResponse(true);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithHttpClient_ShouldSetHttpClient()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var httpClient = new HttpClient();

        // Act
        var result = builder.WithHttpClient(httpClient);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithHttpClient_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithHttpClient(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithLogger_ShouldSetLogger()
    {
        // Arrange
        var builder = new KpsClientBuilder();
        var logger = new Mock<ILogger<KpsClient>>().Object;

        // Act
        var result = builder.WithLogger(logger);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithLogger_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new KpsClientBuilder();

        // Act & Assert
        var action = () => builder.WithLogger(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_WithUsernameAndPassword_ShouldReturnKpsClient()
    {
        // Arrange
        var builder = new KpsClientBuilder()
            .WithUsername("testuser")
            .WithPassword("testpass");

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IKpsClient>();
    }

    [Fact]
    public void Build_WithoutUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new KpsClientBuilder()
            .WithPassword("testpass");

        // Act & Assert
        var action = () => builder.Build();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Username must be set before building the client");
    }

    [Fact]
    public void Build_WithoutPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new KpsClientBuilder()
            .WithUsername("testuser");

        // Act & Assert
        var action = () => builder.Build();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Password must be set before building the client");
    }

    [Fact]
    public void Build_WithAllOptions_ShouldReturnConfiguredClient()
    {
        // Arrange
        var httpClient = new HttpClient();
        var logger = new Mock<ILogger<KpsClient>>().Object;
        
        var builder = new KpsClientBuilder()
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithStsEndpoint("https://custom-sts.com")
            .WithKpsEndpoint("https://custom-kps.com")
            .WithTimeout(60)
            .WithRawResponse(true)
            .WithHttpClient(httpClient)
            .WithLogger(logger);

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IKpsClient>();
    }
}
