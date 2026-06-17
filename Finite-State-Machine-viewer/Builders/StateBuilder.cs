using FSM.Model;
using FSM.Model.States;
using Action = FSM.Model.Action;

namespace FSM.Builders;

public abstract class StateBuilder<TState, TBuilder>
    where TState : State
    where TBuilder : StateBuilder<TState, TBuilder>
{
    protected string _id = string.Empty;
    protected string _name = string.Empty;
    protected readonly List<Action> _entryActions = new();
    protected readonly List<Action> _doActions = new();
    protected readonly List<Action> _exitActions = new();

    public TBuilder WithId(string id) { _id = id; return (TBuilder)this; }
    public TBuilder WithName(string name) { _name = name; return (TBuilder)this; }
    public TBuilder AddEntryAction(Action action) { _entryActions.Add(action); return (TBuilder)this; }
    public TBuilder AddDoAction(Action action) { _doActions.Add(action); return (TBuilder)this; }
    public TBuilder AddExitAction(Action action) { _exitActions.Add(action); return (TBuilder)this; }

    public abstract TState Build();
}
