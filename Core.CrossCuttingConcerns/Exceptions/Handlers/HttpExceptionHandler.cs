using Core.CrossCuttingConcerns.Exceptions.Extensions;
using Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.CrossCuttingConcerns.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    private readonly ErrorDetailUrlConfigurations _configs;

    public HttpExceptionHandler(IOptions<ErrorDetailUrlConfigurations> options)
    {
        _configs = options.Value;
    }

    private HttpResponse? _response;
    public HttpResponse Response
    {
        get => _response ?? throw new ArgumentNullException(nameof(_response));
        set => _response = value;
    }
    protected override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        string details = new BusinessProblemDetails(businessException.Message, _configs).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        string details = new InternalProblemDetails(exception.Message, _configs).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        string details = new ValidationProblemDetails(validationException.Errors, _configs).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(NotFoundException notFoundException)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        string details = new NotFoundProblemDetails(notFoundException.Message, _configs).AsJson();

        return Response.WriteAsync(details);
    }

    protected override Task HandleException(IdTypeValidationException idTypeValidationException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        string details = new IdTypeValidationProblemDetails(idTypeValidationException.Message, _configs).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(ConflictException conflictException)
    {
        Response.StatusCode = StatusCodes.Status409Conflict;
        string detail = new ConflictProblemDetails(conflictException.Message, _configs).AsJson();
        return Response.WriteAsync(detail);
    }
}
