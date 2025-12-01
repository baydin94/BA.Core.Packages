using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Types;

public class BusinessExceptionTests
{
    [Trait("Exceptions","BusinessException")]
    [Fact]
    public async Task BusinessException_Should_Set_Message()
    {
        //Arrange & Act
        BusinessException exception = new BusinessException("Invalid operation");

        //Assert
        exception.Message.Should().Be("Invalid operation");
    }

    [Trait("Exceptions", "BusinessException")]
    [Fact]
    public async Task BusinessException_Default_Should_Have_Default_Message()
    {
        //Arrange & Act
        BusinessException exception = new BusinessException();

        //Assert
        exception.Message.Should().NotBeNull();
        exception.Message.Should().Contain("BusinessException");
    }

    [Trait("Exceptions", "BusinessException")]
    [Fact]
    public async Task BusinessException_Should_Set_InnerException()
    {
        //Arrange 
        Exception innerException = new Exception("inner-error");

        //Act
        BusinessException exception = new BusinessException("outer-error", innerException);

        //Assert
        exception.Message.Should().NotBeNull();
        exception.InnerException.Should().Be(innerException);
    }
}
