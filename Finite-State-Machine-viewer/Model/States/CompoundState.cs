using FSM.Visitors;

namespace FSM.Model.States;

public class CompoundState : State
{
    private readonly List<State> _children = new();
    public IReadOnlyList<State> Children => _children;

    public CompoundState(string id, string name,
        IEnumerable<Action> entryActions,
        IEnumerable<Action> doActions,
        IEnumerable<Action> exitActions)
        : base(id, name, entryActions, doActions, exitActions) { }

    public void AddChild(State state)
    {
        _children.Add(state);
    }

    public void RemoveChild(State state)
    {
        _children.Remove(state);
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
