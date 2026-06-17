using FSM.Model;
using FSM.Model.States;

namespace FSM.Validators;

public class UnreachableStateValidator : IFSMValidator
{
    public ValidationResult Validate(FiniteStateMachine fsm)
    {
        var result = new ValidationResult();
        var initial = fsm.GetInitialState();
        if (initial is null) return result;

        var reachable = new HashSet<State>();
        Traverse(initial, fsm, reachable);

        foreach (var state in fsm.States)
        {
            if (state is InitialState) continue;
            if (!reachable.Contains(state))
                result.AddError($"State '{state.Name}' ({state.Id}) is unreachable.");
        }

        return result;
    }

    private static void Traverse(State current, FiniteStateMachine fsm, HashSet<State> visited)
    {
        if (!visited.Add(current)) return;

        // Entering a child makes its parent compound state reachable too
        var parent = fsm.GetParent(current);
        if (parent is not null) Traverse(parent, fsm, visited);

        // Entering a compound state makes all its children reachable
        if (current is CompoundState compound)
            foreach (var child in compound.Children)
                Traverse(child, fsm, visited);

        foreach (var transition in fsm.GetOutgoingTransitions(current))
            Traverse(transition.Destination, fsm, visited);
    }
}
