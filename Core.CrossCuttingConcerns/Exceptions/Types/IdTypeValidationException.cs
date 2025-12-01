namespace Core.CrossCuttingConcerns.Exceptions.Types;

public sealed class IdTypeValidationException : Exception
{
    public IdTypeValidationException(string message) : base(message) { }
    public IdTypeValidationException(string message, Exception innerException) : base(message, innerException) { }
}
