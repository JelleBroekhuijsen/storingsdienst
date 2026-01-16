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
    public void LanguageSelector_RendersWithoutErrors()
    {
        // Act
        var cut = RenderLanguageSelector();

        // Assert - Component should render without throwing
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("mud-menu"); // Verify the menu is rendered
    }

    [Fact]
    public void LanguageSelector_RendersLanguageButton()
    {
        // Act
        var cut = RenderLanguageSelector();

        // Assert - Component should have a language icon button
        cut.Markup.Should().Contain("mud-icon-button");
    }

    [Fact]
    public void LanguageSelector_CurrentCulture_Dutch()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");

        // Act
        var cut = RenderLanguageSelector();

        // Assert - The component should render without errors
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void LanguageSelector_CurrentCulture_English()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("en");

        // Act
        var cut = RenderLanguageSelector();

        // Assert - The component should render without errors
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void LanguageSelector_SubscribesToOnLanguageChanged()
    {
        // Act
        RenderLanguageSelector();

        // Assert
        _mockLocalizationService.VerifyAdd(
            l => l.OnLanguageChanged += It.IsAny<Action>(),
            Times.Once);
    }

    [Fact]
    public void LanguageSelector_UnsubscribesOnDispose()
    {
        // Arrange
        _mockLocalizationService.SetupRemove(l => l.OnLanguageChanged -= It.IsAny<Action>());
        var cut = RenderLanguageSelector();

        // Act
        cut.Instance.Dispose();

        // Assert
        _mockLocalizationService.VerifyRemove(
            l => l.OnLanguageChanged -= It.IsAny<Action>(),
            Times.Once);
    }

    [Fact]
    public void LanguageSelector_RendersMenuComponent()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l["Nederlands"]).Returns("Nederlands");
        _mockLocalizationService.Setup(l => l["English"]).Returns("English");

        // Act
        var cut = RenderLanguageSelector();

        // Assert - Component should have rendered a MudMenu
        cut.Markup.Should().Contain("mud-menu");
    }

    [Fact]
    public void LanguageSelector_WhenLanguageChanges_ReRendersComponent()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");
        var cut = RenderLanguageSelector();

        // Act - Simulate language change by updating the mock and triggering the callback on the test renderer dispatcher
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("en");
        if (_languageChangedCallback != null)
        {
            cut.InvokeAsync(() => _languageChangedCallback.Invoke());
        }

        // Assert - Component should have re-rendered without errors
        cut.Markup.Should().NotBeEmpty();
    }
}
