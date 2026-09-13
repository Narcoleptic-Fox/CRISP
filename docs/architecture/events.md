---
title: Events
description: The in-process event bus: BaseEvent, IEventPublisher, IEventHandler and the EventWorker that drains the channel.
nav_order: 5
---

# Events

A lightweight, channel-based, in-process bus for decoupling side effects from the write that
triggers them. Contracts live in `Server/Common`; event records live in `Server/Events`.

```csharp
public abstract record BaseEvent { public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow; }
public interface IEventPublisher { ValueTask Publish<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : BaseEvent; }
public interface IEventHandler<in TEvent> where TEvent : BaseEvent { ValueTask HandleAsync(TEvent @event, CancellationToken ct = default); }

// Server/Events/ItemEvents.cs
public sealed record ItemRenamedEvent(Guid ItemId, string OldName, string NewName) : BaseEvent;
```

## How it runs

- **`EventPublisher`** (singleton) owns a bounded `Channel<BaseEvent>` (capacity 500,
  `FullMode = Wait`, single reader). `Publish` writes to the channel (or waits when full).
- **`EventWorker`** (a `BackgroundService`) drains the channel, resolves
  `IEventHandler<TEvent>` from a **fresh DI scope per event**, dispatches to every registered
  handler, and retries with exponential backoff (`EventWorkerOptions.MaxAttempts`). A failing
  handler is isolated — it never stops the worker or blocks others. The generic dispatch
  method is closed per event type and cached (reflection cost paid once per type).
- Wire it with `builder.Services.AddEventInfrastructure(...)`. Register handlers in the
  feature's `AddFeature`.

## Usage

Publish from a write handler **after** persistence; react in a separate handler:

```csharp
// In UpdateItemService.Send, after SaveChangesAsync — publisher injected via the constructor:
if (!string.Equals(previousName, command.Name, StringComparison.Ordinal))
    await publisher.Publish(new ItemRenamedEvent(entity.Id, previousName, command.Name), ct);

// Decoupled reaction, registered as IEventHandler<ItemRenamedEvent> in ItemFeature:
internal sealed class ItemRenamedLogger(ILogger<ItemRenamedLogger> logger) : IEventHandler<ItemRenamedEvent>
{
    public ValueTask HandleAsync(ItemRenamedEvent e, CancellationToken ct = default)
    {
        logger.LogInformation("Item {ItemId} renamed from '{OldName}' to '{NewName}'.", e.ItemId, e.OldName, e.NewName);
        return ValueTask.CompletedTask;
    }
}
```

Use this bus for side effects (notifications, projections, logging) — do **not** inline them
into the write handler. Dispatch is asynchronous and best-effort with retry; do not rely on it
for transactional consistency with the originating write.

Source: `src/CRISP.Server/Features/Items/UpdateItemEndpoint.cs` (publisher), `src/CRISP.Server/Features/Items/Events/ItemRenamedLogger.cs` (handler), `src/CRISP.Server/Infrastructure/Events/` (bus).
