using FSM.Visitors;

namespace FSM.Model.States;

public class SimpleState : State
{
    public SimpleState(string id, string name,
        IEnumerable<Action> entryActions,
        IEnumerable<Action> doActions,
        IEnumerable<Action> exitActions)
        : base(id, name, entryActions, doActions, exitActions) { }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
