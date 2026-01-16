using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Storingsdienst.Client.Resources;
using System.Globalization;
using System.Resources;

namespace Storingsdienst.Client.Services;

/// <summary>
/// Implementation of the localization service that manages language switching
/// and persists user preferences to localStorage.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ResourceManager _resourceManager;
    private string _currentCulture = "nl"; // Dutch is the default
    private CultureInfo _currentCultureInfo;

    public event Action? OnLanguageChanged;

    public string CurrentCulture => _currentCulture;

    public string this[string key]
    {
        get
        {
            // Use ResourceManager directly with explicit culture
            var value = _resourceManager.GetString(key, _currentCultureInfo);
            return value ?? key;
        }
    }

    public LocalizationService(
        IStringLocalizerFactory localizerFactory,
        IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;

        // Get the ResourceManager from the SharedResources type
        _resourceManager = new ResourceManager(
            typeof(SharedResources).FullName!,
            typeof(SharedResources).Assembly);

        _currentCultureInfo = new CultureInfo(_currentCulture);
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
        _currentCultureInfo = new CultureInfo(culture);

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
