<#
.SYNOPSIS
    Creates a multi-tenant Azure AD app registration for Storingsdienst.

.DESCRIPTION
    This script creates an Azure AD app registration configured for:
    - Multi-tenant authentication (users from any Azure AD tenant can sign in)
    - SPA (Single Page Application) redirect URIs
    - Microsoft Graph API permissions: User.Read, Calendars.Read (delegated)
    - ID token issuance enabled

.PARAMETER AppName
    The display name for the app registration. Default: "Storingsdienst"

.PARAMETER TenantId
    The Azure AD tenant ID to create the app registration in. If not specified, the script
    will prompt you to select from available tenants.

.PARAMETER RedirectUri
    The SPA redirect URI for local development. Default: "https://localhost:5266/authentication/login-callback"

.PARAMETER ProductionUri
    Optional production redirect URI to add alongside the development URI.

.EXAMPLE
    .\Register-AzureAdApp.ps1
    Prompts for tenant selection, then creates app with default settings.

.EXAMPLE
    .\Register-AzureAdApp.ps1 -TenantId "contoso.onmicrosoft.com"
    Creates app in the specified tenant.

.EXAMPLE
    .\Register-AzureAdApp.ps1 -AppName "My Calendar Tracker" -ProductionUri "https://myapp.azurewebsites.net/authentication/login-callback"
    Creates app with custom name and adds production redirect URI.

.NOTES
    Prerequisites:
    - Azure CLI installed (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
    - User must be signed in: az login
    - User must have permissions to create app registrations in Azure AD
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$AppName = "Storingsdienst",

    [Parameter()]
    [string]$TenantId = "",

    [Parameter()]
    [string]$RedirectUri = "https://localhost:5266/authentication/login-callback",

    [Parameter()]
    [string]$ProductionUri = ""
)

# Don't stop on non-terminating errors (like warnings from az cli)
$ErrorActionPreference = "Continue"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Storingsdienst Azure AD App Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
try {
    $null = az version 2>$null
}
catch {
    Write-Host "Error: Azure CLI is not installed or not in PATH." -ForegroundColor Red
    Write-Host "Please install Azure CLI from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if user is logged in
Write-Host "Checking Azure CLI login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "You are not logged in to Azure CLI." -ForegroundColor Red
    Write-Host "Please run: az login" -ForegroundColor Yellow
    exit 1
}

Write-Host "Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host ""

# Determine which tenant to use
if ($TenantId) {
    # Tenant ID provided as parameter
    $selectedTenantId = $TenantId
    Write-Host "Using specified tenant: $selectedTenantId" -ForegroundColor Green
}
else {
    # Use the tenant from current login context
    $selectedTenantId = $account.tenantId
    Write-Host "Using current tenant: $selectedTenantId" -ForegroundColor Green
    Write-Host ""
    Write-Host "Tip: To use a different tenant, either:" -ForegroundColor Gray
    Write-Host "  - Run: az login --tenant <tenant-id-or-domain>" -ForegroundColor Gray
    Write-Host "  - Or pass: -TenantId <tenant-id-or-domain> to this script" -ForegroundColor Gray
}

Write-Host ""

# Build redirect URIs array
$redirectUris = @($RedirectUri)
if ($ProductionUri) {
    $redirectUris += $ProductionUri
}

# Microsoft Graph API ID (constant)
$graphApiId = "00000003-0000-0000-c000-000000000000"

# Permission IDs for Microsoft Graph
# User.Read = e1fe6dd8-ba31-4d61-89e7-88639da4683d
# Calendars.Read = 465a38f9-76ea-45b9-9f34-9e8b0d4b0b42
$userReadPermissionId = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
$calendarsReadPermissionId = "465a38f9-76ea-45b9-9f34-9e8b0d4b0b42"

Write-Host "Creating app registration: $AppName" -ForegroundColor Yellow
Write-Host "Redirect URIs: $($redirectUris -join ', ')" -ForegroundColor Gray
Write-Host ""

# Create the app registration with multi-tenant support
# signInAudience: AzureADMultipleOrgs = multi-tenant (any Azure AD tenant)
$appManifest = @{
    displayName = $AppName
    signInAudience = "AzureADMultipleOrgs"
    spa = @{
        redirectUris = $redirectUris
    }
    requiredResourceAccess = @(
        @{
            resourceAppId = $graphApiId
            resourceAccess = @(
                @{
                    id = $userReadPermissionId
                    type = "Scope"
                }
                @{
                    id = $calendarsReadPermissionId
                    type = "Scope"
                }
            )
        }
    )
}

$manifestJson = $appManifest | ConvertTo-Json -Depth 10 -Compress

# Create a temporary file for the manifest (Azure CLI requires file input for complex JSON)
$tempFile = [System.IO.Path]::GetTempFileName()
$manifestJson | Out-File -FilePath $tempFile -Encoding utf8

