using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Cashify.Api.Features;

namespace Cashify.Api.Extensions;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder WithDocs(this RouteHandlerBuilder builder, string summary, string description)
        => builder.WithSummary(summary)
            .WithDescription(description);

    public static IServiceCollection AddEndpointsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder as IEndpointRouteBuilder ?? app;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    public static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group, IServiceProvider services)
    {
        var endpoints = services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(group);
        }
        return group;
    }
}
