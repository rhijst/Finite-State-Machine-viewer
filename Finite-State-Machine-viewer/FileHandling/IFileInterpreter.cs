using FSM.FileHandling.DTO;

namespace FSM.FileHandling;

public interface IFileInterpreter
{
    FSMDTO Interpret(CustomFile file);
}
