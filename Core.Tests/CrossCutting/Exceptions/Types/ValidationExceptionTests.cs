using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Types;

public class ValidationExceptionTests
{
    [Trait("Exceptions", "Validation")]
    [Fact]
    public async Task ValidationException_Default_Should_Set_Empty_Error_List()
    {
        //Act
        ValidationException exception = new ValidationException();

        //Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
        exception.Message.Should().NotBeNull();
    }

    [Trait("Exceptions", "Validation")]
    [Fact]
    public async Task ValidationException_With_Message_Should_Set_Empty_Error_List()
    {
        //Act
        ValidationException exception = new ValidationException("Custom message", new Exception("inner"));

        //Act
        exception.Message.Should().Be("Custom message");
        exception.InnerException!.Message.Should().Be("inner");

        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Trait("Exceptions", "Validation")]
    [Fact]
    public async Task ValidationException_With_Errors_Should_Set_Errors_And_Build_Message()
    {
        //Arr
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Not be null", "Must be greater than 2 characters" } },
            { "Age", new[] { "Must be greater than 18" } }
        };

        //Act
        ValidationException exception = new ValidationException(errors);

        // Assert dictionary count
        exception.Errors.Should().HaveCount(2);

        // Assert item values
        exception.Errors.Should().ContainKey("Name");
        exception.Errors["Name"].Should().Contain("Required");

        exception.Errors.Should().ContainKey("Age");
        exception.Errors["Age"].Should().Contain("Must be greater than 18");

        // Assert message text
        exception.Message.Should().Contain("Validation failed:");
        exception.Message.Should().Contain("Name");
        exception.Message.Should().Contain("Required");
    }

    [Trait("Exceptions", "Validation")]
    [Fact]
    public void ValidationException_With_Empty_Errors_Should_Produce_Message_With_No_Entries()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>();

        // Act
        var ex = new ValidationException(errors);

        // Assert
        ex.Errors.Should().BeEmpty();
        ex.Message.Should().Contain("Validation failed:");
    }
}
