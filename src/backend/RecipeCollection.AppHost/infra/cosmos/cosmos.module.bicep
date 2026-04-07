@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2024-08-15' = {
  name: take('cosmos-${uniqueString(resourceGroup().id)}', 44)
  location: location
  properties: {
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
  }
  kind: 'GlobalDocumentDB'
  tags: {
    'aspire-resource-name': 'cosmos'
  }
}

resource cosmosdb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-08-15' = {
  name: 'cosmosdb'
  location: location
  properties: {
    resource: {
      id: 'cosmosdb'
    }
  }
  parent: cosmos
}

resource recipeContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-08-15' = {
  name: 'Recipe'
  location: location
  properties: {
    resource: {
      id: 'Recipe'
      partitionKey: {
        paths: ['/Pk']
        kind: 'Hash'
      }
    }
  }
  parent: cosmosdb
}

resource recipeIngredientContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-08-15' = {
  name: 'RecipeIngredient'
  location: location
  properties: {
    resource: {
      id: 'RecipeIngredient'
      partitionKey: {
        paths: ['/Pk']
        kind: 'Hash'
      }
    }
  }
  parent: cosmosdb
}

output connectionString string = cosmos.properties.documentEndpoint

output name string = cosmos.name

output id string = cosmos.id
