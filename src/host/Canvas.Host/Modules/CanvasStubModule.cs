using Canvas.Infrastructure.Shared.Modules;
using Canvas.Infrastructure.Shared.Outbox;
using Canvas.Infrastructure.Shared.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Canvas.Host.Modules;

internal sealed class CanvasStubModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration) { }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/stub/ping", async (CanvasDbContext db) =>
        {
            var message = new OutboxMessage
            {
                Type = "StubPing",
                Content = """{"source":"CanvasStubModule"}""",
                OccurredOn = DateTimeOffset.UtcNow
            };
            db.OutboxMessages.Add(message);
            await db.SaveChangesAsync();
            return Results.Ok(new { pong = true, messageId = message.Id });
        });
    }
}
