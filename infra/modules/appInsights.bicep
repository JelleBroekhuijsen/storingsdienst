// ========================================
// Application Insights Module
// ========================================

@description('Name of the Application Insights resource')
param name string

@description('Location for the resource')
param location string

@description('Tags to apply to the resource')
param tags object

// ========================================
// Application Insights Workspace
// ========================================
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${name}-workspace'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// ========================================
// Application Insights
// ========================================
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ========================================
// Outputs
// ========================================
@description('Application Insights resource ID')
output id string = appInsights.id

@description('Application Insights name')
output name string = appInsights.name

@description('Application Insights Instrumentation Key')
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('Application Insights Connection String')
output connectionString string = appInsights.properties.ConnectionString
