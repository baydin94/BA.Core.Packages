using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class IdTypeValidationProblemDetails : ProblemDetails
{
    public IdTypeValidationProblemDetails(string detail, ErrorDetailUrlConfigurations configurations)
    {
        Title = "Invalid ID format";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
        Type = configurations.BadRequestUrl;
    }
}
