namespace Canvas.Infrastructure.Shared.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset OccurredOn { get; init; }
    public DateTimeOffset? ProcessedOn { get; private set; }
    public string? Error { get; private set; }

    public void MarkProcessed() => ProcessedOn = DateTimeOffset.UtcNow;
    public void MarkFailed(string error) => Error = error;
}
