using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Moq;
using Storingsdienst.Client.Resources;
using Storingsdienst.Client.Services;
using System.Globalization;

namespace Storingsdienst.Client.Tests.Services;

public class LocalizationServiceTests
{
    private readonly Mock<IStringLocalizerFactory> _mockLocalizerFactory;
    private readonly Mock<IStringLocalizer> _mockLocalizer;
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly LocalizationService _sut;

    public LocalizationServiceTests()
    {
        _mockLocalizerFactory = new Mock<IStringLocalizerFactory>();
        _mockLocalizer = new Mock<IStringLocalizer>();
        _mockJsRuntime = new Mock<IJSRuntime>();

        // Setup factory to return the mock localizer
        _mockLocalizerFactory
            .Setup(f => f.Create(typeof(SharedResources)))
            .Returns(_mockLocalizer.Object);

        _sut = new LocalizationService(_mockLocalizerFactory.Object, _mockJsRuntime.Object);
    }

    [Fact]
    public void CurrentCulture_DefaultValue_IsDutch()
    {
        // Assert
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Fact]
    public void Indexer_ReturnsLocalizedString()
    {
        // Arrange
        var testKey = "TestKey";
        var expectedValue = "Test Value";
        var localizedString = new LocalizedString(testKey, expectedValue);
        _mockLocalizer.Setup(l => l[testKey]).Returns(localizedString);

        // Act
        var result = _sut[testKey];

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("nl")]
    [InlineData("en")]
    public async Task SetLanguageAsync_ValidCulture_UpdatesCurrentCulture(string culture)
    {
        // Arrange - First set to a different culture to ensure change happens
        if (culture == "nl")
        {
            await _sut.SetLanguageAsync("en");
        }

        // Act
        await _sut.SetLanguageAsync(culture);

        // Assert
        _sut.CurrentCulture.Should().Be(culture);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task SetLanguageAsync_InvalidCulture_DefaultsToDutch(string invalidCulture)
    {
        // Arrange - First set to English to verify the default behavior
        await _sut.SetLanguageAsync("en");

        // Act
        await _sut.SetLanguageAsync(invalidCulture);

        // Assert
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Fact]
    public async Task SetLanguageAsync_SameCulture_DoesNotTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _sut.OnLanguageChanged += () => eventTriggered = true;

        // Act - Set to the default culture (nl) again
        await _sut.SetLanguageAsync("nl");

        // Assert
        eventTriggered.Should().BeFalse();
    }

    [Fact]
    public async Task SetLanguageAsync_DifferentCulture_TriggersOnLanguageChanged()
    {
        // Arrange
        var eventTriggered = false;
        _sut.OnLanguageChanged += () => eventTriggered = true;

        // Act
        await _sut.SetLanguageAsync("en");

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task SetLanguageAsync_PersistsToLocalStorage()
    {
        // Arrange - Setup for InvokeVoidAsync (which internally uses InvokeAsync<object>)
        _mockJsRuntime.Setup(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.IsAny<object[]>()))
            .ReturnsAsync(new object());

        // Act
        await _sut.SetLanguageAsync("en");

        // Assert - Verify the JS call was made
        _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.Is<object[]>(args =>
                args.Length == 2 &&
                args[0].ToString() == "preferredLanguage" &&
                args[1].ToString() == "en")),
            Times.Once);
    }

    [Fact]
    public async Task SetLanguageAsync_LocalStorageError_ContinuesWithoutException()
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("localStorage not available"));

        // Act
        var act = () => _sut.SetLanguageAsync("en");

        // Assert
        await act.Should().NotThrowAsync();
        _sut.CurrentCulture.Should().Be("en");
    }

    [Fact]
    public async Task SetLanguageAsync_UpdatesThreadCulture()
    {
        // Act
        await _sut.SetLanguageAsync("en");

        // Assert
        CultureInfo.DefaultThreadCurrentCulture?.Name.Should().Be("en");
        CultureInfo.DefaultThreadCurrentUICulture?.Name.Should().Be("en");
    }

    [Theory]
    [InlineData("nl")]
    [InlineData("en")]
    public async Task InitializeAsync_WithSavedCulture_LoadsCulture(string savedCulture)
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync(savedCulture);

        // Need to set a different initial culture first to verify change
        if (savedCulture == "nl")
        {
            await _sut.SetLanguageAsync("en");
        }

        // Act
        await _sut.InitializeAsync();

        // Assert
        _sut.CurrentCulture.Should().Be(savedCulture);
    }

    [Fact]
    public async Task InitializeAsync_WithNoSavedCulture_KeepsDefaultDutch()
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        await _sut.InitializeAsync();

        // Assert
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Fact]
    public async Task InitializeAsync_WithEmptySavedCulture_KeepsDefaultDutch()
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        // Act
        await _sut.InitializeAsync();

        // Assert
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("invalid")]
    public async Task InitializeAsync_WithInvalidSavedCulture_KeepsDefaultDutch(string invalidCulture)
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync(invalidCulture);

        // Act
        await _sut.InitializeAsync();

        // Assert
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Fact]
    public async Task InitializeAsync_LocalStorageError_ContinuesWithoutException()
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("localStorage not available"));

        // Act
        var act = () => _sut.InitializeAsync();

        // Assert
        await act.Should().NotThrowAsync();
        _sut.CurrentCulture.Should().Be("nl");
    }

    [Fact]
    public async Task InitializeAsync_DoesNotPersistToLocalStorage()
    {
        // Arrange
        _mockJsRuntime.Setup(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync("en");

        // Act
        await _sut.InitializeAsync();

        // Assert - localStorage.setItem should not be called during initialization
        _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task OnLanguageChanged_MultipleSubscribers_AllGetNotified()
    {
        // Arrange
        var subscriber1Called = false;
        var subscriber2Called = false;
        _sut.OnLanguageChanged += () => subscriber1Called = true;
        _sut.OnLanguageChanged += () => subscriber2Called = true;

        // Act
        await _sut.SetLanguageAsync("en");

        // Assert
        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
    }

    [Fact]
    public async Task OnLanguageChanged_UnsubscribedHandler_DoesNotGetCalled()
    {
        // Arrange
        var handlerCalled = false;
        void Handler() => handlerCalled = true;
        _sut.OnLanguageChanged += Handler;
        _sut.OnLanguageChanged -= Handler;

        // Act
        await _sut.SetLanguageAsync("en");

        // Assert
        handlerCalled.Should().BeFalse();
    }
}
