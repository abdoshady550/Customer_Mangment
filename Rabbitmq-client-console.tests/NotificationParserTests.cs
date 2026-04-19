using FluentAssertions;
using RabbitMQ_Client_Console.DTOs;
using System.Text.Json;

namespace RabbitMQ_Client_Console.Tests;

public class NotificationParserTests
{

    [Fact]
    public void TryParse_NullInput_ReturnsNull()
    {
        var result = NotificationParser.TryParse(null);

        result.Should().BeNull();
    }


    [Fact]
    public void TryParse_AlreadyTyped_ReturnsSameInstance()
    {
        var notification = new QueueNotification(
            QueueName: "customer-snapshots",
            Status: "RECEIVED",
            Message: "Customer created",
            MessageBody: "abc-123",
            ReceivedAt: new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        var result = NotificationParser.TryParse(notification);

        result.Should().BeSameAs(notification);
    }


    [Fact]
    public void TryParse_ValidJsonString_DeserializesCorrectly()
    {
        var json = """
            {
              "queueName":   "address-snapshots",
              "status":      "PROCESSED",
              "message":     "Address updated",
              "messageBody": "guid-456",
              "receivedAt":  "2025-06-01T10:00:00Z"
            }
            """;

        var result = NotificationParser.TryParse(json);

        result.Should().NotBeNull();
        result!.QueueName.Should().Be("address-snapshots");
        result.Status.Should().Be("PROCESSED");
        result.Message.Should().Be("Address updated");
        result.MessageBody.Should().Be("guid-456");
    }

    [Fact]
    public void TryParse_JsonWithExtraFields_IgnoresExtraFields()
    {
        var json = """
            {
              "queueName":   "q",
              "status":      "RECEIVED",
              "message":     "m",
              "messageBody": "b",
              "receivedAt":  "2025-01-01T00:00:00Z",
              "unknownField": "should be ignored"
            }
            """;

        var act = () => NotificationParser.TryParse(json);

        act.Should().NotThrow();
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_BlankString_ReturnsNull(string input)
    {
        var result = NotificationParser.TryParse(input);

        result.Should().BeNull();
    }

    [Fact]
    public void TryParse_InvalidJson_ReturnsNull()
    {
        var result = NotificationParser.TryParse("{ not valid json }}}");

        result.Should().BeNull();
    }

    [Fact]
    public void TryParse_EmptyJsonObject_ReturnsObjectWithDefaults()
    {

        var result = NotificationParser.TryParse("{}");

        result.Should().NotBeNull();
        result!.QueueName.Should().BeNull();
        result.Status.Should().BeNull();
    }


    [Fact]
    public void TryParse_ReceivedAtIsPreserved()
    {
        var expected = new DateTime(2025, 3, 15, 8, 30, 0, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(new
        {
            queueName = "q",
            status = "RECEIVED",
            message = "m",
            messageBody = "b",
            receivedAt = expected
        });

        var result = NotificationParser.TryParse(json);

        result!.ReceivedAt.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }
}
