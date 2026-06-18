using FSM.Visitors;

namespace FSM.Model.States;

public class FinalState : State
{
    public FinalState(string id, string name) : base(id, name, [], [], []) { }

    public override void Accept(IVisitor visitor) {
        visitor.Visit(this);
    }
}
