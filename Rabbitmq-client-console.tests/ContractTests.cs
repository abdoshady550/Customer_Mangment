using FluentAssertions;
using RabbitMQ_Client_Console.DTOs;
using System.Text.Json;

namespace RabbitMQ_Client_Console.Tests;


public class TokenServerResponseTests
{
    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_FullOpenIddictResponse_MapsAllFields()
    {
        var json = """
            {
              "access_token":  "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test",
              "token_type":    "Bearer",
              "expires_in":    3600,
              "refresh_token": "some-refresh-token-value"
            }
            """;

        var result = JsonSerializer.Deserialize<TokenServerResponse>(json, _json);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.RefreshToken.Should().Be("some-refresh-token-value");
    }

    [Fact]
    public void Deserialize_NoRefreshToken_RefreshTokenIsNull()
    {
        var json = """
            {
              "access_token": "some-token",
              "token_type":   "Bearer",
              "expires_in":   3600
            }
            """;

        var result = JsonSerializer.Deserialize<TokenServerResponse>(json, _json);

        result!.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void Deserialize_AccessTokenIsNullInJson_AccessTokenIsNull()
    {
        var json = """
            {
              "access_token": null,
              "token_type":   "Bearer",
              "expires_in":   3600
            }
            """;

        var result = JsonSerializer.Deserialize<TokenServerResponse>(json, _json);

        result!.AccessToken.Should().BeNull();
    }

    [Fact]
    public void Deserialize_CamelCaseProperties_MappedByJsonPropertyName()
    {
        // Confirms the [JsonPropertyName] attributes are correct —
        // without them the snake_case properties would not bind.
        var json = """{"access_token":"tok","token_type":"Bearer","expires_in":1800}""";

        var result = JsonSerializer.Deserialize<TokenServerResponse>(json, _json);

        result!.AccessToken.Should().Be("tok");
        result.ExpiresIn.Should().Be(1800);
    }
}


public class QueueStatusContractTests
{
    [Fact]
    public void Received_ValueIsRECEIVED() => QueueStatus.Received.Should().Be("RECEIVED");

    [Fact]
    public void Processed_ValueIsPROCESSED() => QueueStatus.Processed.Should().Be("PROCESSED");

    [Fact]
    public void Failed_ValueIsFAILED() => QueueStatus.Failed.Should().Be("FAILED");
}


public class QueueNotificationContractTests
{
    [Fact]
    public void QueueNotification_SerializeAndDeserialize_RoundTrip()
    {
        var original = new QueueNotification(
            QueueName: "customer-snapshots",
            Status: QueueStatus.Received,
            Message: "Customer 'Test' created (Id=abc-123)",
            MessageBody: "abc-123",
            ReceivedAt: new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(original);
        var result = JsonSerializer.Deserialize<QueueNotification>(json,
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.Should().NotBeNull();
        result!.QueueName.Should().Be(original.QueueName);
        result.Status.Should().Be(original.Status);
        result.Message.Should().Be(original.Message);
        result.MessageBody.Should().Be(original.MessageBody);
        result.ReceivedAt.Should().BeCloseTo(original.ReceivedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void QueueNotification_IsRecord_SupportsValueEquality()
    {
        var dt = DateTime.UtcNow;
        var a = new QueueNotification("q", "RECEIVED", "msg", "body", dt);
        var b = new QueueNotification("q", "RECEIVED", "msg", "body", dt);

        a.Should().Be(b);
    }
}
