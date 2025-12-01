using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class InternalProblemDetails : ProblemDetails
{
    public InternalProblemDetails(string detail, ErrorDetailUrlConfigurations configurations)
    {
        Title = "Internal Rule Violation";
        Status = StatusCodes.Status500InternalServerError;
        Detail = detail;
        Type = configurations.InternalUrl;
    }
}
