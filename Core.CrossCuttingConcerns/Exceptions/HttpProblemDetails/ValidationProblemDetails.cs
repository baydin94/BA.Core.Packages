using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ValidationProblemDetails : ProblemDetails
{
    public IDictionary<string, string[]>? Errors { get; set; }
    public ValidationProblemDetails(IDictionary<string, string[]>? errors, ErrorDetailUrlConfigurations configurations)
    {
        Title = "Validation error(s)";
        Detail = "One or more validation errors occured";
        Errors = errors;
        Status = StatusCodes.Status422UnprocessableEntity;
        Type = configurations.ValidationUrl;
    }
}
