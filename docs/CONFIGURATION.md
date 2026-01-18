# Configuration Guide

## Overview

This guide explains how to configure the Storingsdienst application for local development and production environments.

## Microsoft Entra Client ID Configuration

The application requires an Microsoft Entra Client ID for Microsoft 365 authentication.

### Configuration Pattern

The repository uses a tiered configuration approach:

1. **`appsettings.json`** (committed to Git): Contains a default development/demo ClientId (`23ab765f-261f-4d10-8cc1-a4ca34a0ad38`). This shared value allows developers to quickly test the application without immediate configuration.

2. **`appsettings.Development.json`** (gitignored): Developers should create this file locally with their personal ClientId to override the default during development.

3. **Production deployment**: The deployment workflow automatically replaces the ClientId with the production value from GitHub Secrets (`AZURE_CLIENT_ID`) before building and deploying.

**Important**: While `appsettings.json` contains a default ClientId for development convenience, production secrets should **never** be committed to version control. Always use GitHub Secrets for production values.

### For Local Development

1. **Register an Microsoft Entra Application** (if not already done):
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

2. **Use appsettings.Development.json** (Recommended):
   
   Create a development-specific configuration file with your personal ClientId. This file is gitignored and won't be committed:
   
   Create `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.Development.json`:
   
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
   
   This file will override the default development ClientId from `appsettings.json` during local development.
   
   **Note**: You can also use the default development ClientId already in `appsettings.json` for quick testing, but it's recommended to use your own ClientId for actual development work.

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
- Use default/demo ClientId values in `appsettings.json` for shared development convenience
- Store personal development Client IDs in `appsettings.Development.json` (gitignored)
- Use GitHub Secrets for production values
- Use separate Microsoft Entra app registrations for development and production
- Use multi-tenant app registrations to support users from any organization

### ❌ DON'T:
- Commit production Client IDs to Git
- Share production Client IDs in public documentation
- Use production Client IDs for local development
- Hardcode production secrets in source code

## Configuration Files

### `appsettings.json` (Committed to Git)
Contains **default configuration with a shared development ClientId**. This file is committed to version control and contains a demo/development ClientId that allows developers to quickly test the application.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.json`

**Purpose**: Provides base configuration structure and a default development ClientId. The deployment workflow replaces this value with the production ClientId during deployment.

### `appsettings.Development.json` (Gitignored)
Contains **development-specific overrides**. This file is automatically excluded from version control via `.gitignore`.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.Development.json`

**Purpose**: Override the default development ClientId with your personal Microsoft Entra Client ID for actual development work. Optional if you're just testing with the default ClientId.

### `appsettings.example.json` (Committed to Git)
Contains **example configuration** showing the required structure. Currently identical to `appsettings.json`.

**Location**: `src/Storingsdienst/Storingsdienst.Client/wwwroot/appsettings.example.json`

**Purpose**: Documentation and reference for developers to understand the expected configuration format.

## Configuration Hierarchy

Blazor WebAssembly configuration is loaded in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific, e.g., `appsettings.Development.json`)

During local development with `dotnet run`, the environment is automatically set to `Development`, so your `appsettings.Development.json` values will be used.

## Troubleshooting

### "AADSTS700016: Application with identifier '23ab765f-261f-4d10-8cc1-a4ca34a0ad38' was not found"

**Cause**: The default development ClientId in `appsettings.json` doesn't have the necessary Microsoft Entra app registration permissions, or you need to use your own ClientId.

**Solution**: Create `appsettings.Development.json` with your personal Microsoft Entra Client ID following the "For Local Development" steps above.

### "AADSTS50011: Reply URL mismatch"

**Cause**: The redirect URI in your Microsoft Entra app registration doesn't match your application URL.

**Solution**: 
1. Note your application URL (e.g., `https://localhost:7123`)
2. Go to Azure Portal → App registrations → Your app → Authentication
3. Add redirect URI: `https://localhost:7123/authentication/login-callback`

### Authentication works locally but fails in production

**Cause**: Production redirect URI not configured in Microsoft Entra app registration.

**Solution**:
1. Go to Azure Portal → App registrations → Your multi-tenant app → Authentication
2. Add production redirect URI: `https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback`
3. If using custom domain, also add: `https://storingsdienst.jll.io/authentication/login-callback`

See [Custom Domain Configuration Guide](./CUSTOM_DOMAIN.md) for custom domain setup.

## Additional Configuration

### Microsoft Graph API Scopes

The application requires these Microsoft Graph API permissions:

- `User.Read`: Read user profile information
- `Calendars.Read`: Read user's calendar events

These are configured in:
- Microsoft Entra app registration (API permissions)
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
