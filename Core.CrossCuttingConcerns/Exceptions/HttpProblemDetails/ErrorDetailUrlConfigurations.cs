namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ErrorDetailUrlConfigurations
{
    public string BusinessUrl { get; set; } = string.Empty;
    public string ValidationUrl { get; set; } = string.Empty;
    public string InternalUrl { get; set; } = string.Empty;
    public string? NotFoundUrl { get; set; }
    public string? BadRequestUrl { get; set; }
    public string? ConflictUrl { get; set; }
}
