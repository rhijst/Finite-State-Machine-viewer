using FSM.Model.States;
using FSM.Visitors;

namespace FSM.Model;

public class FiniteStateMachine
{
    public string Name { get; }
    public IReadOnlyList<State> States { get; }
    public IReadOnlyList<Transition> Transitions { get; }

    public FiniteStateMachine(string name, IEnumerable<State> states, IEnumerable<Transition> transitions)
    {
        Name = name;
        States = states.ToList();
        Transitions = transitions.ToList();
    }

    public InitialState? GetInitialState() => States.OfType<InitialState>().FirstOrDefault();

    public IEnumerable<State> GetTopLevelStates() =>
        States.Where(s => !States.OfType<CompoundState>().Any(cs => cs.Children.Contains(s)));

    public IEnumerable<Transition> GetOutgoingTransitions(State state) =>
        Transitions.Where(t => t.Source == state);

    public IEnumerable<Transition> GetIncomingTransitions(State state) =>
        Transitions.Where(t => t.Destination == state);

    public CompoundState? GetParent(State state) =>
        States.OfType<CompoundState>().FirstOrDefault(cs => cs.Children.Contains(state));

    public void Render(IVisitor visitor)
    {
        visitor.BeginFsm(this);
        foreach (var state in GetTopLevelStates())
            state.Accept(visitor);
        visitor.EndFsm(this);
    }
}
