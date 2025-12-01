namespace Core.Application.Pipelines.Validation.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class IdTypeValidatorAttribute : Attribute
{
    public Type ValidatorType { get; }
    public string ErrorMessage { get; }

    public IdTypeValidatorAttribute(Type validatorType, string errorMessage)
    {
        ValidatorType = validatorType;
        ErrorMessage = errorMessage;
    }
}
