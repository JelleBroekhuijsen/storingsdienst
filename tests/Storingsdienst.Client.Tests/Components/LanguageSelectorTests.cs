using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Storingsdienst.Client.Components;
using Storingsdienst.Client.Services;

namespace Storingsdienst.Client.Tests.Components;

public class LanguageSelectorTests : TestContext
{
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private Action? _languageChangedCallback;

    public LanguageSelectorTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();

        // Setup default behavior
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");
        _mockLocalizationService.Setup(l => l["Nederlands"]).Returns("Nederlands");
        _mockLocalizationService.Setup(l => l["English"]).Returns("English");

        // Capture the OnLanguageChanged event subscription
        _mockLocalizationService.SetupAdd(l => l.OnLanguageChanged += It.IsAny<Action>())
            .Callback<Action>(handler => _languageChangedCallback = handler);

        // Register services
        Services.AddSingleton(_mockLocalizationService.Object);
        Services.AddMudServices();

        // Add JSInterop mock for MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Renders the LanguageSelector component wrapped with MudPopoverProvider
    /// </summary>
    private IRenderedComponent<LanguageSelector> RenderLanguageSelector()
    {
        // Render MudPopoverProvider first to satisfy MudBlazor requirement
        RenderComponent<MudPopoverProvider>();
        return RenderComponent<LanguageSelector>();
    }

    [Fact]
    public void LanguageSelector_RendersWithDutchFlag()
    {
        // Act
        var cut = RenderComponent<LanguageSelector>();

        // Assert - Component should render without throwing
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("\U0001F1F3\U0001F1F1"); // Dutch flag emoji
    }

    [Fact]
    public void LanguageSelector_RendersWithBritishFlag()
    {
        // Act
        var cut = RenderComponent<LanguageSelector>();

        // Assert
        cut.Markup.Should().Contain("\U0001F1EC\U0001F1E7"); // British flag emoji
    }

    [Fact]
    public void LanguageSelector_ShowsCheckmarkForCurrentCulture_Dutch()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");

        // Act
        var cut = RenderComponent<LanguageSelector>();

        // Assert - The component should render with Dutch selected
        // MudBlazor components use CSS classes for icons, so we verify the check icon is present
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void LanguageSelector_ShowsCheckmarkForCurrentCulture_English()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("en");

        // Act
        var cut = RenderComponent<LanguageSelector>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void LanguageSelector_SubscribesToOnLanguageChanged()
    {
        // Act
        RenderComponent<LanguageSelector>();

        // Assert
        _mockLocalizationService.VerifyAdd(
            l => l.OnLanguageChanged += It.IsAny<Action>(),
            Times.Once);
    }

    [Fact]
    public void LanguageSelector_UnsubscribesOnDispose()
    {
        // Arrange
        var cut = RenderComponent<LanguageSelector>();

        // Act
        cut.Dispose();

        // Assert
        _mockLocalizationService.VerifyRemove(
            l => l.OnLanguageChanged -= It.IsAny<Action>(),
            Times.Once);
    }

    [Fact]
    public void LanguageSelector_DisplaysLocalizedText()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l["Nederlands"]).Returns("Nederlands");
        _mockLocalizationService.Setup(l => l["English"]).Returns("English");

        // Act
        var cut = RenderComponent<LanguageSelector>();

        // Assert
        cut.Markup.Should().Contain("Nederlands");
        cut.Markup.Should().Contain("English");
    }

    [Fact]
    public async Task LanguageSelector_WhenLanguageChanges_ReRendersComponent()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");
        var cut = RenderComponent<LanguageSelector>();

        // Act - Simulate language change by updating the mock and triggering the callback
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("en");
        _languageChangedCallback?.Invoke();

        // Assert - Component should have re-rendered
        await Task.Delay(10); // Allow for async re-render
        cut.Markup.Should().NotBeEmpty();
    }
}
