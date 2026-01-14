# Configuration Guide

## Overview

This guide explains how to configure the Storingsdienst application for local development and production environments.

## Azure AD Client ID Configuration

The application requires an Azure AD Client ID for Microsoft 365 authentication. This value should **never** be hardcoded in version control.

### For Local Development

1. **Register an Azure AD Application** (if not already done):
   - Go to [Azure Portal](https://portal.azure.com)
   - Navigate to **Azure Active Directory** → **App registrations**
   - Click **New registration**
   - Configure:
     - **Name**: Storingsdienst (Dev)
     - **Supported account types**: Accounts in any organizational directory (Multitenant)
     - **Redirect URI**: Single-page application → `https://localhost:7xxx/authentication/login-callback`
   - Click **Register**
   - Add API permissions:
     - Microsoft Graph → Delegated permissions → `User.Read`
     - Microsoft Graph → Delegated permissions → `Calendars.Read`
   - Copy the **Application (client) ID** from the Overview page

2. **Configure your local appsettings.json**:
   
   Open `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.json` and replace the placeholder:
   
   ```json
   {
     "AzureAd": {
       "Authority": "https://login.microsoftonline.com/common",
       "ClientId": "YOUR_AZURE_AD_CLIENT_ID_HERE",  ← Replace this
       "ValidateAuthority": true
     }
   }
   ```
   
   With your actual Client ID:
   
   ```json
   {
     "AzureAd": {
       "Authority": "https://login.microsoftonline.com/common",
       "ClientId": "12345678-1234-1234-1234-123456789abc",  ← Your actual ID
       "ValidateAuthority": true
     }
   }
   ```

3. **Alternative: Use appsettings.Development.json** (Recommended):
   
   Instead of modifying `appsettings.json`, create a development-specific configuration that won't be committed:
   
   Create or update `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.Development.json`:
   
   ```json
   {
     "AzureAd": {
       "ClientId": "12345678-1234-1234-1234-123456789abc"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "Microsoft.AspNetCore": "Warning"
       }
     }
   }
   ```
   
   This file will override the values from `appsettings.json` during local development.

### For Production Deployment

Production deployment uses **GitHub Actions** to automatically inject the Client ID from GitHub Secrets. No manual configuration is needed.

The deployment pipeline (`deploy.yml`) performs these steps:

1. Reads `AZURE_CLIENT_ID` from GitHub Secrets
2. Injects it into `appsettings.json` during build
3. Publishes and deploys to Azure Web App

**To configure production:**

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Ensure `AZURE_CLIENT_ID` secret exists with your multi-tenant app registration Client ID
4. Every deployment to `main` branch automatically uses this secret

See [docs/DEPLOYMENT.md](./DEPLOYMENT.md) for complete deployment instructions.

## Security Best Practices

### ✅ DO:
- Use placeholder values in `appsettings.json` committed to version control
- Store actual Client IDs in `appsettings.Development.json` (gitignored)
- Use GitHub Secrets for production values
- Use separate Azure AD app registrations for development and production
- Use multi-tenant app registrations to support users from any organization

### ❌ DON'T:
- Commit actual Client IDs to Git
- Share Client IDs in public documentation
- Use production Client IDs for local development
- Hardcode sensitive configuration in source code

## Configuration Files

### `appsettings.json` (Committed to Git)
Contains **default configuration with placeholders**. This file is committed to version control.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.json`

**Purpose**: Provides base configuration structure and default values.

### `appsettings.Development.json` (Not committed)
Contains **development-specific overrides**. This file should NOT be committed to version control.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.Development.json`

**Purpose**: Override settings for local development environment, including your personal Azure AD Client ID.

### `appsettings.example.json` (Committed to Git)
Contains **example configuration** showing the required structure.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.example.json`

**Purpose**: Documentation and reference for developers.

## Configuration Hierarchy

Blazor WebAssembly configuration is loaded in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific, e.g., `appsettings.Development.json`)

During local development with `dotnet run`, the environment is automatically set to `Development`, so your `appsettings.Development.json` values will be used.

## Troubleshooting

### "AADSTS700016: Application with identifier 'YOUR_AZURE_AD_CLIENT_ID_HERE' was not found"

**Cause**: You haven't configured your Client ID yet.

**Solution**: Follow the "For Local Development" steps above to configure your appsettings.

### "AADSTS50011: Reply URL mismatch"

**Cause**: The redirect URI in your Azure AD app registration doesn't match your application URL.

**Solution**: 
1. Note your application URL (e.g., `https://localhost:7123`)
2. Go to Azure Portal → App registrations → Your app → Authentication
3. Add redirect URI: `https://localhost:7123/authentication/login-callback`

### Authentication works locally but fails in production

**Cause**: Production redirect URI not configured in Azure AD app registration.

**Solution**:
1. Go to Azure Portal → App registrations → Your multi-tenant app → Authentication
2. Add production redirect URI: `https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback`

## Additional Configuration

### Microsoft Graph API Scopes

The application requires these Microsoft Graph API permissions:

- `User.Read`: Read user profile information
- `Calendars.Read`: Read user's calendar events

These are configured in:
- Azure AD app registration (API permissions)
- `appsettings.json` (MicrosoftGraph.Scopes)

### Logging Configuration

Adjust logging levels in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.Graph": "Debug"
    }
  }
}
```

## Support

For configuration issues:
1. Check this guide's Troubleshooting section
2. Review the [README.md](../README.md)
3. See [docs/DEPLOYMENT.md](./DEPLOYMENT.md) for production setup
4. Open an issue in the GitHub repository
