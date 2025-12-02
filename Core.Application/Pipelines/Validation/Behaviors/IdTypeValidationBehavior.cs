using Core.Abstractions.Applications.Validators;
using Core.Application.Pipelines.Validation.Attributes;
using Core.CrossCuttingConcerns.Exceptions.Types;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application.Pipelines.Validation.Behaviors;

public class IdTypeValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public IdTypeValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IdTypeValidatorAttribute? attribute = request.GetType().GetCustomAttribute<IdTypeValidatorAttribute>();
        if (attribute is null)
            return await next();

        IIdTypeValidator validator = (IIdTypeValidator)_serviceProvider.GetRequiredService(attribute.ValidatorType);

        PropertyInfo? idTypeProperty = request.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idTypeProperty is null)
            throw new IdTypeValidationException(attribute.ErrorMessage);

        string idTypeValue = idTypeProperty.GetValue(request)?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(idTypeValue))
            throw new IdTypeValidationException(attribute.ErrorMessage);
        if (!validator.IsValid(idTypeValue))
            throw new IdTypeValidationException(attribute.ErrorMessage);

        return await next();

    }
}
