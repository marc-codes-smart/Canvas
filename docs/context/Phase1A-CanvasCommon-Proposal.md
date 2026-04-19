# Phase 1A — Canvas.Common: Scope & Folder Structure Proposal

**Status:** Awaiting Marc's review and approval before any code is written.
**Jira story:** SCRUM-6 (sub-tasks: SCRUM-13 through SCRUM-17)

---

## What exists today

The scaffold commit created three empty projects (no `.cs` source files):

```
src/shared/Canvas.Common/            ← references ErrorOr 2.0.1
tests/Canvas.Common.Tests/           ← xUnit, FluentAssertions, NSubstitute
tests/Canvas.ArchitectureTests/      ← NetArchTest.Rules
```

The solution file is `Canvas.slnx` (the new XML-based format — fine, .NET 10 supports it).

---

## Proposed folder structure for Canvas.Common

```
src/shared/Canvas.Common/
├── Canvas.Common.csproj
├── Primitives/
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   └── ValueObject.cs
├── Events/
│   └── DomainEvent.cs
└── Errors/
    └── DomainException.cs
```

**Why this grouping (not flat):**
- `Primitives/` contains the core building blocks that every module's Domain layer depends on.
- `Events/` is distinct because domain events participate in a different lifecycle (collected on aggregates, dispatched after commit) — grouping them separately makes that lifecycle obvious.
- `Errors/` holds exception types. Note: we are *not* defining a custom `Error` value type — the `ErrorOr` package (already referenced) provides `ErrorOr.Error`. `DomainException` is for unrecoverable invariant violations thrown inside the domain.

---

## Proposed type definitions (intent, not final code)

### `Primitives/Entity.cs`
```csharp
// Generic base for all domain entities. TId should be a Vogen-generated
// strongly-typed ID (defined in each module, not here).
public abstract class Entity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; } = id;

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public IReadOnlyList<DomainEvent> PopDomainEvents()
    {
        var copy = _domainEvents.ToList();
        _domainEvents.Clear();
        return copy;
    }

    // Equality by ID
    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Id.Equals(other.Id);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}
```

### `Primitives/AggregateRoot.cs`
```csharp
// Marker for aggregate roots. An aggregate root is the entry point
// for a consistency boundary. Cross-aggregate communication is via events only.
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : notnull;
```

### `Primitives/ValueObject.cs`
```csharp
// Base for value objects. Equality is structural — by all component values.
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);
}
```

### `Events/DomainEvent.cs`
```csharp
// Abstract base for all domain events.
// Using record for structural equality and positional syntax.
// OccurredOn is set at construction to the moment the event was raised.
public abstract record DomainEvent(DateTimeOffset OccurredOn)
{
    protected DomainEvent() : this(DateTimeOffset.UtcNow) { }
}
```

### `Errors/DomainException.cs`
```csharp
// Thrown when a domain invariant is violated (a programming error, not
// a user-correctable condition — user-correctable errors use ErrorOr).
public sealed class DomainException(string message) : Exception(message);
```

---

## Open questions — decisions needed before code is written

1. **`DomainEvent` as record vs class?**
   Proposal uses `record` (value-semantic, immutable by default, nice `ToString()`). The alternative is `abstract class` (more explicit, no accidental equality comparisons). Recommendation: **record**.

2. **`Vogen` in Canvas.Common?**
   Vogen is *not* needed in Canvas.Common itself — `Entity<TId>` accepts any `TId : notnull`. Each module project will reference Vogen and define its own IDs (`IdeaId`, `ProjectId`, etc.). Canvas.Common stays dependency-free (except ErrorOr). Recommendation: **don't add Vogen here**.

3. **Should `ErrorOr` stay in Canvas.Common, or move to a layer higher?**
   `ErrorOr` is used by Application-layer handlers (commands/queries return `ErrorOr<T>`). Technically Canvas.Common doesn't consume it — it's just a convenient place to pull the transitive dependency. Alternative: remove it from Canvas.Common and add it in each module. Recommendation: **remove from Canvas.Common; add when needed in module projects**. This keeps Canvas.Common's dependency list truly zero project-level and minimal package-level.

4. **`PopDomainEvents` name vs `ClearDomainEvents`?**
   `Pop` implies "take and clear" (like a stack). Some codebases use separate `DomainEvents` + `ClearDomainEvents()`. Recommendation: **keep `PopDomainEvents`** — one call, clear intent.

---

## Architecture test (ArchitectureTests/CanvasCommonTests.cs)

```csharp
// Assert Canvas.Common has no references to any other Canvas.* assembly.
[Fact]
public void CanvasCommon_ShouldNotDependOnAnyOtherCanvasAssembly()
{
    var result = Types
        .InAssembly(typeof(Entity<>).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny("Canvas.Infrastructure", "Canvas.Host",
                             "Canvas.Idea", "Canvas.Project",
                             "Canvas.Planning", "Canvas.Architecture",
                             "Canvas.Memory", "Canvas.Integration")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

*Note: This test uses explicit module names. As modules are created in Phase 2+, they get added to the list. An alternative is a regex/prefix match (`"Canvas."` excluding `"Canvas.Common"`) — worth discussing.*

---

## Unit test plan (Canvas.Common.Tests)

All in `Canvas.Common.Tests/`, mirroring the `src/` folder grouping:

| File | Tests |
|---|---|
| `Primitives/EntityTests.cs` | ID stored correctly; equality by ID (same ID = equal, different ID ≠ equal); events raised, accumulated, popped (list cleared); operators == and !=. |
| `Primitives/AggregateRootTests.cs` | Inherits from Entity; domain events still work. |
| `Primitives/ValueObjectTests.cs` | Equal when all components equal; not equal when any component differs; `GetHashCode` consistent with equality; `==` / `!=` operators. |
| `Events/DomainEventTests.cs` | `OccurredOn` is set; record equality (two events with same type + date are equal). |
| `Errors/DomainExceptionTests.cs` | Message propagated; is-an `Exception`. |

Target: **100% line coverage** on Canvas.Common (all paths are exercisable; no external I/O to stub out).

---

## Summary of what needs Marc's approval

1. ✅ or ❌ the proposed **folder structure** (`Primitives/`, `Events/`, `Errors/`)
2. Decision on **`DomainEvent` record vs class**
3. Decision on **removing ErrorOr from Canvas.Common.csproj** (move to modules)
4. Any changes to the type signatures above

Once approved, sub-tasks SCRUM-13 through SCRUM-17 can proceed in order.
