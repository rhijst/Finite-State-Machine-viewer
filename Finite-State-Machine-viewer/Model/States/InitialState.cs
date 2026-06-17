using FSM.Visitors;

namespace FSM.Model.States;

public class InitialState : State
{
    public InitialState(string id, string name) : base(id, name, [], [], []) { }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
