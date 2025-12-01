using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ExceptionMiddleware(RequestDelegate next, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _provider = provider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        using var scope = _provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<HttpExceptionHandler>();

        handler.Response = context.Response;
        await handler.HandleExceptionAsync(exception);
    }
}
