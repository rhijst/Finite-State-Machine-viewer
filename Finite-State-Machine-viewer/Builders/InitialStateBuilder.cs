using FSM.Model.States;

namespace FSM.Builders;

public class InitialStateBuilder : StateBuilder<InitialState, InitialStateBuilder>
{
    public override InitialState Build() => new(_id, _name);
}
