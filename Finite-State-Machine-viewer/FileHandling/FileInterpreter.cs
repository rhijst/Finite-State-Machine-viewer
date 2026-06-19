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
            var tokens = FileTokenizer.Tokenize(line);
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

    // STATE <id> <parent|_> "<name>" : <type>;
    private static StateDTO ParseState(List<string> stateTokens)
    {
        if (stateTokens.Count < 6)
            throw new FormatException($"Invalid STATE: {string.Join(" ", stateTokens)}");

        string id = stateTokens[1];
        string parentId = stateTokens[2];
        string name = FileTokenizer.Unquote(stateTokens[3]);
        // t[4] == ":"
        StateType stateType = ParseStateType(stateTokens[5]);

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
    private static TriggerDTO ParseTrigger(List<string> stateTokens)
    {
        if (stateTokens.Count < 3)
            throw new FormatException($"Invalid TRIGGER: {string.Join(" ", stateTokens)}");

        return new TriggerDTO(stateTokens[1], FileTokenizer.Unquote(stateTokens[2]));
    }

    // ACTION <owner_id> "<description>" : <type>;
    private static ActionDTO ParseAction(List<string> stateTokens)
    {
        if (stateTokens.Count < 5)
            throw new FormatException($"Invalid ACTION: {string.Join(" ", stateTokens)}");

        string ownerId = stateTokens[1];
        string description = FileTokenizer.Unquote(stateTokens[2]);
        // t[3] == ":"
        ActionType actionType = ParseActionType(stateTokens[4]);

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
    private static TransitionDTO ParseTransition(List<string> stateTokens)
    {
        if (stateTokens.Count < 5)
            throw new FormatException($"Invalid TRANSITION: {string.Join(" ", stateTokens)}");

        string id = stateTokens[1];
        string sourceId = stateTokens[2];
        // t[3] == "->"
        string destinationId = stateTokens[4];

        string? triggerId = null;
        string? guard = null;

        int i = 5;
        if (i < stateTokens.Count)
        {
            if (FileTokenizer.IsQuoted(stateTokens[i]))
            {
                guard = FileTokenizer.Unquote(stateTokens[i]);
                if (guard.Length == 0) guard = null;
            }
            else
            {
                triggerId = stateTokens[i];
                i++;
                if (i < stateTokens.Count && FileTokenizer.IsQuoted(stateTokens[i]))
                {
                    guard = FileTokenizer.Unquote(stateTokens[i]);
                    if (guard.Length == 0) guard = null;
                }
            }
        }

        return new TransitionDTO(id, sourceId, destinationId, triggerId, guard);
    }

}