try {
    # Check if an app with this name already exists
    Write-Host "Checking for existing app registration..." -ForegroundColor Yellow
    $existingApps = az ad app list --display-name $AppName --query "[].{appId:appId, id:id, displayName:displayName}" 2>$null | ConvertFrom-Json

    if ($existingApps -and $existingApps.Count -gt 0) {
        $existingApp = $existingApps[0]
        Write-Host ""
        Write-Host "Found existing app registration: $($existingApp.displayName)" -ForegroundColor Yellow
        Write-Host "  App (Client) ID: $($existingApp.appId)" -ForegroundColor Cyan
        Write-Host ""

        $response = Read-Host "Do you want to update this existing app? (Y/n)"
        if ($response -eq 'n' -or $response -eq 'N') {
            Write-Host "Operation cancelled. To create a new app, use -AppName with a different name." -ForegroundColor Yellow
            exit 0
        }

        $appId = $existingApp.appId
        Write-Host ""
        Write-Host "Updating existing app registration..." -ForegroundColor Yellow
    }
    else {
        # Create the app registration in the selected tenant
        Write-Host "Creating app registration in tenant $selectedTenantId..." -ForegroundColor Yellow

        $createOutput = az ad app create --display-name $AppName `
            --sign-in-audience "AzureADMultipleOrgs" `
            --enable-id-token-issuance true `
            2>&1

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create app registration: $createOutput"
        }

        $basicApp = $createOutput | ConvertFrom-Json
        $appId = $basicApp.appId
        Write-Host "Created new app with ID: $appId" -ForegroundColor Green
    }

    # Update with SPA redirect URIs using az ad app update
    Write-Host "Configuring SPA redirect URIs..." -ForegroundColor Yellow
    # Get the object ID needed for the update
    $appDetails = az ad app show --id $appId | ConvertFrom-Json
    $objectId = $appDetails.id

    # Create a temp file with the SPA configuration
    $spaConfig = @{ spa = @{ redirectUris = $redirectUris } }
    $spaConfigFile = [System.IO.Path]::GetTempFileName() + ".json"
    $spaConfig | ConvertTo-Json -Depth 5 | Out-File -FilePath $spaConfigFile -Encoding utf8

    # Use az rest with file input to avoid escaping issues
    $graphUri = "https://graph.microsoft.com/v1.0/applications/$objectId"
    az rest --method PATCH --uri $graphUri --body "@$spaConfigFile" 2>$null
    Remove-Item $spaConfigFile -Force -ErrorAction SilentlyContinue

    # Add required permissions
    Write-Host "Adding Microsoft Graph permissions..." -ForegroundColor Yellow
    $null = az ad app permission add --id $appId --api $graphApiId --api-permissions "$userReadPermissionId=Scope" 2>&1
    $null = az ad app permission add --id $appId --api $graphApiId --api-permissions "$calendarsReadPermissionId=Scope" 2>&1

    # Refresh app details
    $app = az ad app show --id $appId | ConvertFrom-Json

    $clientId = $app.appId

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  App Registration Created Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "App Name:     $AppName" -ForegroundColor White
    Write-Host "Client ID:    $clientId" -ForegroundColor Cyan
    Write-Host "Object ID:    $($app.id)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Redirect URIs:" -ForegroundColor White
    foreach ($uri in $redirectUris) {
        Write-Host "  - $uri" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "API Permissions (Delegated):" -ForegroundColor White
    Write-Host "  - Microsoft Graph: User.Read" -ForegroundColor Gray
    Write-Host "  - Microsoft Graph: Calendars.Read" -ForegroundColor Gray
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "  Next Steps" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Update your appsettings.json with the Client ID:" -ForegroundColor White
    Write-Host ""
    Write-Host "   {" -ForegroundColor Gray
    Write-Host "     `"AzureAd`": {" -ForegroundColor Gray
    Write-Host "       `"Authority`": `"https://login.microsoftonline.com/common`"," -ForegroundColor Gray
    Write-Host "       `"ClientId`": `"$clientId`"," -ForegroundColor Cyan
    Write-Host "       `"ValidateAuthority`": true" -ForegroundColor Gray
    Write-Host "     }" -ForegroundColor Gray
    Write-Host "   }" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Run the application and test sign-in with any Azure AD account." -ForegroundColor White
    Write-Host ""
    Write-Host "3. Users will be prompted to consent to the following permissions:" -ForegroundColor White
    Write-Host "   - Sign in and read user profile (User.Read)" -ForegroundColor Gray
    Write-Host "   - Read user calendars (Calendars.Read)" -ForegroundColor Gray
    Write-Host ""

    # Output just the client ID for easy scripting
    Write-Host "CLIENT_ID=$clientId" -ForegroundColor DarkGray
}
catch {
    Write-Host ""
    Write-Host "Error creating app registration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "1. Ensure you have permissions to create app registrations in Azure AD" -ForegroundColor Gray
    Write-Host "2. Try running: az login --allow-no-subscriptions" -ForegroundColor Gray
    Write-Host "3. Check if an app with the same name already exists" -ForegroundColor Gray
    exit 1
}
finally {
    # Clean up temp file
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
}
