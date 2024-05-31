@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name of the App Service.')
param appName string = 'tasktacklerapp'

@description('SKU for the App Service Plan.')
param skuName string = 'F1' // Free tier

@secure()
@description('API URL for the application.')
param apiUrl string

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appName
  location: location
  sku: {
    name: skuName
    tier: 'Free'
  }
  kind: 'Windows'
  properties: {
    reserved: false
  }
}

resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ApiUrl'
          value: apiUrl
        }
      ]
    }
  }
}

output appServiceAppName string = webApp.name
