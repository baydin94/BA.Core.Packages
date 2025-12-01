using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Types;

public class NotFoundExceptionTests
{
    [Trait("Exceptions","NotFound")]
    [Fact]
    public async Task NotFoundException_Should_Set_Message()
    {
        //Arrange & Act
        NotFoundException exception = new NotFoundException("Item not found");

        //Act
        exception.Message.Should().Be("Item not found");
    }

    [Trait("Exceptions", "NotFound")]
    [Fact]
    public async Task NotFoundException_Should_Have_Default_Message()
    {
        //Arrange & Act
        NotFoundException exception = new NotFoundException();

        //Assert
        exception.Message.Should().NotBeNull();
    }

    [Trait("Exception","NotFound")]
    [Fact]
    public async Task NotFoundException_Should_Set_InnerException()
    {
        //Arrange
        Exception innerException = new Exception("inner-error");

        //Act
        NotFoundException exception = new NotFoundException("outer-error", innerException);

        //Assert
        exception.InnerException.Should().NotBeNull();
        exception.InnerException.Should().Be(innerException);
    }
}
