using System.Text;
using FSM.FileHandling.DTO;
using FSM.Model;

namespace FSM.FileHandling;

public class FileInterpreter : IFileInterpreter
{
    public FSMDTO Interpret(CustomFile file)
    {
        var dto = new FSMDTO
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(file.Path)
        };

        var lines = file.Content.Split('\n')
            .Select(l => l.Trim('\r'))
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith('#'));

        foreach (var line in lines)
        {
            var tokens = Tokenize(line);
            if (tokens.Count == 0) continue;

            switch (tokens[0].ToUpperInvariant())
            {
                case "STATE":
                    dto.States.Add(ParseState(tokens));
                    break;
                case "TRIGGER":
                    dto.Triggers.Add(ParseTrigger(tokens));
                    break;
                case "ACTION":
                    dto.Actions.Add(ParseAction(tokens));
                    break;
                case "TRANSITION":
                    dto.Transitions.Add(ParseTransition(tokens));
                    break;
            }
        }

        return dto;
    }

    private static string Unquote(string token) =>
        token.StartsWith('\x01') ? token[1..] : token;

    // STATE <id> <parent|_> "<name>" : <type>;
    private static StateDTO ParseState(List<string> t)
    {
        if (t.Count < 6)
            throw new FormatException($"Invalid STATE: {string.Join(" ", t)}");

        string id = t[1];
        string parentId = t[2];
        string name = Unquote(t[3]);
        // t[4] == ":"
        StateType stateType = ParseStateType(t[5]);

        return new StateDTO(id, parentId, name, stateType);
    }

    private static StateType ParseStateType(string raw) => raw switch
    {
        "INITIAL"  => StateType.Initial,
        "SIMPLE"   => StateType.Simple,
        "COMPOUND" => StateType.Compound,
        "FINAL"    => StateType.Final,
        _ => throw new FormatException($"Unknown state type: '{raw}'")
    };

    // TRIGGER <id> "<description>";
    private static TriggerDTO ParseTrigger(List<string> t)
    {
        if (t.Count < 3)
            throw new FormatException($"Invalid TRIGGER: {string.Join(" ", t)}");

        return new TriggerDTO(t[1], Unquote(t[2]));
    }

    // ACTION <owner_id> "<description>" : <type>;
    private static ActionDTO ParseAction(List<string> t)
    {
        if (t.Count < 5)
            throw new FormatException($"Invalid ACTION: {string.Join(" ", t)}");

        string ownerId = t[1];
        string description = Unquote(t[2]);
        // t[3] == ":"
        ActionType actionType = ParseActionType(t[4]);

        return new ActionDTO(ownerId, description, actionType);
    }

    private static ActionType ParseActionType(string raw) => raw switch
    {
        "ENTRY_ACTION"      => ActionType.EntryAction,
        "DO_ACTION"         => ActionType.DoAction,
        "EXIT_ACTION"       => ActionType.ExitAction,
        "TRANSITION_ACTION" => ActionType.TransitionAction,
        _ => throw new FormatException($"Unknown action type: '{raw}'")
    };

    // TRANSITION <id> <src> -> <dst> [<trigger>] ["<guard>"];
    private static TransitionDTO ParseTransition(List<string> t)
    {
        if (t.Count < 5)
            throw new FormatException($"Invalid TRANSITION: {string.Join(" ", t)}");

        string id = t[1];
        string sourceId = t[2];
        // t[3] == "->"
        string destinationId = t[4];

        string? triggerId = null;
        string? guard = null;

        int idx = 5;
        if (idx < t.Count)
        {
            // Marked quoted strings have a special marker — see tokenizer
            if (t[idx].StartsWith('\x01'))
            {
                // It's a quoted string (guard), no trigger
                guard = t[idx][1..];
                if (guard.Length == 0) guard = null;
            }
            else
            {
                // It's a trigger identifier
                triggerId = t[idx];
                idx++;
                if (idx < t.Count && t[idx].StartsWith('\x01'))
                {
                    guard = t[idx][1..];
                    if (guard.Length == 0) guard = null;
                }
            }
        }

        return new TransitionDTO(id, sourceId, destinationId, triggerId, guard);
    }

    // Tokenizer that marks quoted strings with \x01 prefix to distinguish from identifiers
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
                // Quoted string: mark with \x01 prefix, content without quotes
                var sb = new StringBuilder("\x01");
                i++;
                while (i < line.Length && line[i] != '"') sb.Append(line[i++]);
                if (i < line.Length) i++; // skip closing quote
                tokens.Add(sb.ToString());
            }
            else
            {
                var sb = new StringBuilder();
                while (i < line.Length && !char.IsWhiteSpace(line[i]) && line[i] != ';' && line[i] != '"')
                    sb.Append(line[i++]);
                if (sb.Length > 0) tokens.Add(sb.ToString());
            }
        }
        return tokens;
    }
}
