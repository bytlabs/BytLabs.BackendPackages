Pass if the response:
- defines the command as a record implementing ICommand or ICommand<TResult>
- puts validation in a separate AbstractValidator<UpdateWarehouseCommand> class
- relies on automatic validator discovery via AddCQS rather than calling the validator
  manually inside the handler
- keeps the handler free of argument checking

Fail if validation is hand-rolled with if/throw inside the handler, or if the validator is
invoked explicitly by the handler.
