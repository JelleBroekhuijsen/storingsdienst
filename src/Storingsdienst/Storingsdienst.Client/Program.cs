using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.Msal;
using Storingsdienst.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register services
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IMeetingAnalysisService, MeetingAnalysisService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<JsonImportService>();

// MSAL Authentication (for Graph API mode) - will be configured when UI is ready
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("Calendars.Read");
});

// GraphService will be registered when we set up the full authentication flow

await builder.Build().RunAsync();
