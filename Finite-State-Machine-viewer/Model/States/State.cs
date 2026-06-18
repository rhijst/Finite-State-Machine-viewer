using FSM.Visitors;

namespace FSM.Model.States;

public abstract class State : IElement
{
    public string Id { get; }
    public string Name { get; }
    public List<Action> EntryActions { get; } = new();
    public List<Action> DoActions { get; } = new();
    public List<Action> ExitActions { get; } = new();

    protected State(string id, string name,
        IEnumerable<Action> entryActions,
        IEnumerable<Action> doActions,
        IEnumerable<Action> exitActions)
    {
        Id = id;
        Name = name;
        EntryActions.AddRange(entryActions);
        DoActions.AddRange(doActions);
        ExitActions.AddRange(exitActions);
    }

    public abstract void Accept(IVisitor visitor);

    public override string ToString() { return Name; }
}
