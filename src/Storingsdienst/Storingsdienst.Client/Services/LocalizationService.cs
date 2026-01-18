using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Storingsdienst.Client.Resources;
using System.Globalization;
using System.Net.Http.Json;
using System.Resources;

namespace Storingsdienst.Client.Services;

/// <summary>
/// Implementation of the localization service that manages language switching
/// and persists user preferences to localStorage.
/// Uses JSON files for translations to work around Blazor WASM satellite assembly limitations.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentCulture = "nl"; // Dutch is the default
    private bool _isInitialized = false;

    public event Action? OnLanguageChanged;

    public string CurrentCulture => _currentCulture;

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(_currentCulture, out var cultureDict) &&
                cultureDict.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to Dutch if key not found in current culture
            if (_currentCulture != "nl" &&
                _translations.TryGetValue("nl", out var nlDict) &&
                nlDict.TryGetValue(key, out var nlValue))
            {
                return nlValue;
            }

            // Return the key itself if not found
            return key;
        }
    }

    public LocalizationService(
        IStringLocalizerFactory localizerFactory,
        IJSRuntime jsRuntime,
        HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["nl"] = new Dictionary<string, string>(),
            ["en"] = new Dictionary<string, string>()
        };
    }

    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("[LocalizationService] InitializeAsync called");

            // Load translations from JSON files
            bool justLoaded = false;
            if (!_isInitialized)
            {
                await LoadTranslationsAsync();
                _isInitialized = true;
                justLoaded = true;
            }

            var savedCulture = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem", "preferredLanguage");

            Console.WriteLine($"[LocalizationService] savedCulture from localStorage: '{savedCulture}'");
            Console.WriteLine($"[LocalizationService] _currentCulture before: '{_currentCulture}'");

            bool cultureChanged = false;
            if (!string.IsNullOrEmpty(savedCulture) &&
                (savedCulture == "nl" || savedCulture == "en"))
            {
                Console.WriteLine($"[LocalizationService] Setting language to: '{savedCulture}'");
                var previousCulture = _currentCulture;
                await SetLanguageInternalAsync(savedCulture, persist: false);
                cultureChanged = previousCulture != _currentCulture;
            }
            
            // Notify subscribers when translations are just loaded AND culture didn't change
            // (if culture changed, SetLanguageInternalAsync already fired the event)
            if (justLoaded && !cultureChanged)
            {
                Console.WriteLine("[LocalizationService] Translations loaded, notifying subscribers");
                OnLanguageChanged?.Invoke();
            }

            Console.WriteLine($"[LocalizationService] _currentCulture after: '{_currentCulture}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalizationService] InitializeAsync error: {ex.Message}");
            // Ignore localStorage errors (e.g., in SSR or restricted environments)
        }
    }

    private async Task LoadTranslationsAsync()
    {
        try
        {
            Console.WriteLine("[LocalizationService] Loading translations from JSON files...");

            // Load Dutch translations
            var nlTranslations = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>("locales/nl.json");
            if (nlTranslations != null)
            {
                _translations["nl"] = nlTranslations;
                Console.WriteLine($"[LocalizationService] Loaded {nlTranslations.Count} Dutch translations");
            }

            // Load English translations
            var enTranslations = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>("locales/en.json");
            if (enTranslations != null)
            {
                _translations["en"] = enTranslations;
                Console.WriteLine($"[LocalizationService] Loaded {enTranslations.Count} English translations");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalizationService] Failed to load translations: {ex.Message}");
            // Fall back to empty dictionaries - keys will be returned as-is
        }
    }

    public async Task SetLanguageAsync(string culture)
    {
        await SetLanguageInternalAsync(culture, persist: true);
    }

    private async Task SetLanguageInternalAsync(string culture, bool persist)
    {
        Console.WriteLine($"[LocalizationService] SetLanguageInternalAsync called with culture='{culture}', persist={persist}");

        if (culture != "nl" && culture != "en")
            culture = "nl"; // Default to Dutch for invalid values

        if (_currentCulture == culture)
        {
            Console.WriteLine($"[LocalizationService] Culture already set to '{culture}', returning early");
            return;
        }

        Console.WriteLine($"[LocalizationService] Changing culture from '{_currentCulture}' to '{culture}'");
        _currentCulture = culture;

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        Console.WriteLine($"[LocalizationService] Set DefaultThreadCurrentUICulture to '{CultureInfo.DefaultThreadCurrentUICulture.Name}'");

        if (persist)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "localStorage.setItem", "preferredLanguage", culture);
                Console.WriteLine($"[LocalizationService] Persisted culture '{culture}' to localStorage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalizationService] Failed to persist culture: {ex.Message}");
                // Ignore localStorage errors
            }
        }

        Console.WriteLine($"[LocalizationService] Invoking OnLanguageChanged event");
        OnLanguageChanged?.Invoke();
        Console.WriteLine($"[LocalizationService] OnLanguageChanged event completed");
    }
}
