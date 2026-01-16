namespace Storingsdienst.Client.Services;

/// <summary>
/// Service for managing application localization and language switching.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture code (e.g., "nl" or "en").
    /// </summary>
    string CurrentCulture { get; }

    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localized string value.</returns>
    string this[string key] { get; }

    /// <summary>
    /// Event raised when the language is changed.
    /// </summary>
    event Action? OnLanguageChanged;

    /// <summary>
    /// Sets the application language and persists the preference.
    /// </summary>
    /// <param name="culture">The culture code ("nl" or "en").</param>
    Task SetLanguageAsync(string culture);

    /// <summary>
    /// Initializes the service by loading the persisted language preference.
    /// </summary>
    Task InitializeAsync();
}
