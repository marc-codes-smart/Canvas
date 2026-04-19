using Canvas.Infrastructure.Shared.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Canvas.Host.Modules;

internal sealed class CanvasStubModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration) { }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
