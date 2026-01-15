// ========================================
// Storingsdienst - Azure Infrastructure
// ========================================
// This Bicep template deploys the complete infrastructure for the Storingsdienst application
// including App Service Plan, Web App, and Application Insights.

// Parameters
@description('The name of the application (used for resource naming)')
param appName string = 'storingsdienst'

@description('Environment name (prod, dev, etc.)')
param environment string = 'prod'

@description('Azure region for all resources')
param location string = 'westeurope'

@description('App Service Plan SKU')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1V2'
  'P2V2'
  'P3V2'
])
param sku string = 'B1'

@description('Tags to apply to all resources')
param tags object = {
  Application: 'Storingsdienst'
  Environment: environment
  ManagedBy: 'Bicep'
}

@description('Custom domain hostname (optional, e.g., storingsdienst.jll.io)')
param customDomain string = ''

// Variables
var resourceGroupName = 'rg-${appName}-${environment}'
var appServicePlanName = 'asp-${appName}-${environment}'
var webAppName = 'app-${appName}-${environment}'
var appInsightsName = 'appi-${appName}-${environment}'

// ========================================
// Resource Group (target scope)
// ========================================
targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// ========================================
// Application Insights Module
// ========================================
module appInsights 'modules/appInsights.bicep' = {
  name: 'appInsights-deployment'
  scope: resourceGroup
  params: {
    name: appInsightsName
    location: location
    tags: tags
  }
}

// ========================================
// App Service Plan Module
// ========================================
module appServicePlan 'modules/appServicePlan.bicep' = {
  name: 'appServicePlan-deployment'
  scope: resourceGroup
  params: {
    name: appServicePlanName
    location: location
    sku: sku
    tags: tags
  }
}

// ========================================
// Web App Module
// ========================================
module webApp 'modules/webApp.bicep' = {
  name: 'webApp-deployment'
  scope: resourceGroup
  params: {
    name: webAppName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    appInsightsConnectionString: appInsights.outputs.connectionString
    tags: tags
    customDomain: customDomain
  }
}

// ========================================
// Outputs
// ========================================
@description('The name of the resource group')
output resourceGroupName string = resourceGroup.name

@description('The name of the App Service Plan')
output appServicePlanName string = appServicePlan.outputs.name

@description('The name of the Web App')
output webAppName string = webApp.outputs.name

@description('The default hostname of the Web App')
output webAppHostname string = webApp.outputs.defaultHostname

@description('The URL of the Web App')
output webAppUrl string = 'https://${webApp.outputs.defaultHostname}'

@description('The name of Application Insights')
output appInsightsName string = appInsights.outputs.name

@description('Application Insights Instrumentation Key')
output appInsightsInstrumentationKey string = appInsights.outputs.instrumentationKey

@description('Application Insights Connection String')
output appInsightsConnectionString string = appInsights.outputs.connectionString

@description('Custom domain hostname (if configured)')
output customDomain string = webApp.outputs.customDomain

@description('Custom domain URL (if configured)')
output customDomainUrl string = webApp.outputs.customDomainUrl
