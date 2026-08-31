Pass if the response:
- calls AddMongoDatabase(...) with MongoDatabaseConfiguration, then
  AddMongoRepository<Warehouse, Guid>()
- explains that the per-aggregate registration is required and that omitting it is a
  runtime DI failure
- does not pass a tenant id into any repository call or configuration
- if it mentions soft delete on a Mongo aggregate fluent, spells it
  ExcludeSoftDeletedEntites (no second "i")

Fail if it threads a tenant id through the repository, registers a repository for a
non-aggregate entity, or calls a dynamic-data filter method such as FilterData or
ApplyDynamicDataFilteration, which do not exist.
