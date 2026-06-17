using FSM.Model;
using FSM.Model.States;
using Action = FSM.Model.Action;

namespace FSM.Builders;

public class FSMDirector
{
    public InitialState MakeInitialState(InitialStateBuilder builder, string id, string name)
    {
        builder.WithId(id).WithName(name);
        return builder.Build();
    }

    public FinalState MakeFinalState(FinalStateBuilder builder, string id, string name)
    {
        builder.WithId(id).WithName(name);
        return builder.Build();
    }

    public SimpleState MakeSimpleState(SimpleStateBuilder builder, string id, string name,
        IEnumerable<Action> entryActions, IEnumerable<Action> doActions, IEnumerable<Action> exitActions)
    {
        builder.WithId(id).WithName(name);
        foreach (var a in entryActions) builder.AddEntryAction(a);
        foreach (var a in doActions) builder.AddDoAction(a);
        foreach (var a in exitActions) builder.AddExitAction(a);
        return builder.Build();
    }

    public CompoundState MakeCompoundState(CompoundStateBuilder builder, string id, string name,
        IEnumerable<Action> entryActions, IEnumerable<Action> doActions, IEnumerable<Action> exitActions)
    {
        builder.WithId(id).WithName(name);
        foreach (var a in entryActions) builder.AddEntryAction(a);
        foreach (var a in doActions) builder.AddDoAction(a);
        foreach (var a in exitActions) builder.AddExitAction(a);
        return builder.Build();
    }

    public Transition MakeTransition(TransitionBuilder builder, string id, State source,
        State destination, Trigger? trigger, string? guard, IEnumerable<Action> effects)
    {
        builder.WithId(id).WithSource(source).WithDestination(destination)
               .WithTrigger(trigger).WithGuard(guard);
        foreach (var e in effects) builder.AddEffect(e);
        return builder.Build();
    }
}
