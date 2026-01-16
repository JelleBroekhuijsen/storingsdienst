using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Storingsdienst.E2E.Tests;

[TestFixture]
public class LanguageSwitchingTests : PageTest
{
    private const string BaseUrl = "http://localhost:5266";

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

        // Assert - Default should be Dutch
        var appTitle = await Page.Locator("text=Vergaderdagen Tracker").CountAsync();
        appTitle.Should().Be(1, "App title should be shown in Dutch by default");
    }

    [Test]
    public async Task LanguageSelector_ClickEnglishFlag_ChangesToEnglish()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we start in Dutch
        var dutchTitle = await Page.Locator("text=Vergaderdagen Tracker").CountAsync();
        dutchTitle.Should().Be(1, "Should start in Dutch");

        // Act - Click the English flag button (second button in the group)
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for page reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should now show English
        var englishTitle = await Page.Locator("text=Meeting Days Tracker").CountAsync();
        englishTitle.Should().Be(1, "App title should be in English after clicking English flag");
    }

    [Test]
    public async Task LanguageSelector_ClickEnglishFlag_SavesPreferenceToLocalStorage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Click the English flag
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait a moment for localStorage write
        await Task.Delay(100);

        // Assert - Check localStorage directly (before page reloads)
        var savedLanguage = await Page.EvaluateAsync<string>("localStorage.getItem('preferredLanguage')");
        savedLanguage.Should().Be("en", "Language preference should be saved to localStorage");
    }

    [Test]
    public async Task LanguagePreference_PersistsAfterRefresh()
    {
        // Arrange - Set English and let it persist
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Click English flag
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for navigation to complete
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Manually refresh the page
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should still be in English
        var englishTitle = await Page.Locator("text=Meeting Days Tracker").CountAsync();
        englishTitle.Should().Be(1, "English should persist after page refresh");
    }

    [Test]
    public async Task LanguageSelector_SwitchFromEnglishToDutch_Works()
    {
        // Arrange - First set to English via localStorage
        await Page.GotoAsync(BaseUrl);
        await Page.EvaluateAsync("localStorage.setItem('preferredLanguage', 'en')");
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're in English
        var englishTitle = await Page.Locator("text=Meeting Days Tracker").CountAsync();
        englishTitle.Should().Be(1, "Should start in English");

        // Act - Click the Dutch flag
        var dutchButton = Page.Locator("button[title='Nederlands']");
        await dutchButton.ClickAsync();

        // Wait for page reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should now show Dutch
        var dutchTitle = await Page.Locator("text=Vergaderdagen Tracker").CountAsync();
        dutchTitle.Should().Be(1, "App title should be in Dutch after clicking Dutch flag");
    }

    [Test]
    public async Task LanguageSelector_EnglishButton_HasCorrectVisualState()
    {
        // Arrange - Set to English
        await Page.GotoAsync(BaseUrl);
        await Page.EvaluateAsync("localStorage.setItem('preferredLanguage', 'en')");
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act & Assert - English button should have selected style (opacity: 1)
        var englishButton = Page.Locator("button[title='English']");
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

        // Assert - Check Dutch navigation labels
        var homeLink = await Page.Locator("text=Startpagina").CountAsync();
        homeLink.Should().Be(1, "Home link should be in Dutch");

        // Act - Switch to English
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Navigation should now be in English
        var englishHomeLink = await Page.Locator("text=Home").CountAsync();
        englishHomeLink.Should().Be(1, "Home link should be in English after switching");
    }

    [Test]
    public async Task SignInButton_LocalizesCorrectly()
    {
        // Arrange - Start in Dutch
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Check Dutch sign in
        var signInDutch = await Page.Locator("text=Inloggen").CountAsync();
        signInDutch.Should().Be(1, "Sign in button should be in Dutch");

        // Act - Switch to English
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Sign in should now be in English
        var signInEnglish = await Page.Locator("text=Sign In").CountAsync();
        signInEnglish.Should().Be(1, "Sign in button should be in English after switching");
    }
}
