using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Types;

public class IdTypeValidationExceptionTests
{
    [Trait("Exceptions", "IdTypeValidation")]
    [Fact]
    public async Task IdTypeValidation_Should_Set_Message()
    {
        //Act
        IdTypeValidationException exception = new IdTypeValidationException("ID must be a valid GUID");

        //Ass
        exception.Message.Should().Be("ID must be a valid GUID");
        exception.Should().NotBeNull();
    }

    [Trait("Exceptions", "IdTypeValidation")]
    [Fact]
    public async Task IdTypeValidation_Default_Should_Have_Default_Message()
    {
        //Arr
        Exception innerException = new Exception("inner-message:");

        //Act
        IdTypeValidationException exception = new IdTypeValidationException("ID must be a valid GUID", innerException);

        //Ass
        exception.Message.Should().Be("ID must be a valid GUID");
        exception.Should().NotBeNull();
        exception.InnerException.Should().Be(innerException);
    }
}
