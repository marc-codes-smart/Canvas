using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Canvas.Infrastructure.Shared.Modules;

public static class ModuleExtensions
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .Select(t => (IModule)Activator.CreateInstance(t)!);

        foreach (var module in modules)
        {
            services.AddSingleton<IModule>(module);
            module.RegisterServices(services, configuration);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapModules(
        this IEndpointRouteBuilder endpoints,
        IServiceProvider serviceProvider)
    {
        foreach (var module in serviceProvider.GetServices<IModule>())
            module.MapEndpoints(endpoints);
        return endpoints;
    }
}
