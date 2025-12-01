using Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.HttpProblemDetails;

public class ProblemDetailsTests
{
    [Trait("ProblemDetails", "NotFound")]
    [Fact]
    public void NotFoundProblemDetails_Should_Set_Properties_Correctly()
    {
        //Arrange
        string detail = "Entity not found";
        ErrorDetailUrlConfigurations config = new ErrorDetailUrlConfigurations { NotFoundUrl = "https://errors.mydomain.com/notfound" };

        //Act
        NotFoundProblemDetails problem = new NotFoundProblemDetails(detail, config);

        problem.Title.Should().Be("Resource not found");
        problem.Detail.Should().Be(detail);
        problem.Type.Should().Be(config.NotFoundUrl);

        problem.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Trait("ProblemDetails", "ValidationProblemDetail")]
    [Fact]
    public void ValidationProblemDetail_Should_Set_Properties_Correctly()
    {
        //Arr
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Not be null" } },
            { "Age", new[] { "Must be greater than 18" } }
        };

        ErrorDetailUrlConfigurations config = new ErrorDetailUrlConfigurations { ValidationUrl = "https://errors.mydomain.com/validationerror" };

        //Act
        ValidationProblemDetails problem = new ValidationProblemDetails(errors, config);

        //Assert
        problem.Errors.Should().HaveCount(2);
        problem.Errors.Should().NotBeNull();
        problem.Title.Should().Be("Validation error(s)");
        problem.Detail.Should().Be("One or more validation errors occured");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problem.Type.Should().Be(config.ValidationUrl);


    }
}
