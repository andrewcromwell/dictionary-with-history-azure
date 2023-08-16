param location string = resourceGroup().location
param tags object = {}

param databaseName string = ''
param keyVaultName string
param accountName string = ''

module cosmosDB '../core/database/cosmos/sql/cosmos-sql-db.bicep' = {
  name: 'cosmosDB'
  params: {
    accountName: accountName
    databaseName: databaseName
    location: location
    tags: tags

    containers: [
      {
        name: 'WordLookups'
        id: 'WordLookups'
        partitionKey: '/userId'
      }
    ]
    keyVaultName: keyVaultName
    principalIds: []
  }
}

output connectionStringKey string = cosmosDB.outputs.connectionStringKey
output databaseName string = cosmosDB.outputs.databaseName
