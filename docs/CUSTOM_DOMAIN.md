# Custom Domain Configuration Guide

This guide provides step-by-step instructions for configuring the custom domain `storingsdienst.jll.io` for the Storingsdienst application.

## Overview

The application supports a custom domain through Azure App Service custom domain binding with automatic managed certificate (free SSL/TLS certificate).

**Custom Domain**: `storingsdienst.jll.io`  
**Default Azure URL**: `https://app-storingsdienst-prod.azurewebsites.net`

## Prerequisites

Before configuring the custom domain, ensure you have:

- [x] Azure Web App deployed (see [DEPLOYMENT.md](./DEPLOYMENT.md))
- [x] Access to DNS management for `jll.io` domain
- [x] Azure Portal access with permissions to modify the Web App
- [x] Microsoft Entra App Registration access to update redirect URIs

## Architecture

```
User Browser
    ↓
storingsdienst.jll.io (DNS CNAME)
    ↓
app-storingsdienst-prod.azurewebsites.net (Azure Web App)
    ↓
Azure App Service (HTTPS with Managed Certificate)
```

## Configuration Steps

### Step 1: Configure DNS (CNAME Record)

You need to create a CNAME record in your DNS provider pointing to the Azure Web App's default hostname.

#### Option A: Using Your DNS Provider's Web Interface

1. Log in to your DNS provider (e.g., Cloudflare, GoDaddy, Azure DNS, etc.)
2. Navigate to DNS management for domain `jll.io`
3. Add a new CNAME record:
   - **Name/Host**: `storingsdienst`
   - **Type**: `CNAME`
   - **Value/Target**: `app-storingsdienst-prod.azurewebsites.net`
   - **TTL**: `3600` (1 hour) or `Auto`
4. Save the changes

#### Option B: Using Azure DNS (if jll.io is hosted in Azure)

```bash
# Login to Azure
az login

# Add CNAME record
az network dns record-set cname set-record \
  --resource-group <dns-zone-resource-group> \
  --zone-name jll.io \
  --record-set-name storingsdienst \
  --cname app-storingsdienst-prod.azurewebsites.net
```

#### Verify DNS Propagation

Wait 5-15 minutes for DNS propagation, then verify:

```bash
# Test DNS resolution
nslookup storingsdienst.jll.io

# Or use dig
dig storingsdienst.jll.io CNAME
```

Expected output should show:
```
storingsdienst.jll.io → app-storingsdienst-prod.azurewebsites.net
```

### Step 2: Manual Azure Portal Configuration (Initial Setup)

For the **first-time setup**, you must manually configure the custom domain in Azure Portal before running the Bicep deployment.

#### 2.1 Validate Domain Ownership

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **app-storingsdienst-prod** (your Web App)
3. In the left menu, select **Custom domains**
4. Click **Add custom domain**
5. Enter domain name: `storingsdienst.jll.io`
6. Click **Validate**
7. Azure will check for:
   - ✅ CNAME record pointing to your Web App
   - ✅ Domain ownership verification
8. If validation succeeds, click **Add**
9. The custom domain is now added **without SSL** (HTTP only)

**Note**: At this point, `http://storingsdienst.jll.io` will work but HTTPS will not yet be configured.

#### 2.2 Add Managed Certificate (Free SSL/TLS)

After adding the custom domain, enable HTTPS with a free managed certificate:

1. Still in **Custom domains** page
2. Find `storingsdienst.jll.io` in the list
3. Click on the domain name
4. In the **SSL binding** section:
   - **Certificate source**: Select **App Service Managed Certificate**
   - **TLS/SSL type**: Select **SNI SSL**
5. Click **Add binding**
6. Wait 2-5 minutes for certificate provisioning
7. Once complete, you'll see:
   - ✅ SSL State: **SNI SSL**
   - ✅ Certificate: **Managed Certificate**

**Note**: Azure automatically renews managed certificates before expiration.

### Step 3: Deploy Infrastructure with Custom Domain (Automated)

After manual Azure Portal setup is complete, you can run the Bicep deployment to automate future updates:

#### 3.1 Verify Parameters

Check that `infra/parameters/prod.bicepparam` includes:

```bicep
param customDomain = 'storingsdienst.jll.io'
```

#### 3.2 Deploy via GitHub Actions

1. Go to your GitHub repository
2. Navigate to **Actions** tab
3. Select **Deploy Infrastructure** workflow
4. Click **Run workflow**
5. Type `deploy` to confirm
6. Click **Run workflow**

The deployment will:
- ✅ Recognize the existing custom domain
- ✅ Update the hostname binding
- ✅ Output custom domain URLs in workflow summary

