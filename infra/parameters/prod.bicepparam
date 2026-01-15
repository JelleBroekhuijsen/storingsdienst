// ========================================
// Production Environment Parameters
// ========================================

using '../main.bicep'

param appName = 'storingsdienst'
param environment = 'prod'
param location = 'westeurope'
param sku = 'B1'
param customDomain = 'storingsdienst.jll.io'

param tags = {
  Application: 'Storingsdienst'
  Environment: 'Production'
  ManagedBy: 'Bicep'
  CostCenter: 'IT'
}
