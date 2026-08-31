Pass if the response:
- registers types with the real package methods AddCommandType<T>() and AddDtoType<T>()
- relies on AddGraphQLService() already enabling mutation conventions rather than
  re-adding them
- explains the naming conventions: CreateWarehouseCommand becomes input
  CreateWarehouseInput, WarehouseDto becomes object type Warehouse
- routes business and validation failures to the typed BusinessError / ValidationError

Fail if it calls AddCommandTypes(), AddDtoTypes(), AddAggregateTypes() or
AddMongoDbQuerySettings() as though they were package methods - they are consumer-side
helpers that do not exist in BytLabs.Api.Graphql.
