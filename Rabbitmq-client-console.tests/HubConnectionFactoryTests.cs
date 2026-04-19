using FluentAssertions;

namespace RabbitMQ_Client_Console.Tests;

public class HubConnectionFactoryTests
{
    //  BuildHubUrl 

    [Fact]
    public void BuildHubUrl_StandardInput_ReturnsCorrectUrl()
    {
        var result = HubConnectionFactory.BuildHubUrl("https://localhost:7279");

        result.Should().Be("https://localhost:7279/hubs/queue-monitor");
    }

    [Fact]
    public void BuildHubUrl_TrailingSlashOnBase_IsStripped()
    {
        var result = HubConnectionFactory.BuildHubUrl("https://localhost:7279/");

        result.Should().Be("https://localhost:7279/hubs/queue-monitor");
    }

    [Fact]
    public void BuildHubUrl_MultipleTrailingSlashes_AreStripped()
    {
        var result = HubConnectionFactory.BuildHubUrl("https://localhost:7279///");

        result.Should().Be("https://localhost:7279/hubs/queue-monitor");
    }

    [Fact]
    public void BuildHubUrl_CustomHubPath_UsesCustomPath()
    {
        var result = HubConnectionFactory.BuildHubUrl(
            "https://myserver.com",
            "/hubs/custom-monitor");

        result.Should().Be("https://myserver.com/hubs/custom-monitor");
    }

    [Fact]
    public void BuildHubUrl_HttpScheme_Works()
    {
        var result = HubConnectionFactory.BuildHubUrl("http://localhost:5046");

        result.Should().Be("http://localhost:5046/hubs/queue-monitor");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void BuildHubUrl_EmptyOrNullBase_ThrowsArgumentException(string? apiBase)
    {
        var act = () => HubConnectionFactory.BuildHubUrl(apiBase!);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*API base URL*");
    }

    // ── Build (connection) 

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Build_EmptyHubUrl_ThrowsArgumentException(string? url)
    {
        var act = () => HubConnectionFactory.Build(url!, "some-token");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Hub URL*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Build_EmptyToken_ThrowsArgumentException(string? token)
    {
        var act = () => HubConnectionFactory.Build("https://localhost:7279/hubs/queue-monitor", token!);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Access token*");
    }

    [Fact]
    public void Build_ValidInputs_ReturnsHubConnection()
    {
        var connection = HubConnectionFactory.Build(
            "https://localhost:7279/hubs/queue-monitor",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test");

        connection.Should().NotBeNull();
    }
}
