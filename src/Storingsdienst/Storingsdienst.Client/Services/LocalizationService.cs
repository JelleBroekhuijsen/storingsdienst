using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Storingsdienst.Client.Resources;
using System.Globalization;

namespace Storingsdienst.Client.Services;

/// <summary>
/// Implementation of the localization service that manages language switching
/// and persists user preferences to localStorage.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IJSRuntime _jsRuntime;
    private string _currentCulture = "nl"; // Dutch is the default

    public event Action? OnLanguageChanged;

    public string CurrentCulture => _currentCulture;

    public string this[string key] => _localizer[key];

    public LocalizationService(
        IStringLocalizer<SharedResources> localizer,
        IJSRuntime jsRuntime)
    {
        _localizer = localizer;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedCulture = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem", "preferredLanguage");

            if (!string.IsNullOrEmpty(savedCulture) &&
                (savedCulture == "nl" || savedCulture == "en"))
            {
                await SetLanguageInternalAsync(savedCulture, persist: false);
            }
        }
        catch
        {
            // Ignore localStorage errors (e.g., in SSR or restricted environments)
        }
    }

    public async Task SetLanguageAsync(string culture)
    {
        await SetLanguageInternalAsync(culture, persist: true);
    }

    private async Task SetLanguageInternalAsync(string culture, bool persist)
    {
        if (culture != "nl" && culture != "en")
            culture = "nl"; // Default to Dutch for invalid values

        if (_currentCulture == culture)
            return;

        _currentCulture = culture;

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        if (persist)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "localStorage.setItem", "preferredLanguage", culture);
            }
            catch
            {
                // Ignore localStorage errors
            }
        }

        OnLanguageChanged?.Invoke();
    }
}
