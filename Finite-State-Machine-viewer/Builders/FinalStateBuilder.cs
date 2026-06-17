using FSM.Model.States;

namespace FSM.Builders;

public class FinalStateBuilder : StateBuilder<FinalState, FinalStateBuilder>
{
    public override FinalState Build() => new(_id, _name);
}
