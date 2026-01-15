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

@description('Custom domain hostname (optional, e.g., storingsdienst.jll.io)')
param customDomain string = ''

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
// Custom Domain Binding (Optional)
// ========================================
// Note: Before this works, you must:
// 1. Create a CNAME record in DNS pointing to the Web App's default hostname
// 2. Verify the domain ownership in Azure Portal
// 3. The managed certificate will be automatically provisioned

resource customDomainBinding 'Microsoft.Web/sites/hostNameBindings@2023-01-01' = if (!empty(customDomain)) {
  parent: webApp
  name: customDomain
  properties: {
    siteName: webApp.name
    hostNameType: 'Verified'
    sslState: 'SniEnabled'
    thumbprint: null
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

@description('Custom domain hostname (if configured)')
output customDomain string = customDomain

@description('Custom domain URL (if configured)')
output customDomainUrl string = !empty(customDomain) ? 'https://${customDomain}' : ''
