using FSM.Model.States;
using FSM.Visitors;

namespace FSM.Model;

public class Transition : IElement
{
    public string Id { get; }
    public State Source { get; }
    public State Destination { get; }
    public Trigger? Trigger { get; }
    public string? Guard { get; }
    public List<Action> Effects { get; } = new();

    // True only for unconditional completion transitions (no trigger AND no guard)
    public bool IsAutomatic => Trigger is null && string.IsNullOrEmpty(Guard);

    public Transition(string id, State source, State destination,
        Trigger? trigger, string? guard, IEnumerable<Action> effects)
    {
        Id = id;
        Source = source;
        Destination = destination;
        Trigger = trigger;
        Guard = guard;
        Effects.AddRange(effects);
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);

    public string GetLabel()
    {
        var parts = new List<string>();
        if (Trigger is not null) parts.Add(Trigger.Description);
        if (!string.IsNullOrEmpty(Guard)) parts.Add($"[{Guard}]");
        if (Effects.Count > 0) parts.Add($"/ {string.Join(", ", Effects.Select(e => e.Description))}");
        return string.Join(" ", parts);
    }

    public override string ToString() => $"{Source.Name} --{GetLabel()}--> {Destination.Name}";
}
