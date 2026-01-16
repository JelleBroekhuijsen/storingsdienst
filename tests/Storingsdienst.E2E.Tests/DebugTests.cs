using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Storingsdienst.E2E.Tests;

[TestFixture]
public class DebugTests : PageTest
{
    private const string BaseUrl = "https://localhost:5266";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        };
    }

    [Test]
    public async Task DebugPageContent()
    {
        // Capture console logs
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) =>
        {
            consoleMessages.Add($"[{msg.Type}] {msg.Text}");
        };

        // Navigate and wait
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get the page content
        var content = await Page.ContentAsync();

        // Print key portions
        Console.WriteLine("=== PAGE TITLE ===");
        var title = await Page.TitleAsync();
        Console.WriteLine($"Title: {title}");

        Console.WriteLine("\n=== APP BAR TEXT ===");
        var appBarText = await Page.Locator(".mud-appbar").TextContentAsync();
        Console.WriteLine($"AppBar: {appBarText}");

        Console.WriteLine("\n=== H6 Elements (App Title) ===");
        var h6Elements = await Page.Locator("h6.mud-typography").AllTextContentsAsync();
        foreach (var h6 in h6Elements)
        {
            Console.WriteLine($"H6: {h6}");
        }

        Console.WriteLine("\n=== Looking for 'Vergaderdagen' ===");
        var vergaderagenCount = await Page.Locator("text=Vergaderdagen").CountAsync();
        Console.WriteLine($"Found 'Vergaderdagen' {vergaderagenCount} times");

        Console.WriteLine("\n=== Looking for 'Meeting Days' ===");
        var meetingDaysCount = await Page.Locator("text=Meeting Days").CountAsync();
        Console.WriteLine($"Found 'Meeting Days' {meetingDaysCount} times");

        Console.WriteLine("\n=== Looking for 'Inloggen' ===");
        var inloggenCount = await Page.Locator("text=Inloggen").CountAsync();
        Console.WriteLine($"Found 'Inloggen' {inloggenCount} times");

        Console.WriteLine("\n=== All text content snippets with 'Tracker' ===");
        var trackerElements = await Page.Locator("*:has-text('Tracker')").AllTextContentsAsync();
        foreach (var el in trackerElements.Take(5))
        {
            Console.WriteLine($"Element: {el.Substring(0, Math.Min(100, el.Length))}...");
        }

        // Check if our JavaScript function exists
        Console.WriteLine("\n=== CHECK IF storingsdienst_changeLanguage EXISTS ===");
        var functionExists = await Page.EvaluateAsync<bool>("typeof window.storingsdienst_changeLanguage === 'function'");
        Console.WriteLine($"storingsdienst_changeLanguage function exists: {functionExists}");

        // Try setting localStorage and then manually reloading with Playwright
        Console.WriteLine("\n=== SETTING LOCALSTORAGE AND RELOADING WITH PLAYWRIGHT ===");
        await Page.EvaluateAsync("localStorage.setItem('preferredLanguage', 'en')");
        Console.WriteLine("localStorage set to en");
        
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Page reloaded");
        
        // Wait for page to reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        Console.WriteLine("\n=== AFTER DIRECT CALL ===");
        var appBarTextAfterDirect = await Page.Locator(".mud-appbar").TextContentAsync();
        Console.WriteLine($"AppBar After Direct Call: {appBarTextAfterDirect}");
        
        var meetingDaysAfterDirect = await Page.Locator("text=Meeting Days").CountAsync();
        Console.WriteLine($"Found 'Meeting Days' {meetingDaysAfterDirect} times");

        // Now try clicking English flag
        Console.WriteLine("\n=== CLICKING ENGLISH FLAG ===");
        var englishButton = Page.Locator("button[title='English']");
        await englishButton.ClickAsync();

        // Wait for any updates
        await Task.Delay(1000);

        Console.WriteLine("\n=== AFTER CLICK - APP BAR TEXT ===");
        var appBarTextAfter = await Page.Locator(".mud-appbar").TextContentAsync();
        Console.WriteLine($"AppBar After: {appBarTextAfter}");

        Console.WriteLine("\n=== AFTER CLICK - Looking for 'Meeting Days' ===");
        var meetingDaysCountAfter = await Page.Locator("text=Meeting Days").CountAsync();
        Console.WriteLine($"Found 'Meeting Days' {meetingDaysCountAfter} times");

        Console.WriteLine("\n=== LocalStorage value ===");
        var savedLang = await Page.EvaluateAsync<string>("localStorage.getItem('preferredLanguage')");
        Console.WriteLine($"preferredLanguage: {savedLang}");

        // Print all console messages
        Console.WriteLine("\n=== BROWSER CONSOLE MESSAGES ===");
        foreach (var msg in consoleMessages)
        {
            Console.WriteLine(msg);
        }
    }
}
