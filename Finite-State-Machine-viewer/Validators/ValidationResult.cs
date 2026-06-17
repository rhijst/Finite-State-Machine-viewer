namespace FSM.Validators;

public class ValidationResult
{
    private readonly List<string> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string message) => _errors.Add(message);

    public static ValidationResult Ok() => new();
    public static ValidationResult Fail(string message)
    {
        var r = new ValidationResult();
        r.AddError(message);
        return r;
    }
}
