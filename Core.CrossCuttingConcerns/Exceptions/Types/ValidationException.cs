
namespace Core.CrossCuttingConcerns.Exceptions.Types;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException() : base() => Errors = new Dictionary<string, string[]>();
    public ValidationException(string? message, Exception? innerException) : base(message, innerException) => Errors = new Dictionary<string, string[]>();
    public ValidationException(IDictionary<string, string[]> errors) : base(BuildErrorMessage(errors)) => Errors = errors;

    private static string? BuildErrorMessage(IDictionary<string, string[]> errors)
    {
        var messages = errors.Select(e =>
            $"{e.Key}: {string.Join(", ", e.Value)}");

        return "Validation failed: " + string.Join(" | ", messages);
    }
}


