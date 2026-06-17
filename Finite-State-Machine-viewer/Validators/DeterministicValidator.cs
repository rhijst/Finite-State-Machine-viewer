using FSM.Model;
using FSM.Model.States;

namespace FSM.Validators;

public class DeterministicValidator : IFSMValidator
{
    public ValidationResult Validate(FiniteStateMachine fsm)
    {
        var result = new ValidationResult();

        foreach (var state in fsm.States)
        {
            var outgoing = fsm.GetOutgoingTransitions(state).ToList();
            if (outgoing.Count <= 1) continue;

            // A state with an automatic transition cannot have any other transitions
            if (outgoing.Any(t => t.IsAutomatic))
            {
                result.AddError(
                    $"State '{state.Name}' has an automatic transition alongside other transitions (non-deterministic).");
                continue;
            }

            // Two transitions with same trigger AND same guard are non-deterministic
            var groups = outgoing.GroupBy(t => t.Trigger?.Id ?? string.Empty);
            foreach (var group in groups)
            {
                var list = group.ToList();
                if (list.Count <= 1) continue;

                var guardGroups = list.GroupBy(t => t.Guard ?? string.Empty);
                foreach (var guardGroup in guardGroups)
                {
                    if (guardGroup.Count() > 1)
                    {
                        result.AddError(
                            $"State '{state.Name}' has non-deterministic transitions: " +
                            $"trigger '{group.Key}' with guard '{guardGroup.Key}' appears {guardGroup.Count()} times.");
                    }
                }
            }
        }

        return result;
    }
}
