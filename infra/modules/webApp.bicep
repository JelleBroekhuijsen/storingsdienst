// ========================================
// Web App Module
// ========================================

@description('Name of the Web App')
param name string

@description('Location for the resource')
param location string

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Application Insights Instrumentation Key')
@secure()
param appInsightsInstrumentationKey string

@description('Application Insights Connection String')
@secure()
param appInsightsConnectionString string

@description('Tags to apply to the resource')
param tags object

// ========================================
// Web App
// ========================================
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app'
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      alwaysOn: true
      ftpsState: 'Disabled'
      http20Enabled: true
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

// ========================================
// Outputs
// ========================================
@description('Web App resource ID')
output id string = webApp.id

@description('Web App name')
output name string = webApp.name

@description('Web App default hostname')
output defaultHostname string = webApp.properties.defaultHostName
