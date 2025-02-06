using BytLabs.Application.CQS.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytLabs.Application.CQS.Queries
{
    public static class QueryValidatorExtensions
    {
        public static async Task ValidateQueryAndThrowAsync<TRequest, TResponse>(this IValidator<TRequest> validator, TRequest instance, CancellationToken cancellationToken)
            where TRequest : IQuery<TResponse>
            where TResponse : notnull
        {
            var res = await validator.ValidateAsync(instance, cancellationToken);

            if (!res.IsValid)
            {
                var ex = new ValidationException(res.Errors);
                throw new QueryValidationException(ex.Message, res.Errors);
            }
        }
    }
}
