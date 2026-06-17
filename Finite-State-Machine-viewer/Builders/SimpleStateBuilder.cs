using FSM.Model.States;

namespace FSM.Builders;

public class SimpleStateBuilder : StateBuilder<SimpleState, SimpleStateBuilder>
{
    public override SimpleState Build() =>
        new(_id, _name, _entryActions, _doActions, _exitActions);
}
