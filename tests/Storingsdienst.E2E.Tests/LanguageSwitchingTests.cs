using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Storingsdienst.E2E.Tests;

[TestFixture]
public class LanguageSwitchingTests : PageTest
{
    private const string BaseUrl = "https://localhost:5266";

    // Use the system-installed Edge browser
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        };
    }

    [SetUp]
    public async Task Setup()
    {
        // Clear localStorage before each test
        await Page.GotoAsync(BaseUrl);
        await Page.EvaluateAsync("localStorage.clear()");
    }

    [Test]
    public async Task HomePage_DefaultsToNl_ShowsAppTitleInDutch()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (Dutch is default) - app bar title, exact match
        var dutchTitleLocator = Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true });
        await Expect(dutchTitleLocator).ToBeVisibleAsync();

        // Assert - Default should be Dutch (text appears in title bar)
        var appTitle = await dutchTitleLocator.CountAsync();
        appTitle.Should().BeGreaterThanOrEqualTo(1, "App title should be shown in Dutch by default");
    }

    [Test]
    public async Task LanguageSelector_ClickEnglishFlag_ChangesToEnglish()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load and verify we start in Dutch (app bar title, exact match)
        var dutchTitleLocator = Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true });
        await Expect(dutchTitleLocator).ToBeVisibleAsync();

        // Act - Click the English flag button (second button in the group)
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update (no page reload needed) - app bar title exact match
        var englishTitleLocator = Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true });
        await Expect(englishTitleLocator).ToBeVisibleAsync();
    }

    [Test]
    public async Task LanguageSelector_ClickEnglishFlag_SavesPreferenceToLocalStorage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true })).ToBeVisibleAsync();

        // Act - Click the English flag
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update to English (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Assert - Check localStorage directly
        var savedLanguage = await Page.EvaluateAsync<string>("localStorage.getItem('preferredLanguage')");
        savedLanguage.Should().Be("en", "Language preference should be saved to localStorage");
    }

    [Test]
    public async Task LanguagePreference_PersistsAfterRefresh()
    {
        // Arrange - Set English and let it persist
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true })).ToBeVisibleAsync();

        // Click English flag
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update to English (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Act - Manually refresh the page
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load after refresh (app bar title, exact match)
        var englishTitleLocator = Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true });
        await Expect(englishTitleLocator).ToBeVisibleAsync();
    }

    [Test]
    public async Task LanguageSelector_SwitchFromEnglishToDutch_Works()
    {
        // Arrange - First set to English via localStorage
        await Page.GotoAsync(BaseUrl);
        await Page.EvaluateAsync("localStorage.setItem('preferredLanguage', 'en')");
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load and verify we're in English (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Act - Click the Dutch flag
        var dutchButton = Page.Locator("button[title='Nederlands']");
        await dutchButton.ClickAsync();

        // Wait for UI to update (no page reload needed) - app bar title, exact match
        var dutchTitleLocator = Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true });
        await Expect(dutchTitleLocator).ToBeVisibleAsync();
    }

    [Test]
    public async Task LanguageSelector_EnglishButton_HasCorrectVisualState()
    {
        // Arrange - Start with Dutch, then click English
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true })).ToBeVisibleAsync();

        // Click English flag to switch
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Act & Assert - English button should have selected style (opacity: 1)
        var englishStyle = await englishButton.GetAttributeAsync("style");
        englishStyle.Should().Contain("opacity: 1", "Selected flag should have full opacity");

        // Dutch button should have unselected style (opacity: 0.6)
        var dutchButton = Page.Locator("button[title='Nederlands']");
        var dutchStyle = await dutchButton.GetAttributeAsync("style");
        dutchStyle.Should().Contain("opacity: 0.6", "Unselected flag should have reduced opacity");
    }

    [Test]
    public async Task LanguageSelector_BothFlagsVisible()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Both flag buttons should be visible
        var dutchButton = Page.Locator("button[title='Nederlands']");
        var englishButton = Page.Locator("button[title='English']");

        await Expect(dutchButton).ToBeVisibleAsync();
        await Expect(englishButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task NavigationMenu_LocalizesCorrectly()
    {
        // Arrange - Start in Dutch
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true })).ToBeVisibleAsync();

        // Wait for NavMenu to re-render with translations
        await Task.Delay(1000);

        // Assert - Check Dutch navigation labels in the drawer (first nav link should be Home)
        // Use text content to debug what's actually rendered
        var firstNavLink = Page.Locator(".mud-nav-link").First;
        var firstLinkText = await firstNavLink.TextContentAsync();
        Console.WriteLine($"DEBUG: First nav link text: '{firstLinkText}'");

        // The first nav link should contain "Startpagina" in Dutch
        firstLinkText.Should().Contain("Startpagina", $"First nav link should be 'Startpagina' but was '{firstLinkText}'");

        // Act - Switch to English
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Wait for NavMenu to update
        await Task.Delay(500);

        // Assert - Navigation should now be in English
        var englishFirstLinkText = await firstNavLink.TextContentAsync();
        englishFirstLinkText.Should().Contain("Home", $"First nav link should be 'Home' but was '{englishFirstLinkText}'");
    }

    [Test]
    public async Task SignInButton_LocalizesCorrectly()
    {
        // Arrange - Start in Dutch
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for translations to load (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Vergaderdagen Tracker", Exact = true })).ToBeVisibleAsync();

        // Assert - Check Dutch sign in button in the app bar
        var signInDutchLocator = Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Link, new() { Name = "Inloggen" });
        await Expect(signInDutchLocator).ToBeVisibleAsync();

        // Act - Switch to English
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for UI to update (app bar title, exact match)
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Meeting Days Tracker", Exact = true })).ToBeVisibleAsync();

        // Assert - Sign in should now be in English (app bar button)
        var signInEnglishLocator = Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Link, new() { Name = "Sign In" });
        await Expect(signInEnglishLocator).ToBeVisibleAsync();
    }
}
