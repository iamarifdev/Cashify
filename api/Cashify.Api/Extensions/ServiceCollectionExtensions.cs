using System.Reflection;

namespace Cashify.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly
            .DefinedTypes
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Handler", StringComparison.Ordinal));

        foreach (var type in handlerTypes)
        {
            services.AddTransient(type);
        }

        return services;
    }
}
