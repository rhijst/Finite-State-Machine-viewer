using FSM.Model;
using FSM.Model.States;
using Action = FSM.Model.Action;

namespace FSM.Builders;

public class TransitionBuilder
{
    private string _id = string.Empty;
    private State? _source;
    private State? _destination;
    private Trigger? _trigger;
    private string? _guard;
    private readonly List<Action> _effects = new();

    public TransitionBuilder WithId(string id) { _id = id; return this; }
    public TransitionBuilder WithSource(State source) { _source = source; return this; }
    public TransitionBuilder WithDestination(State destination) { _destination = destination; return this; }
    public TransitionBuilder WithTrigger(Trigger? trigger) { _trigger = trigger; return this; }
    public TransitionBuilder WithGuard(string? guard) { _guard = guard; return this; }
    public TransitionBuilder AddEffect(Action effect) { _effects.Add(effect); return this; }

    public Transition Build()
    {
        if (_source is null) throw new InvalidOperationException("Source state is required.");
        if (_destination is null) throw new InvalidOperationException("Destination state is required.");
        return new Transition(_id, _source, _destination, _trigger, _guard, _effects);
    }
}
