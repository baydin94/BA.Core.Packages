using Core.CrossCuttingConcerns.Exceptions.Types;

namespace Core.CrossCuttingConcerns.Exceptions.Handlers;

public abstract class ExceptionHandler
{
    public Task HandleExceptionAsync(Exception exception) => exception switch
    {
        BusinessException businessException => HandleException(businessException),
        ConflictException conflictException => HandleException(conflictException),
        ValidationException validationException => HandleException(validationException),
        IdTypeValidationException idTypeValidationException => HandleException(idTypeValidationException),
        NotFoundException notFoundException => HandleException(notFoundException),
        _ => HandleException(exception)
    };

    protected abstract Task HandleException(BusinessException businessException);
    protected abstract Task HandleException(ConflictException conflictException);
    protected abstract Task HandleException(ValidationException validationException);
    protected abstract Task HandleException(IdTypeValidationException idTypeValidationException);
    protected abstract Task HandleException(NotFoundException notFoundException);
    protected abstract Task HandleException(Exception exception);
}
