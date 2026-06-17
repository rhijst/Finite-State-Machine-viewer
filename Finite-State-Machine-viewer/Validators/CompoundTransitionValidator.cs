using FSM.Model;
using FSM.Model.States;

namespace FSM.Validators;

public class CompoundTransitionValidator : IFSMValidator
{
    public ValidationResult Validate(FiniteStateMachine fsm)
    {
        var result = new ValidationResult();

        foreach (var transition in fsm.Transitions)
        {
            if (transition.Destination is CompoundState)
            {
                result.AddError(
                    $"Transition '{transition.Id}' ends at compound state '{transition.Destination.Name}'. " +
                    $"Transitions must end at a simple state inside the compound state.");
            }
        }

        return result;
    }
}
