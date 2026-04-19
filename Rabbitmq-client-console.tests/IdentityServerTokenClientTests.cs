using FluentAssertions;
using RabbitMQ_Client_Console.Services;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace RabbitMQ_Client_Console.Tests;


public class IdentityServerTokenClientTests
{
    private const string BaseUrl = "https://localhost:7278";
    private const string TokenPath = "/connect/token";
    private const string TestEmail = "admin@test.com";
    private const string TestPass = "Admin@123";


    private static string ValidTokenJson(string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test") =>
        JsonSerializer.Serialize(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 3600,
            refresh_token = "refresh-abc"
        });

    private static IdentityServerTokenClient CreateClient(MockHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) }, BaseUrl);


    [Fact]
    public async Task GetAccessTokenAsync_ValidCredentials_ReturnsAccessToken()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.OK, "application/json", ValidTokenJson("my-token-value"));

        var client = CreateClient(handler);

        var token = await client.GetAccessTokenAsync(TestEmail, TestPass);

        token.Should().Be("my-token-value");
    }

    [Fact]
    public async Task GetAccessTokenAsync_SendsCorrectGrantType()
    {
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .With(req =>
            {
                capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond(HttpStatusCode.OK, "application/json", ValidTokenJson());

        var client = CreateClient(handler);
        await client.GetAccessTokenAsync(TestEmail, TestPass);

        capturedBody.Should().Contain("grant_type=password");
        capturedBody.Should().Contain($"username={Uri.EscapeDataString(TestEmail)}");
    }

    [Fact]
    public async Task GetAccessTokenAsync_SendsClientId()
    {
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .With(req =>
            {
                capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond(HttpStatusCode.OK, "application/json", ValidTokenJson());

        var client = CreateClient(handler);
        await client.GetAccessTokenAsync(TestEmail, TestPass);

        capturedBody.Should().Contain("client_id=customer-management-swagger");
    }

    //  Server error responses 

    [Fact]
    public async Task GetAccessTokenAsync_Unauthorized_ThrowsHttpRequestException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.Unauthorized, "application/json",
                     """{"error":"invalid_client"}""");

        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(TestEmail, TestPass);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*401*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_BadRequest_ThrowsHttpRequestException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.BadRequest, "application/json",
                     """{"error":"invalid_grant","error_description":"Wrong password"}""");

        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(TestEmail, "wrong-password");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*400*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ServerError_ThrowsHttpRequestException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.InternalServerError, "application/json",
                     """{"error":"server_error"}""");

        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(TestEmail, TestPass);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*500*");
    }

    //  Missing token   

    [Fact]
    public async Task GetAccessTokenAsync_ResponseMissingAccessToken_ThrowsInvalidOperationException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.OK, "application/json",
                     """{"token_type":"Bearer","expires_in":3600}""");

        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(TestEmail, TestPass);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*access_token*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_EmptyBody_ThrowsException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(HttpStatusCode.OK, "application/json", "null");

        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(TestEmail, TestPass);

        await act.Should().ThrowAsync<Exception>();
    }

    //  Input validation 

    [Theory]
    [InlineData("", "Admin@123")]
    [InlineData("   ", "Admin@123")]
    public async Task GetAccessTokenAsync_EmptyEmail_ThrowsArgumentException(
        string email, string password)
    {
        var handler = new MockHttpMessageHandler();
        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(email, password);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Email*");
    }

    [Theory]
    [InlineData("admin@test.com", "")]
    [InlineData("admin@test.com", "   ")]
    public async Task GetAccessTokenAsync_EmptyPassword_ThrowsArgumentException(
        string email, string password)
    {
        var handler = new MockHttpMessageHandler();
        var client = CreateClient(handler);

        var act = () => client.GetAccessTokenAsync(email, password);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Password*");
    }

    //  Cancellation 

    [Fact]
    public async Task GetAccessTokenAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .Respond(async () =>
            {
                await Task.Delay(5_000);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var client = CreateClient(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var act = () => client.GetAccessTokenAsync(TestEmail, TestPass, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    //  Custom client ID 

    [Fact]
    public async Task GetAccessTokenAsync_CustomClientId_SendsCustomClientId()
    {
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler();
        handler
            .When(HttpMethod.Post, $"{BaseUrl}{TokenPath}")
            .With(req =>
            {
                capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond(HttpStatusCode.OK, "application/json", ValidTokenJson());

        var client = new IdentityServerTokenClient(
            new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) },
            BaseUrl,
            clientId: "my-custom-client");

        await client.GetAccessTokenAsync(TestEmail, TestPass);

        capturedBody.Should().Contain("client_id=my-custom-client");
    }
}
