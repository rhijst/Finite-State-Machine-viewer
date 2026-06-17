using FSM.Model;

namespace FSM.Validators;

public interface IFSMValidator
{
    ValidationResult Validate(FiniteStateMachine fsm);
}
