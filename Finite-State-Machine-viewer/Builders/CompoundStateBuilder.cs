using FSM.Model.States;

namespace FSM.Builders;

public class CompoundStateBuilder : StateBuilder<CompoundState, CompoundStateBuilder>
{
    public override CompoundState Build() =>
        new(_id, _name, _entryActions, _doActions, _exitActions);
}
