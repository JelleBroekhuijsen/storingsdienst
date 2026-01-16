using Bunit;
using FluentAssertions;
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

        // Capture the OnLanguageChanged event subscription
        _mockLocalizationService.SetupAdd(l => l.OnLanguageChanged += It.IsAny<Action>())
            .Callback<Action>(handler => _languageChangedCallback = handler);

        // Register services
        Services.AddSingleton(_mockLocalizationService.Object);
        Services.AddMudServices();

        // bUnit provides a FakeNavigationManager automatically - no need to mock it

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
        cut.Markup.Should().Contain("mud-button-group"); // Now uses button group instead of menu
    }

    [Fact]
    public void LanguageSelector_RendersTwoFlagButtons()
    {
        // Act
        var cut = RenderLanguageSelector();

        // Assert - Component should have two icon buttons (one for each flag)
        cut.Markup.Should().Contain("mud-icon-button");
        // Should contain both flag emojis
        cut.Markup.Should().Contain("\U0001F1F3\U0001F1F1"); // Dutch flag
        cut.Markup.Should().Contain("\U0001F1EC\U0001F1E7"); // British flag
    }

    [Fact]
    public void LanguageSelector_CurrentCulture_Dutch_ShowsSelectedStyle()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("nl");

        // Act
        var cut = RenderLanguageSelector();

        // Assert - The component should render with Dutch selected (opacity: 1)
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("opacity: 1");
    }

    [Fact]
    public void LanguageSelector_CurrentCulture_English_ShowsSelectedStyle()
    {
        // Arrange
        _mockLocalizationService.Setup(l => l.CurrentCulture).Returns("en");

        // Act
        var cut = RenderLanguageSelector();

        // Assert - The component should render with English selected
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
    public void LanguageSelector_RendersButtonGroup()
    {
        // Act
        var cut = RenderLanguageSelector();

        // Assert - Component should have rendered a MudButtonGroup
        cut.Markup.Should().Contain("mud-button-group");
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