#### 3.3 Or Deploy Manually via Azure CLI

```bash
# Login to Azure
az login

# Deploy Bicep template
az deployment sub create \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam
```

### Step 4: Update Microsoft Entra Redirect URIs

The Microsoft Entra App Registration must include redirect URIs for the custom domain.

#### 4.1 Navigate to App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Select your multi-tenant app registration (the one used for user authentication)
4. Go to **Authentication** in the left menu

#### 4.2 Add Custom Domain Redirect URI

Under **Single-page application** platform configurations, add:

```
https://storingsdienst.jll.io/authentication/login-callback
```

**Keep existing URIs**:
- ✅ `https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback` (default Azure URL)
- ✅ `https://localhost:5266/authentication/login-callback` (local development)

**Total URIs**: You should have all three configured for maximum compatibility.

#### 4.3 Save Changes

Click **Save** at the top of the Authentication page.

### Step 5: Update GitHub Workflows (Optional Enhancement)

Update deployment workflow to reference custom domain in summary:

**File**: `.github/workflows/deploy.yml`

```yaml
- name: Display deployment summary
  run: |
    echo "### Deployment Complete! :rocket:" >> $Env:GITHUB_STEP_SUMMARY
    echo "" >> $Env:GITHUB_STEP_SUMMARY
    echo "**Application URLs:**" >> $Env:GITHUB_STEP_SUMMARY
    echo "- Custom Domain: https://storingsdienst.jll.io" >> $Env:GITHUB_STEP_SUMMARY
    echo "- Azure Default: https://app-storingsdienst-prod.azurewebsites.net" >> $Env:GITHUB_STEP_SUMMARY
```

### Step 6: Verification

#### 6.1 Test Custom Domain Access

1. Open browser to: `https://storingsdienst.jll.io`
2. Verify:
   - ✅ Application loads successfully
   - ✅ HTTPS connection (padlock icon)
   - ✅ Certificate is valid (click padlock to check)
   - ✅ No browser security warnings

#### 6.2 Test Authentication

1. Click **Sign In** in the navigation
2. Sign in with any Microsoft 365 account
3. Grant consent for permissions
4. Verify:
   - ✅ Redirect to Microsoft login works
   - ✅ After authentication, returns to `https://storingsdienst.jll.io`
   - ✅ User name appears in navigation
   - ✅ No AADSTS50011 errors (reply URL mismatch)

#### 6.3 Test Both Domains

Both domains should work simultaneously:

- ✅ `https://storingsdienst.jll.io` (custom domain)
- ✅ `https://app-storingsdienst-prod.azurewebsites.net` (default Azure domain)

Users can access either URL; authentication works on both.

## Troubleshooting

### DNS Issues

#### Error: "Domain validation failed - CNAME not found"

**Cause**: DNS record not created or not propagated yet.

**Solution**:
1. Verify CNAME record exists in your DNS provider
2. Wait 15-30 minutes for DNS propagation
3. Test DNS resolution: `nslookup storingsdienst.jll.io`
4. Try validation again in Azure Portal

#### Error: "CNAME points to wrong target"

**Cause**: CNAME target is incorrect.

**Solution**:
1. Update CNAME record to point to: `app-storingsdienst-prod.azurewebsites.net`
2. Ensure no trailing dot in the CNAME value (unless your DNS provider requires it)
3. Wait for DNS propagation

### SSL Certificate Issues

#### Error: "Unable to create managed certificate"

**Cause**: Domain not validated, or CAA records blocking certificate issuance.

**Solution**:
1. Ensure domain validation (Step 2.1) completed successfully
2. Check for CAA records in DNS:
   ```bash
   dig jll.io CAA
   ```
3. If CAA records exist, ensure they allow Azure certificates:
   ```
   jll.io. CAA 0 issue "digicert.com"
   jll.io. CAA 0 issuewild "digicert.com"
   ```
4. Wait 10-15 minutes and try again

#### Error: "Certificate stuck in 'Pending' state"

**Cause**: Azure is provisioning the certificate; this can take time.

**Solution**:
1. Wait 10-15 minutes
2. Refresh the Azure Portal page
3. If still pending after 30 minutes, remove and re-add the SSL binding

### Authentication Issues

#### Error: "AADSTS50011: Reply URL mismatch"

**Cause**: Custom domain redirect URI not added to Microsoft Entra App Registration.

**Solution**:
1. Follow Step 4 to add: `https://storingsdienst.jll.io/authentication/login-callback`
2. Ensure you clicked **Save** in Azure Portal
3. Wait 1-2 minutes for changes to propagate
4. Try signing in again

