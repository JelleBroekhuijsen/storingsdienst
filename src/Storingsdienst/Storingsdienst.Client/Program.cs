using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.Msal;
using Microsoft.Graph;
using Microsoft.Kiota.Http.HttpClientLibrary;
using MudBlazor.Services;
using Storingsdienst.Client.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Set default culture to Dutch
var defaultCulture = new CultureInfo("nl");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Register MudBlazor services
builder.Services.AddMudServices();

// Register localization services
builder.Services.AddLocalization();

// Register a default HttpClient for loading locale JSON files
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Register business logic services
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IMeetingAnalysisService, MeetingAnalysisService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<JsonImportService>();

// MSAL Authentication (multi-tenant, any Azure AD organization)
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("Calendars.Read");
});

// Register Graph API authentication provider
builder.Services.AddScoped<GraphAuthProvider>();

// Configure HttpClient for Graph API (Blazor WASM-compatible)
builder.Services.AddHttpClient("GraphClient", client =>
{
    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0");
});

// Register GraphServiceClient with our auth provider and WASM-compatible HttpClient
builder.Services.AddScoped(sp =>
{
    var authProvider = sp.GetRequiredService<GraphAuthProvider>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("GraphClient");

    // Create a KiotaClientFactory adapter for the HttpClient
    var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
    return new GraphServiceClient(requestAdapter);
});

// Register GraphService as the ICalendarDataService implementation for Graph API mode
builder.Services.AddScoped<ICalendarDataService, GraphService>();

await builder.Build().RunAsync();
