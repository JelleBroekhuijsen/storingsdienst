# Deployment Guide - Storingsdienst

This guide provides complete instructions for deploying the Storingsdienst application to Azure Web App using GitHub Actions and Bicep.

## Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Azure AD Architecture](#azure-ad-architecture)
- [One-Time Setup](#one-time-setup)
- [Deployment Process](#deployment-process)
- [Post-Deployment Configuration](#post-deployment-configuration)
- [Troubleshooting](#troubleshooting)

## Overview

The deployment uses a fully automated CI/CD pipeline with GitHub Actions and Azure Bicep templates. The application is deployed as a Blazor WebAssembly hosted application on Azure App Service.

### Architecture Components
- **Azure App Service Plan**: B1 SKU (Windows, .NET 8)
- **Azure Web App**: Hosts both Server and Client (WebAssembly)
- **Application Insights**: Monitoring and telemetry
- **GitHub Actions**: CI/CD automation
- **Bicep**: Infrastructure as Code

### Deployment URL
After deployment, the application will be accessible at:
```
https://app-storingsdienst-prod.azurewebsites.net
```

**Custom Domain**: `https://storingsdienst.jll.io` (see [Custom Domain Configuration Guide](./CUSTOM_DOMAIN.md))

## Prerequisites

Before starting, ensure you have:

- [x] Azure subscription with **Contributor** access
- [x] GitHub repository with **admin** access
- [x] Multi-tenant Azure AD App Registration created (for user authentication)
- [ ] Azure CLI installed (optional, for manual operations)

## Azure AD Architecture

This deployment uses **two separate Azure AD identities** for different purposes. Understanding this separation is critical:

### 1. Deployment Service Principal (GitHub Actions → Azure)

**Purpose**: Allows GitHub Actions to deploy infrastructure and code to your Azure subscription.

**Type**: Azure AD Service Principal with Azure RBAC permissions

**Permissions**:
- `Contributor` role on subscription or resource group
- Can create and manage Azure resources
- **NO** Microsoft Graph API access
- **NO** user authentication capabilities

**Where it's used**: GitHub Actions workflows for deploying infrastructure and application

**GitHub Secret**: `AZURE_CREDENTIALS`

### 2. Multi-tenant App Registration (End Users → Application)

**Purpose**: Allows end users from ANY organization to sign in and access their Microsoft 365 calendar data.

**Type**: Azure AD App Registration configured for multi-tenancy

**Permissions**:
- Microsoft Graph API: `User.Read` (delegated)
- Microsoft Graph API: `Calendars.Read` (delegated)
- **NO** Azure subscription access
- **NO** infrastructure deployment capabilities

**Where it's used**: Application authentication (MSAL), embedded in appsettings.json

**GitHub Secret**: `AZURE_CLIENT_ID`

### Why Separate Identities?

| Aspect | Deployment SP | Multi-tenant App Reg |
|--------|---------------|---------------------|
| **Purpose** | Deploy infrastructure | Authenticate users |
| **Permissions** | Azure RBAC | Microsoft Graph API |
| **Scope** | Azure resources | User authentication |
| **Secret Storage** | AZURE_CREDENTIALS | AZURE_CLIENT_ID |
| **Lifetime** | Permanent (for CI/CD) | Permanent (for app) |
| **Multi-tenant** | No | **Yes** |

**Important**: Never confuse these two identities. The Deployment Service Principal cannot authenticate users, and the Multi-tenant App Registration cannot deploy Azure resources.

## One-Time Setup

### Step 1: Create Deployment Service Principal

This Service Principal is used by GitHub Actions to deploy to Azure.

```bash
# Login to Azure
az login

# Set your subscription
az account set --subscription "<your-subscription-id>"

# Create Service Principal
az ad sp create-for-rbac \
  --name "sp-storingsdienst-github-actions" \
  --role contributor \
  --scopes /subscriptions/<your-subscription-id> \
  --sdk-auth
```

**Output** (copy this JSON):
```json
{
  "clientId": "00000000-0000-0000-0000-000000000000",
  "clientSecret": "your-secret-here",
  "subscriptionId": "00000000-0000-0000-0000-000000000000",
  "tenantId": "00000000-0000-0000-0000-000000000000",
  ...
}
```

**Security**: This output contains sensitive credentials. Store it securely.

### Step 2: Configure Multi-tenant App Registration

You already have a multi-tenant App Registration. Verify these settings:

#### 2.1 Authentication Platform
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Select your multi-tenant app registration
4. Go to **Authentication**
5. Under **Platform configurations**, ensure **Single-page application** is configured
6. Add the production redirect URI:
   ```
   https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback
   ```
7. If using custom domain, also add:
   ```
   https://storingsdienst.jll.io/authentication/login-callback
   ```
   (See [Custom Domain Configuration Guide](./CUSTOM_DOMAIN.md) for custom domain setup)
8. Keep your local development URI:
   ```
   https://localhost:5266/authentication/login-callback
   ```
9. Save changes

#### 2.2 API Permissions
Verify these delegated permissions are granted:
- **Microsoft Graph API**:
  - `User.Read`
  - `Calendars.Read`

#### 2.3 Multi-tenant Configuration
1. Go to **Authentication** → **Supported account types**
2. Ensure it's set to:
   - **Accounts in any organizational directory (Any Azure AD directory - Multitenant)**

#### 2.4 Copy Client ID
1. Go to **Overview**
2. Copy the **Application (client) ID**
3. This will be stored as `AZURE_CLIENT_ID` GitHub Secret

### Step 3: Configure GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret** for each of the following:

#### Required Secrets

| Secret Name | Description | Source |
|-------------|-------------|--------|
| `AZURE_CREDENTIALS` | Deployment Service Principal JSON | Output from Step 1 |
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID | Azure Portal or Step 1 output |
| `AZURE_CLIENT_ID` | Multi-tenant App Registration Client ID | Step 2.4 |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Web App publish profile (added after infrastructure deployment) | Step 5 below |

**Example for AZURE_CREDENTIALS**:
```json
{
  "clientId": "00000000-0000-0000-0000-000000000000",
  "clientSecret": "your-secret-here",
  "subscriptionId": "00000000-0000-0000-0000-000000000000",
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

## Deployment Process

### Step 4: Deploy Infrastructure (One-Time)

This creates all Azure resources (Resource Group, App Service Plan, Web App, Application Insights).

1. Go to your GitHub repository
2. Navigate to **Actions** tab
3. Select **Deploy Infrastructure** workflow
4. Click **Run workflow**
5. Type `deploy` in the confirmation field
6. Click **Run workflow**
7. Wait for completion (2-3 minutes)

**What gets created**:
- Resource Group: `rg-storingsdienst-prod`
- App Service Plan: `asp-storingsdienst-prod`
- Web App: `app-storingsdienst-prod`
- Application Insights: `appi-storingsdienst-prod`

**Outputs** (shown in workflow summary):
- Web App URL
- Resource names
- Next steps

### Step 5: Get Web App Publish Profile

After infrastructure deployment, you need the publish profile for application deployment.

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **app-storingsdienst-prod** (your Web App)
3. Click **Overview** (left menu)
4. Click **Get publish profile** (or **Download publish profile**)
5. Open the downloaded `.PublishSettings` file in a text editor
6. Copy the **entire XML content**
7. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
8. Create a new secret named `AZURE_WEBAPP_PUBLISH_PROFILE`
9. Paste the XML content
10. Save

### Step 6: Deploy Application

This builds the application, injects the Azure AD Client ID, and deploys to Azure.

#### Automatic Deployment (on push to main)
Any push to the `main` branch automatically triggers deployment (unless the changes are only to documentation).

#### Manual Deployment
1. Go to your GitHub repository
2. Navigate to **Actions** tab
3. Select **Deploy Application** workflow
4. Click **Run workflow**
5. Select branch: `main`
6. Click **Run workflow**
7. Wait for completion (4-6 minutes)

**What happens**:
1. Code is checked out
2. .NET 8 SDK is set up
3. Dependencies are restored
4. **Azure AD Client ID is injected** into `appsettings.json`
5. Solution is built
6. Server project (including WebAssembly client) is published
7. Application is deployed to Azure Web App

## Post-Deployment Configuration

### Step 7: Verify Deployment

1. Open a browser and navigate to:
   ```
   https://app-storingsdienst-prod.azurewebsites.net
   ```
2. The application should load successfully
3. You should see the Storingsdienst interface

### Step 8: Test Authentication

#### Test Multi-tenant Sign-In
1. Click **Sign In** in the navigation
2. You'll be redirected to Microsoft login
3. Sign in with **any** Microsoft 365 account (from any organization)
4. Grant consent for the requested permissions:
   - Read your profile
   - Read your calendars
5. You should be redirected back to the application
6. Your name should appear in the navigation

**Note**: Because this is a multi-tenant application, users from ANY organization can sign in without requiring separate app registrations.

#### Test JSON Import Mode
1. Use the **JSON Import** mode (default)
2. Upload the sample file: `src/Storingsdienst/Storingsdienst/wwwroot/sample-calendar-export.json`
3. Click **Process**
4. Verify the monthly breakdown is displayed
5. Click **Export to Excel**
6. Verify the Excel file downloads

### Step 9: Monitor Application

#### View Application Insights
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **appi-storingsdienst-prod**
3. Explore:
   - **Live Metrics**: Real-time telemetry
   - **Failures**: Error tracking
   - **Performance**: Response times
   - **Users**: Usage analytics

#### View Deployment History
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **app-storingsdienst-prod**
3. Go to **Deployment Center**
4. View deployment history and logs

## Continuous Deployment

After initial setup, the application automatically deploys on every push to `main`:

```bash
# Make changes to your code
git add .
git commit -m "Your changes"
git push origin main

# GitHub Actions automatically:
# 1. Builds the application
# 2. Injects Azure AD Client ID
# 3. Deploys to Azure
# 4. Application is live within 4-6 minutes
```

## Troubleshooting

### Infrastructure Deployment Issues

#### Error: "The client 'X' does not have authorization"
**Cause**: Service Principal lacks Contributor permissions

**Solution**:
```bash
# Grant Contributor role
az role assignment create \
  --assignee <service-principal-client-id> \
  --role Contributor \
  --scope /subscriptions/<subscription-id>
```

#### Error: "Location not valid"
**Cause**: Invalid Azure region specified

**Solution**: Verify `location` parameter in `infra/parameters/prod.bicepparam`

### Application Deployment Issues

#### Error: "Secret 'AZURE_CLIENT_ID' not found"
**Cause**: Missing GitHub Secret

**Solution**: Add the Multi-tenant App Registration Client ID as `AZURE_CLIENT_ID` secret

#### Error: "Publish profile is invalid"
**Cause**: Incorrect or expired publish profile

**Solution**:
1. Download a fresh publish profile from Azure Portal
2. Update the `AZURE_WEBAPP_PUBLISH_PROFILE` secret

### Authentication Issues

#### Error: "AADSTS50011: Reply URL mismatch"
**Cause**: Production redirect URI not added to App Registration

**Solution**:
1. Go to Azure Portal → App registrations → Your multi-tenant app
2. Go to **Authentication**
3. Add: `https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback`
4. Save

#### Error: "AADSTS50020: User account does not exist in tenant"
**Cause**: Attempting to sign in with a personal Microsoft account (multi-tenant is configured for organizational accounts only)

**Solution**: Multi-tenant apps support **organizational accounts** (Azure AD). If you need personal Microsoft accounts, update the app registration:
1. Go to **Authentication** → **Supported account types**
2. Select: **Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)**

### Application Issues

#### Application loads but shows errors
**Check Application Insights**:
1. Go to Azure Portal → **appi-storingsdienst-prod**
2. Go to **Failures**
3. Review recent exceptions

**Check App Service Logs**:
1. Go to Azure Portal → **app-storingsdienst-prod**
2. Go to **Log stream**
3. View real-time logs

#### JSON Import not working
**Verify file format**: Ensure the JSON file follows the Power Automate export schema:
```json
{
  "value": [
    {
      "subject": "Meeting Title",
      "start": {
        "dateTime": "2024-01-15T10:00:00",
        "timeZone": "UTC"
      },
      "end": {
        "dateTime": "2024-01-15T11:00:00",
        "timeZone": "UTC"
      }
    }
  ]
}
```

### Custom Domain Issues

For issues related to the custom domain `storingsdienst.jll.io`, see the dedicated [Custom Domain Configuration Guide](./CUSTOM_DOMAIN.md) which includes:
- DNS configuration troubleshooting
- SSL certificate issues
- Domain validation problems
- Authentication redirect URI mismatches

## Rollback

If a deployment introduces issues, you can rollback:

### Option 1: Redeploy Previous Version
1. Azure Portal → **app-storingsdienst-prod**
2. Go to **Deployment Center**
3. Find the previous successful deployment
4. Click **Redeploy**

### Option 2: Revert Git Commit
```bash
git revert HEAD
git push origin main
# GitHub Actions automatically deploys the reverted code
```

## Cost Management

### Current Costs (EUR/month)
- App Service Plan (B1): €13
- Application Insights: €2-5 (5GB free, then pay-as-you-go)
- **Total**: €15-18/month

### Reducing Costs
**Pause during non-business hours**:
- Stop the Web App when not in use
- Restart when needed
- App Service Plan charges continue, but resources are freed

**Downgrade to Free tier** (for testing):
- Change SKU to `F1` in `infra/parameters/prod.bicepparam`
- Limitations: No Always On, no custom domains, no managed certificates, shared resources

### Increasing Capacity
**Scale up** (more powerful instances):
- Edit `infra/parameters/prod.bicepparam`
- Change `sku` to `S1`, `S2`, `P1V2`, etc.
- Redeploy infrastructure

**Scale out** (more instances):
- Azure Portal → App Service Plan → Scale out
- Add multiple instances for high availability

## Security Best Practices

1. **Keep Secrets Secure**: Never commit secrets to Git
2. **Rotate Credentials**: Regularly rotate Service Principal secrets
3. **Review Permissions**: Audit who has access to GitHub Secrets
4. **Monitor Access**: Use Application Insights to detect unusual activity
5. **Update Dependencies**: Keep NuGet packages up to date
6. **Use HTTPS**: Always enforced in production
7. **Separate Identities**: Deployment SP ≠ Application App Registration

## Support

For deployment issues:
1. Check this guide's Troubleshooting section
2. Review GitHub Actions workflow logs
3. Check Azure Portal activity logs
4. Review Application Insights telemetry
5. Open an issue in the GitHub repository

## Appendix: Manual Deployment

If GitHub Actions is unavailable, you can deploy manually:

### Deploy Infrastructure
```bash
az login
az deployment sub create \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam
```

### Deploy Application
```bash
# Build and publish
dotnet publish src/Storingsdienst/Storingsdienst/Storingsdienst.csproj \
  --configuration Release \
  --output ./publish

# Zip the output
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group rg-storingsdienst-prod \
  --name app-storingsdienst-prod \
  --src ./app.zip
```

**Note**: Manual deployment does NOT inject the Azure AD Client ID. You must manually update `appsettings.json` before building.
