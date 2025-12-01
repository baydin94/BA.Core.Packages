using FluentValidation;
using MediatR;
using ValidationException = Core.CrossCuttingConcerns.Exceptions.Types.ValidationException;

namespace Core.Application.Pipelines.Validation.Behaviors;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        var errors = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                key => key.Key,
                value => value.Select(v=> v.ErrorMessage).ToArray()
            );

        if (errors.Any())
            throw new ValidationException(errors);

        TResponse response = await next();
        return response;
    }
}
