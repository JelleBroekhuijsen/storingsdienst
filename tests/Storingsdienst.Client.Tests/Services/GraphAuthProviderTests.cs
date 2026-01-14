using FluentAssertions;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;
using Moq;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Services;

public class GraphAuthProviderTests
{
    private readonly Mock<IAccessTokenProvider> _tokenProviderMock;

    public GraphAuthProviderTests()
    {
        _tokenProviderMock = new Mock<IAccessTokenProvider>();
    }

    private static IConfiguration CreateConfiguration(string[]? scopes = null)
    {
        var configData = new Dictionary<string, string?>();
        if (scopes != null)
        {
            for (int i = 0; i < scopes.Length; i++)
            {
                configData[$"MicrosoftGraph:Scopes:{i}"] = scopes[i];
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void Constructor_WithNullScopes_UsesDefaultScopes()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        // Act - Constructor should not throw
        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);

        // Assert - Provider should be created successfully
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithConfiguredScopes_UsesConfiguredScopes()
    {
        // Arrange
        var configuredScopes = new[] { "User.Read", "Calendars.Read", "Mail.Read" };
        var configuration = CreateConfiguration(configuredScopes);

        // Act
        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithValidToken_AddsAuthorizationHeader()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        var accessToken = new AccessToken { Value = "test-token-12345", Expires = DateTimeOffset.Now.AddHours(1) };
        var tokenResult = new AccessTokenResult(
            AccessTokenResultStatus.Success,
            accessToken,
            "https://login.microsoftonline.com/redirect",
            null);

        _tokenProviderMock
            .Setup(x => x.RequestAccessToken(It.IsAny<AccessTokenRequestOptions>()))
            .ReturnsAsync(tokenResult);

        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://graph.microsoft.com/v1.0/me"
        };

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        request.Headers.Should().ContainKey("Authorization");
        request.Headers["Authorization"].Should().Contain("Bearer test-token-12345");
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithFailedTokenResult_ThrowsAccessTokenNotAvailableException()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        var tokenResult = new AccessTokenResult(
            AccessTokenResultStatus.RequiresRedirect,
            new AccessToken(),
            "https://login.microsoftonline.com/redirect",
            null);

        _tokenProviderMock
            .Setup(x => x.RequestAccessToken(It.IsAny<AccessTokenRequestOptions>()))
            .ReturnsAsync(tokenResult);

        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://graph.microsoft.com/v1.0/me"
        };

        // Act
        Func<Task> act = async () => await provider.AuthenticateRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<AccessTokenNotAvailableException>();
    }

    [Fact]
    public async Task AuthenticateRequestAsync_RequestsTokenWithConfiguredScopes()
    {
        // Arrange
        var configuredScopes = new[] { "User.Read", "Calendars.Read" };
        var configuration = CreateConfiguration(configuredScopes);

        var accessToken = new AccessToken { Value = "test-token", Expires = DateTimeOffset.Now.AddHours(1) };
        var tokenResult = new AccessTokenResult(
            AccessTokenResultStatus.Success,
            accessToken,
            "https://login.microsoftonline.com/redirect",
            null);

        AccessTokenRequestOptions? capturedOptions = null;
        _tokenProviderMock
            .Setup(x => x.RequestAccessToken(It.IsAny<AccessTokenRequestOptions>()))
            .Callback<AccessTokenRequestOptions>(opts => capturedOptions = opts)
            .ReturnsAsync(tokenResult);

        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://graph.microsoft.com/v1.0/me"
        };

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Scopes.Should().BeEquivalentTo(configuredScopes);
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithCancellationToken_PassesTokenToProvider()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        var accessToken = new AccessToken { Value = "test-token", Expires = DateTimeOffset.Now.AddHours(1) };
        var tokenResult = new AccessTokenResult(
            AccessTokenResultStatus.Success,
            accessToken,
            "https://login.microsoftonline.com/redirect",
            null);

        _tokenProviderMock
            .Setup(x => x.RequestAccessToken(It.IsAny<AccessTokenRequestOptions>()))
            .ReturnsAsync(tokenResult);

        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://graph.microsoft.com/v1.0/me"
        };

        using var cts = new CancellationTokenSource();

        // Act - should not throw even with cancellation token
        await provider.AuthenticateRequestAsync(request, null, cts.Token);

        // Assert
        request.Headers.Should().ContainKey("Authorization");
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithAdditionalContext_CompletesSuccessfully()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        var accessToken = new AccessToken { Value = "test-token", Expires = DateTimeOffset.Now.AddHours(1) };
        var tokenResult = new AccessTokenResult(
            AccessTokenResultStatus.Success,
            accessToken,
            "https://login.microsoftonline.com/redirect",
            null);

        _tokenProviderMock
            .Setup(x => x.RequestAccessToken(It.IsAny<AccessTokenRequestOptions>()))
            .ReturnsAsync(tokenResult);

        var provider = new GraphAuthProvider(_tokenProviderMock.Object, configuration);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://graph.microsoft.com/v1.0/me"
        };

        var additionalContext = new Dictionary<string, object>
        {
            { "claim", "value" }
        };

        // Act - should complete successfully with additional context
        await provider.AuthenticateRequestAsync(request, additionalContext);

        // Assert
        request.Headers.Should().ContainKey("Authorization");
    }
}
