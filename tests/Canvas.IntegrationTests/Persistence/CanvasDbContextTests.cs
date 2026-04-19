using Canvas.Infrastructure.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Canvas.IntegrationTests.Persistence;

[Trait("Category", "Integration")]
public class CanvasDbContextTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder().Build();
    private CanvasDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<CanvasDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _dbContext = new CanvasDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task DbContext_CanConnectToDatabase()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task OutboxMessages_TableExists_AndIsEmpty()
    {
        var count = await _dbContext.OutboxMessages.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task OutboxMessages_CanInsertAndRetrieve()
    {
        var message = new Canvas.Infrastructure.Shared.Outbox.OutboxMessage
        {
            Type = "TestEvent",
            Content = """{"id":1}""",
            OccurredOn = DateTimeOffset.UtcNow
        };

        _dbContext.OutboxMessages.Add(message);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.OutboxMessages.FindAsync(message.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Type.Should().Be("TestEvent");
    }
}
