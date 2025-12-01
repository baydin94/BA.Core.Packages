using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns;

public static class CrossCuttingConcernsServiceRegistration
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddScoped<HttpExceptionHandler>();
        return services;
    }
}
