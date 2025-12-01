using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class NotFoundProblemDetails : ProblemDetails
{
    public NotFoundProblemDetails(string detail, ErrorDetailUrlConfigurations configurations)
    {
        Title = "Resource not found";
        Detail = detail;
        Status = StatusCodes.Status404NotFound;
        Type = configurations.NotFoundUrl;
    }
}
