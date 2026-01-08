// ========================================
// App Service Plan Module
// ========================================

@description('Name of the App Service Plan')
param name string

@description('Location for the resource')
param location string

@description('SKU for the App Service Plan')
param sku string

@description('Tags to apply to the resource')
param tags object

// ========================================
// App Service Plan
// ========================================
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'windows'
  properties: {
    reserved: false // Windows App Service Plan
  }
}

// ========================================
// Outputs
// ========================================
@description('App Service Plan resource ID')
output id string = appServicePlan.id

@description('App Service Plan name')
output name string = appServicePlan.name