#### Error: Authentication works on default URL but not custom domain

**Cause**: Missing redirect URI for custom domain.

**Solution**:
1. Verify all three redirect URIs are configured in Microsoft Entra:
   - `https://storingsdienst.jll.io/authentication/login-callback`
   - `https://app-storingsdienst-prod.azurewebsites.net/authentication/login-callback`
   - `https://localhost:5266/authentication/login-callback`
2. Save changes and test again

### Infrastructure Deployment Issues

#### Error: "Hostname binding already exists"

**Cause**: Custom domain is already bound to the Web App.

**Solution**: This is expected behavior after manual setup. The Bicep template will update (not recreate) the binding. This is not an error.

#### Warning: "Custom domain binding requires manual certificate configuration"

**Cause**: Bicep cannot automatically provision managed certificates; this must be done manually in Azure Portal.

**Solution**: This is expected. Follow Step 2.2 to add the managed certificate in Azure Portal.

## DNS Configuration Examples

### Example: Cloudflare DNS

```
Type: CNAME
Name: storingsdienst
Target: app-storingsdienst-prod.azurewebsites.net
Proxy status: DNS only (gray cloud)
TTL: Auto
```

**Important**: Set proxy status to **DNS only** (not proxied through Cloudflare) to avoid SSL certificate issues.

### Example: Azure DNS (Azure Portal)

1. Navigate to your DNS Zone: `jll.io`
2. Click **+ Record set**
3. Configure:
   - **Name**: `storingsdienst`
   - **Type**: `CNAME`
   - **Alias**: No
   - **TTL**: 1 Hour
   - **Alias name**: `app-storingsdienst-prod.azurewebsites.net`
4. Click **OK**

### Example: GoDaddy DNS

```
Type: CNAME
Host: storingsdienst
Points to: app-storingsdienst-prod.azurewebsites.net
TTL: 1 Hour
```

## Cost Implications

**Free Components**:
- ✅ Custom domain binding: Free
- ✅ Managed certificate: Free
- ✅ Certificate auto-renewal: Free
- ✅ SNI SSL: Free (on Basic tier and above)

**No Additional Costs**: Adding a custom domain with managed certificate does not increase your Azure bill.

## Security Best Practices

1. **Always use HTTPS**: Ensure managed certificate is configured
2. **Redirect HTTP to HTTPS**: Enabled by default (`httpsOnly: true` in Bicep)
3. **Keep certificates up to date**: Azure automatically renews managed certificates
4. **Use strong TLS version**: Configured to TLS 1.2+ in Bicep (`minTlsVersion: '1.2'`)
5. **Configure both domains**: Keep default Azure URL and custom domain for redundancy

## Rollback Plan

If custom domain causes issues, you can temporarily disable it:

### Option 1: Remove Custom Domain in Azure Portal

1. Go to Azure Portal → **app-storingsdienst-prod** → **Custom domains**
2. Select `storingsdienst.jll.io`
3. Click **Delete**
4. Confirm deletion
5. Application remains accessible via: `https://app-storingsdienst-prod.azurewebsites.net`

### Option 2: Update Bicep Parameters

1. Edit `infra/parameters/prod.bicepparam`
2. Set: `param customDomain = ''` (empty string)
3. Redeploy infrastructure via GitHub Actions
4. Custom domain binding will be removed

### Option 3: DNS Level (Temporary)

1. Delete CNAME record for `storingsdienst.jll.io`
2. Custom domain becomes unreachable
3. Default Azure URL continues to work

## Maintenance

### Monitoring

- **Certificate Expiration**: Azure automatically renews 30 days before expiration
- **DNS Health**: Monitor DNS resolution with uptime tools
- **Application Insights**: Track requests by hostname to see custom domain usage

### Updates

When the default Azure Web App name changes:
1. Update CNAME record to point to new hostname
2. Re-validate domain in Azure Portal
3. Update Bicep parameters if needed

## Support

For custom domain issues:
1. Check this guide's Troubleshooting section
2. Verify DNS configuration with `nslookup` or `dig`
3. Check Azure Portal → Web App → Custom domains for status
4. Review Application Insights for errors
5. Open an issue in the GitHub repository

## References

- [Azure App Service Custom Domain Documentation](https://learn.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-custom-domain)
- [Azure Managed Certificates](https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate)
- [Microsoft Entra Redirect URI Configuration](https://learn.microsoft.com/en-us/azure/active-directory/develop/reply-url)
- [DNS CNAME Records](https://www.cloudflare.com/learning/dns/dns-records/dns-cname-record/)
