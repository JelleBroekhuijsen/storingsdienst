using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using BlazorTokenProvider = Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider;

namespace Storingsdienst.Client.Services;

/// <summary>
/// Authentication provider for Microsoft Graph that uses MSAL tokens from Blazor WebAssembly.
/// This provider integrates with the MSAL authentication configured in Program.cs.
/// </summary>
public class GraphAuthProvider : IAuthenticationProvider
{
    private readonly BlazorTokenProvider _tokenProvider;
    private readonly string[] _scopes;

    public GraphAuthProvider(BlazorTokenProvider tokenProvider, IConfiguration configuration)
    {
        _tokenProvider = tokenProvider;

        // Get scopes from configuration, default to basic Graph scopes
        _scopes = configuration.GetSection("MicrosoftGraph:Scopes").Get<string[]>()
            ?? new[] { "User.Read", "Calendars.Read" };
    }

    /// <summary>
    /// Authenticates the request by adding the Bearer token to the Authorization header.
    /// </summary>
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        // Request an access token with the configured scopes
        var tokenResult = await _tokenProvider.RequestAccessToken(
            new AccessTokenRequestOptions
            {
                Scopes = _scopes
            });

        if (tokenResult.TryGetToken(out var token))
        {
            // Add the Bearer token to the request
            request.Headers.Add("Authorization", $"Bearer {token.Value}");
        }
        else
        {
            // Token acquisition failed - throw exception to trigger redirect to login
            // The calling code should catch AccessTokenNotAvailableException and call ex.Redirect()
            throw new AccessTokenNotAvailableException(
                new PlaceholderNavigationManager(),
                tokenResult,
                _scopes);
        }
    }
}

/// <summary>
/// Minimal NavigationManager implementation for AccessTokenNotAvailableException.
/// The actual redirect is handled by the calling code when it catches the exception.
/// </summary>
internal class PlaceholderNavigationManager : Microsoft.AspNetCore.Components.NavigationManager
{
    public PlaceholderNavigationManager()
    {
        // Initialize with placeholder values - the actual navigation is handled elsewhere
        Initialize("https://localhost/", "https://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        // Navigation is handled by the Blazor component that catches the exception
    }
}
