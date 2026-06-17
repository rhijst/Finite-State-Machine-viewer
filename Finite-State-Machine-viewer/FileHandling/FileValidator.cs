namespace FSM.FileHandling;

public class FileValidator : IFileValidator
{
    private readonly List<string> _errors = new();

    public bool Validate(string content)
    {
        _errors.Clear();

        var lines = content.Split('\n')
            .Select(l => l.Trim('\r', ' '))
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
            .ToList();

        foreach (var line in lines)
        {
            if (!line.TrimEnd().EndsWith(';'))
            {
                _errors.Add($"Line does not end with ';': {line}");
                continue;
            }

            var keyword = line.Split(' ')[0].ToUpperInvariant();
            switch (keyword)
            {
                case "STATE":     ValidateStateLine(line);      break;
                case "TRIGGER":   ValidateTriggerLine(line);    break;
                case "ACTION":    ValidateActionLine(line);     break;
                case "TRANSITION":ValidateTransitionLine(line); break;
                default:
                    _errors.Add($"Unknown keyword '{keyword}' in line: {line}");
                    break;
            }
        }

        return _errors.Count == 0;
    }

    public IEnumerable<string> GetErrors() => _errors;

    private void ValidateStateLine(string line)
    {
        // Tokens: STATE(0) id(1) parent(2) name(3) :(4) type(5)
        var tokens = Tokenize(line);
        if (tokens.Count < 6 || tokens[0] != "STATE" || tokens[4] != ":" ||
            !new[] { "INITIAL", "SIMPLE", "COMPOUND", "FINAL" }.Contains(tokens[5]))
            _errors.Add($"Invalid STATE definition: {line}");
    }

    private void ValidateTriggerLine(string line)
    {
        var tokens = Tokenize(line);
        if (tokens.Count < 3 || tokens[0] != "TRIGGER")
            _errors.Add($"Invalid TRIGGER definition: {line}");
    }

    private void ValidateActionLine(string line)
    {
        var tokens = Tokenize(line);
        if (tokens.Count < 5 || tokens[0] != "ACTION" || tokens[3] != ":" ||
            !new[] { "ENTRY_ACTION", "DO_ACTION", "EXIT_ACTION", "TRANSITION_ACTION" }.Contains(tokens[4]))
            _errors.Add($"Invalid ACTION definition: {line}");
    }

    private void ValidateTransitionLine(string line)
    {
        var tokens = Tokenize(line);
        if (tokens.Count < 5 || tokens[0] != "TRANSITION" || tokens[3] != "->")
            _errors.Add($"Invalid TRANSITION definition: {line}");
    }

    private static List<string> Tokenize(string line)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < line.Length)
        {
            if (char.IsWhiteSpace(line[i])) { i++; continue; }
            if (line[i] == '#') break;
            if (line[i] == ';') { i++; break; }
            if (line[i] == '"')
            {
                var sb = new System.Text.StringBuilder();
                i++;
                while (i < line.Length && line[i] != '"') sb.Append(line[i++]);
                if (i < line.Length) i++;
                tokens.Add(sb.ToString());
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                while (i < line.Length && !char.IsWhiteSpace(line[i]) && line[i] != ';')
                    sb.Append(line[i++]);
                tokens.Add(sb.ToString());
            }
        }
        return tokens;
    }
}
