using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;
using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Handlers;

public class HttpExceptionHandlerTests
{
    private HttpContext CreateHttpContext()
    {
        HttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); //capture output
        return context;
    }

    private HttpExceptionHandler CreateHandler(HttpContext httpContext)
    {
        ErrorDetailUrlConfigurations config = new ErrorDetailUrlConfigurations
        {
            BusinessUrl = "https://errors/biz",
            ValidationUrl = "https://errors/val",
            NotFoundUrl = "https://errors/notfound",
            InternalUrl = "https://errors/internal",
            BadRequestUrl = "https://errors/internal"
        };

        HttpExceptionHandler handler = new HttpExceptionHandler(options: Options.Create(config))
        {
            Response = httpContext.Response
        };

        return handler;
    }

    [Trait("ExceptionHandler", "Business")]
    [Fact]
    public async Task Should_Handle_BusinessException_Correctly()
    {
        //Arr
        HttpContext http = CreateHttpContext();
        HttpExceptionHandler handler = CreateHandler(http);
        BusinessException exception = new BusinessException("Name already exists.");

        //Act
        await handler.HandleExceptionAsync(exception);

        //Assert Status Code
        http.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        //Assert Json Body
        http.Response.Body.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(http.Response.Body).ReadToEndAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        root.GetProperty("title").GetString().Should().Be("Business Rule Violation");
        root.GetProperty("detail").GetString().Should().Be("Name already exists.");
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("type").GetString().Should().Be("https://errors/biz");
    }
    [Trait("ExceptionHandler", "IdTypeValidation")]
    [Fact]
    public async Task Should_Handle_IdTypeValidationException_Correctly()
    {
        //Arr
        HttpContext httpContext = CreateHttpContext();
        HttpExceptionHandler handler = CreateHandler(httpContext);
        IdTypeValidationException exception = new IdTypeValidationException("Type error");

        //Act
        await handler.HandleExceptionAsync(exception);

        //Assert Status Code
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        //Assert Json Body
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        root.GetProperty("title").GetString().Should().Be("Invalid ID format");
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status400BadRequest);
        root.GetProperty("detail").GetString().Should().Be("Type error");
        root.GetProperty("type").GetString().Should().Be("https://errors/internal");
    }

    [Trait("ExceptionHandler", "Exception")]
    [Fact]
    public async Task Should_Handle_InternalException_Correctly()
    {
        //Arr
        HttpContext context = CreateHttpContext();
        HttpExceptionHandler handler =  CreateHandler(context);
        Exception exception = new Exception("exception error");

        //Act
        await handler.HandleExceptionAsync(exception);

        //Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(context.Response.Body).ReadToEndAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        root.GetProperty("title").GetString().Should().Be("Internal Rule Violation");
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status500InternalServerError);
        root.GetProperty("detail").GetString().Should().Be("exception error");
        root.GetProperty("type").GetString().Should().Be("https://errors/internal");
    }

    [Trait("ExceptionHandler", "Exception")]
    [Fact]
    public async Task Should_Handle_ValidationException_Correctly()
    {
        //Arr
        HttpContext context = CreateHttpContext();
        HttpExceptionHandler handler = CreateHandler(context);

        Dictionary<string, string[]> errors = new Dictionary<string, string[]>
        {
            {"Name", new[] {"Not be null", "Must be greater than 2 characters"} },
            { "Age",  new[] { "Must be greater than 18" } }
        };

        ValidationException exception = new ValidationException(errors);

        //Act
        await handler.HandleExceptionAsync(exception);

        //Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(context.Response.Body).ReadToEndAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;


        root.GetProperty("title").GetString().Should().Be("Validation error(s)");
        root.GetProperty("detail").GetString().Should().Be("One or more validation errors occured");
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        root.GetProperty("type").GetString().Should().Be("https://errors/val");

        // Name field
        JsonElement errorsObj = root.GetProperty("Errors");
        errorsObj.GetProperty("Name")[0].GetString().Should().Be("Not be null");
        errorsObj.GetProperty("Name")[1].GetString().Should().Be("Must be greater than 2 characters");

        //// Age field
        errorsObj.GetProperty("Age")[0].GetString().Should().Be("Must be greater than 18");
    }   
}
