using Core.CrossCuttingConcerns.Exceptions;
using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit;

namespace Core.Tests.CrossCutting.Exceptions.Middleware;

public class ExceptionMiddlewareTests
{
    private ServiceProvider CreateServiceProvider()
    {
        ErrorDetailUrlConfigurations config = new ErrorDetailUrlConfigurations
        {
            BusinessUrl = "https://errors/biz",
            ValidationUrl = "https://errors/val",
            NotFoundUrl = "https://errors/notfound",
            InternalUrl = "https://errors/internal",
            BadRequestUrl = "https://errors/bad"
        };

        ServiceCollection services = new ServiceCollection();

        services.AddSingleton(Options.Create(config));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<HttpExceptionHandler>();

        return services.BuildServiceProvider();
    }

    [Trait("Middleware", "ExceptionMiddleware")]
    [Fact]
    public async Task Should_Catch_Exception_And_Return_InternalProblemDetails()
    {
        //Arrange

        RequestDelegate next = (ctx) => throw new Exception("Test error");

        HttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        ServiceProvider provider = CreateServiceProvider();
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = context;

        var middleware = new ExceptionMiddleware(next, provider, accessor);

        //Act
        await middleware.Invoke(context);

        //Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(context.Response.Body).ReadToEndAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        root.GetProperty("title").GetString().Should().Be("Internal Rule Violation");
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status500InternalServerError);
        root.GetProperty("type").GetString().Should().Be("https://errors/internal");

    }

}
