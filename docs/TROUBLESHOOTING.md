# Troubleshooting Guide

## Known Issues and Solutions

### Arg_GetMethNotFnd Error in Azure (Fixed)

**Issue**: When running the application in Azure, clicking "Search Meetings" resulted in an error:
```
Error searching meetings: Arg_GetMethNotFnd
```

This error occurred in production (Azure) but not in local development.

**Root Cause**: 
Blazor WebAssembly uses IL trimming in production builds to reduce bundle size. The original code in `MeetingAnalysisService.cs` used tuple deconstruction syntax with LINQ:

```csharp
foreach (var ((year, month), days) in meetingDaysByMonth.OrderByDescending(x => x.Key.Year).ThenByDescending(x => x.Key.Month))
```

This pattern required runtime reflection metadata that was being removed by the IL trimmer, causing the `Arg_GetMethNotFnd` exception.

**Solution** (Fixed in commit 14664af):
Refactored the code to explicitly access KeyValuePair properties instead of using tuple deconstruction:

```csharp
var sortedMonths = meetingDaysByMonth
    .OrderByDescending(x => x.Key.Year)
    .ThenByDescending(x => x.Key.Month);

foreach (var monthEntry in sortedMonths)
{
    var year = monthEntry.Key.Year;
    var month = monthEntry.Key.Month;
    var days = monthEntry.Value;
    // ... rest of the code
}
```

This approach is fully compatible with IL trimming while maintaining identical functionality.

**Testing**:
- All 113 unit tests pass
- Release build with IL trimming completes successfully
- No behavioral changes to the application

**Prevention**:
When writing Blazor WebAssembly code, avoid:
- Tuple deconstruction in foreach loops with complex types
- Deep reflection patterns that may not survive IL trimming
- Dynamic code generation that relies on runtime metadata

Always test with `dotnet publish -c Release` to catch IL trimming issues before deployment.

## General Azure Deployment Issues

### Build Configuration
The application uses the following settings for Azure deployment:
- Target Framework: .NET 8.0
- Blazor WebAssembly standalone
- IL trimming enabled in Release builds
- No ahead-of-time (AOT) compilation

### Deployment Checklist
1. Ensure all NuGet packages are compatible with IL trimming
2. Test locally with `dotnet publish -c Release`
3. Run all unit tests in Release configuration
4. Verify no trimming warnings in build output
5. Test critical user flows after deployment

## Performance Considerations

### Holiday Service Caching
The `HolidayService` caches holiday calculations per year to avoid redundant computations. This cache is stored in memory and will be cleared when the application reloads.

### Large Calendar Data
For users with extensive calendar history (thousands of events):
- The Graph API has pagination built-in (max 1000 events per request)
- Client-side processing is done in-memory
- Consider filtering by date range to reduce data volume

## Browser Compatibility

The application requires a modern browser with WebAssembly support:
- Chrome/Edge 91+
- Firefox 89+
- Safari 15+

Internet Explorer is not supported.
