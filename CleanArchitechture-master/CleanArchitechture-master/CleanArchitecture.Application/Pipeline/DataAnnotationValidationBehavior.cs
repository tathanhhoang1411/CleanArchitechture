using MediatR;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Utilities;

namespace CleanArchitecture.Application.Pipeline
{
    public class DataAnnotationValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request == null)
                return await next();

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request, serviceProvider: null, items: null);

            // Validate the request object itself
            Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

            // Validate simple nested properties (one level)
            var props = request.GetType().GetProperties().Where(p => p != null);
            foreach (var prop in props)
            {
                var value = prop.GetValue(request);
                if (value == null) continue;
                var nestedContext = new ValidationContext(value, serviceProvider: null, items: null);
                Validator.TryValidateObject(value, nestedContext, validationResults, validateAllProperties: true);
            }

            if (validationResults.Any())
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? r.MemberNames.FirstOrDefault() ?? "Invalid").Distinct().ToList();
                throw new CleanArchitecture.Application.Utilities.ValidationException(errors);
            }

            return await next();
        }
    }
}
