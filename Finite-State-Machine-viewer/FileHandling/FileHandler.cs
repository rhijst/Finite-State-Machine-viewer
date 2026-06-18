using FSM.FileHandling.DTO;

namespace FSM.FileHandling;

public class FileHandler
{
    private readonly IFileReader _reader;
    private readonly IFileInterpreter _interpreter;
    private readonly IFileValidator _validator;

    public FileHandler(IFileReader reader, IFileInterpreter interpreter, IFileValidator validator)
    {
        _reader = reader;
        _interpreter = interpreter;
        _validator = validator;
    }

    public CustomFile LoadFile(string path)
    {
        return _reader.Read(path);
    }

    public CustomFile LoadFile(string path, string file, string extension = ".fsm")
    {
        return _reader.Read(System.IO.Path.Combine(path, file + extension));
    }

    public bool ValidateFileContent(string content)
    {
        return _validator.Validate(content);
    }

    public IEnumerable<string> GetValidationErrors() => _validator.GetErrors();

    public FSMDTO InterpretFileContent(CustomFile file) => _interpreter.Interpret(file);

    public FSMDTO ReadAndInterpret(string path)
    {
        var file = LoadFile(path);
        if (!ValidateFileContent(file.Content))
            throw new InvalidOperationException("File validation failed:\n" + string.Join("\n", GetValidationErrors()));
        return InterpretFileContent(file);
    }
}
