using FSM.Model;
using FSM.Model.States;

namespace FSM.Visitors;

public interface IVisitor
{
    void BeginFsm(FiniteStateMachine fsm);
    void EndFsm(FiniteStateMachine fsm);
    void Visit(InitialState state);
    void Visit(FinalState state);
    void Visit(SimpleState state);
    void Visit(CompoundState state);
    void Visit(Transition transition);
}
