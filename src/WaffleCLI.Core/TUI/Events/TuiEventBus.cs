using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Events;

public class TuiEventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = [];
        
        _handlers[eventType].Add(handler);
    }
    
    public void Publish<TEvent>(TEvent @event)
    {
        var eventType = typeof(TEvent);
        if (!_handlers.TryGetValue(eventType, out var value)) return;
        foreach (var handler in value)
        {
            ((Action<TEvent>)handler)(@event);
        }
    }
}

public record ComponentCreatedEvent(ITuiComponent Component);
public record ComponentDestroyedEvent(ITuiComponent Component);
public record ScreenChangedEvent(ITuiScreen From, ITuiScreen To);
public record FocusChangedEvent(ITuiElement OldElement, ITuiElement NewElement);