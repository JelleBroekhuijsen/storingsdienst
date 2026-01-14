## Overview
This PR migrates the Storingsdienst application from Bootstrap to MudBlazor, implementing a modern Material Design UI with a consistent visual identity.

## What Changed

### Package & Configuration
- Added MudBlazor 7.x package to `Storingsdienst.Client.csproj`
- Configured MudBlazor services in `Program.cs`
- Added global MudBlazor imports to `_Imports.razor`
- Replaced Bootstrap CSS/JS references with MudBlazor in `App.razor`

### Layout Components
- **MainLayout.razor**: Complete rewrite using `MudLayout`, `MudAppBar`, and `MudDrawer` with custom theme
- **NavMenu.razor**: Migrated to `MudNavMenu` component

### Page Migrations
- **Home.razor**: Converted to MudBlazor components (`MudTable`, `MudToggleGroup`, `MudFileUpload`, etc.)
- **PowerAutomateGuide.razor**: Replaced custom CSS timeline with `MudTimeline`
- **Authentication.razor**: Updated loading/error states with `MudProgressCircular` and `MudAlert`
- **PaystubVerification.razor**: Migrated form controls and verification results UI

### Styling & Cleanup
- Simplified `app.css` to minimal Blazor error styles
- Removed component-specific CSS files (`.razor.css`)
- Deleted Bootstrap assets from `wwwroot/bootstrap/`

## Custom Theme
Applied a professional neutral theme throughout:
- **Primary**: `#6b6c6b` (Neutral grey)
- **Secondary**: `#cf9b67` (Warm copper/bronze)
- **AppBar**: `#5a5b5a` (Darker grey)

## Impact
- 28 files changed (+1,127 / -1,543 lines)
- Consistent Material Design experience across all pages
- Improved component reusability and maintainability
- Reduced custom CSS maintenance