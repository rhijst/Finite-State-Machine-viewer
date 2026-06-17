namespace FSM.FileHandling;

public interface IFileValidator
{
    bool Validate(string content);
    IEnumerable<string> GetErrors();
}
