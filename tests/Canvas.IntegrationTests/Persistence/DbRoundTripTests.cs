using Canvas.Infrastructure.Shared.Outbox;
using Canvas.Infrastructure.Shared.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Canvas.IntegrationTests.Persistence;

public class DbRoundTripTests : IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder().Build();
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, cfg) =>
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:CanvasDb"] = _db.GetConnectionString()
                    })));

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CanvasDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _db.StopAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MigrateAsync_AppliesMigrations_SchemaExists()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CanvasDbContext>();

        var canConnect = await db.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OutboxMessages_CanInsert_AndRetrieve_ViaMigration()
    {
        var message = new OutboxMessage
        {
            Type = "TestEvent",
            Content = """{"key":"value"}""",
            OccurredOn = DateTimeOffset.UtcNow
        };

        await using var writeScope = _factory.Services.CreateAsyncScope();
        var writeDb = writeScope.ServiceProvider.GetRequiredService<CanvasDbContext>();
        writeDb.OutboxMessages.Add(message);
        await writeDb.SaveChangesAsync();

        await using var readScope = _factory.Services.CreateAsyncScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<CanvasDbContext>();
        var retrieved = await readDb.OutboxMessages.FindAsync(message.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Type.Should().Be("TestEvent");
        retrieved.ProcessedOn.Should().BeNull();
    }
}
