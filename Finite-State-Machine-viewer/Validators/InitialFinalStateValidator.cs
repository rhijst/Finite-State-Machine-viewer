using FSM.Model;
using FSM.Model.States;

namespace FSM.Validators;

public class InitialFinalStateValidator : IFSMValidator
{
    public ValidationResult Validate(FiniteStateMachine fsm)
    {
        var result = new ValidationResult();

        foreach (var state in fsm.States.OfType<InitialState>())
        {
            if (fsm.GetIncomingTransitions(state).Any())
                result.AddError($"Initial state '{state.Name}' has incoming transitions (not allowed).");
        }

        foreach (var state in fsm.States.OfType<FinalState>())
        {
            if (fsm.GetOutgoingTransitions(state).Any())
                result.AddError($"Final state '{state.Name}' has outgoing transitions (not allowed).");
        }

        return result;
    }
}
