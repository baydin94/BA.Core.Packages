namespace Core.Application.Pipelines.Validation.Validators;

public interface IIdTypeValidator
{
    bool IsValid(string value);
}
