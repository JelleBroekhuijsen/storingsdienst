# Infrastructure as Code - Bicep Templates

This directory contains the Azure infrastructure templates for the Storingsdienst application using Bicep.

## Structure

```
infra/
├── main.bicep              # Main template orchestrating all resources
├── parameters/
│   └── prod.bicepparam     # Production environment parameters
└── modules/
    ├── appInsights.bicep   # Application Insights configuration
    ├── appServicePlan.bicep # App Service Plan configuration
    └── webApp.bicep         # Web App configuration
```

## Resources Deployed

The infrastructure template deploys the following Azure resources:

### 1. Resource Group
- **Name**: `rg-storingsdienst-prod`
- **Location**: West Europe (Amsterdam datacenter)
- **Purpose**: Container for all application resources

### 2. App Service Plan
- **Name**: `asp-storingsdienst-prod`
- **SKU**: B1 (Basic, €13/month)
- **Platform**: Windows
- **Features**: Always On, autoscaling capable

### 3. Web App
- **Name**: `app-storingsdienst-prod`
- **Runtime**: .NET 8.0
- **URL**: https://app-storingsdienst-prod.azurewebsites.net
- **Features**:
  - HTTPS only
  - Application Insights integration
  - FTP disabled
  - HTTP/2 enabled
  - TLS 1.2 minimum

### 4. Application Insights
- **Name**: `appi-storingsdienst-prod`
- **Workspace**: Log Analytics workspace with 30-day retention
- **Purpose**: Application monitoring, telemetry, and error tracking

## Deployment

### Prerequisites
- Azure subscription with Contributor access
- Azure CLI installed (for local deployment)
- Bicep CLI installed (optional, for local validation)

### Using GitHub Actions (Recommended)
1. Navigate to the GitHub repository
2. Go to **Actions** → **Deploy Infrastructure**
3. Click **Run workflow**
4. Type `deploy` to confirm
5. Click **Run workflow**

### Using Azure CLI (Manual)
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "<your-subscription-id>"

# Deploy infrastructure
az deployment sub create \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam
```

## Parameters

### Available Parameters
- `appName`: Application name (default: `storingsdienst`)
- `environment`: Environment name (default: `prod`)
- `location`: Azure region (default: `westeurope`)
- `sku`: App Service Plan SKU (default: `B1`)
- `tags`: Resource tags

### Modifying Parameters
Edit `infra/parameters/prod.bicepparam` to customize the deployment.

## Outputs

After successful deployment, the following values are output:

- `resourceGroupName`: Name of the resource group
- `appServicePlanName`: Name of the App Service Plan
- `webAppName`: Name of the Web App
- `webAppHostname`: Default hostname of the Web App
- `webAppUrl`: Full HTTPS URL of the Web App
- `appInsightsName`: Name of Application Insights
- `appInsightsInstrumentationKey`: Instrumentation key
- `appInsightsConnectionString`: Connection string

## Scaling

### Vertical Scaling (SKU Changes)
To scale the App Service Plan to a different SKU:

1. Edit `infra/parameters/prod.bicepparam`
2. Change the `sku` parameter to:
   - `B1`, `B2`, `B3` - Basic tier
   - `S1`, `S2`, `S3` - Standard tier (supports auto-scaling)
   - `P1V2`, `P2V2`, `P3V2` - Premium tier (recommended for production)
3. Redeploy the infrastructure

### Horizontal Scaling (Multiple Instances)
For Standard or Premium tiers, you can enable auto-scaling:

1. Go to Azure Portal
2. Navigate to the App Service Plan
3. Go to **Scale out (App Service plan)**
4. Configure auto-scaling rules based on CPU, memory, or custom metrics

## Cost Estimation

| Resource | SKU | Monthly Cost (EUR) |
|----------|-----|-------------------|
| App Service Plan | B1 | €13 |
| Application Insights | Pay-as-you-go | €2-5 (5GB free) |
| **Total** | | **€15-18** |

## Troubleshooting

### Deployment Failures

**Error: Resource group already exists**
- The template uses `targetScope = 'subscription'` and creates the resource group
- Ensure you're deploying at subscription scope, not resource group scope

**Error: Location not valid**
- Verify the `location` parameter is a valid Azure region code
- Use `az account list-locations -o table` to see available regions

**Error: SKU not available**
- Some SKUs may not be available in all regions
- Try a different SKU or region

### Validation
To validate the Bicep template without deploying:

```bash
az deployment sub validate \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam
```

Or using Bicep CLI:
```bash
bicep build infra/main.bicep
```

## Security Considerations

- **HTTPS Only**: Enforced on the Web App
- **TLS 1.2+**: Minimum TLS version required
- **FTP Disabled**: No FTP/FTPS access allowed
- **Managed Identity**: Can be enabled for accessing Azure services
- **Application Insights**: Ingestion over HTTPS only

## Maintenance

### Updating Infrastructure
1. Make changes to Bicep files
2. Commit and push to the repository
3. Run the **Deploy Infrastructure** workflow
4. Azure Resource Manager handles incremental updates

### Deleting Infrastructure
To completely remove all resources:

```bash
az group delete --name rg-storingsdienst-prod --yes --no-wait
```

**Warning**: This action is irreversible and will delete all data.

## Support

For issues related to infrastructure:
1. Check the GitHub Actions workflow logs
2. Review Azure Portal activity logs
3. Consult the [main deployment guide](../docs/DEPLOYMENT.md)
4. Open an issue in the GitHub repository
