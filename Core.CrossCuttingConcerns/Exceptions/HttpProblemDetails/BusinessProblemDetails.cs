using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class BusinessProblemDetails : ProblemDetails
{
    public BusinessProblemDetails(string detail, ErrorDetailUrlConfigurations config)
    {
        Title = "Business Rule Violation";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
        Type = config.BusinessUrl;
    }
}
