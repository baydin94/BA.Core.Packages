using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ConflictProblemDetails : ProblemDetails
{
    public ConflictProblemDetails(string detail, ErrorDetailUrlConfigurations configurations)
    {
        Title = "Conflict rule violation";
        Detail = detail;
        Status = StatusCodes.Status409Conflict;
        Type = configurations.ConflictUrl;
    }
}
