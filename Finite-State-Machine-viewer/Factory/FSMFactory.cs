using FSM.Builders;
using FSM.FileHandling.DTO;
using FSM.Model;
using FSM.Model.States;
using Action = FSM.Model.Action;

namespace FSM.Factory;

public class FSMFactory
{
    private readonly FSMDirector _director = new();

    public FiniteStateMachine Create(FSMDTO dto)
    {
        var actionGroups = GroupActions(dto);
        var triggers = dto.Triggers.ToDictionary(t => t.Id, t => new Trigger(t.Id, t.Description));
        var states = BuildStates(dto, actionGroups);

        SetParentChildRelationships(dto, states);

        var transitions = BuildTransitions(dto, states, triggers, actionGroups);

        return new FiniteStateMachine(dto.Name, states.Values, transitions);
    }

    private static Dictionary<string, (List<Action> Entry, List<Action> Do, List<Action> Exit, List<Action> Transition)>
        GroupActions(FSMDTO dto)
    {
        var groups = new Dictionary<string, (List<Action>, List<Action>, List<Action>, List<Action>)>();

        foreach (var actionDto in dto.Actions)
        {
            if (!groups.ContainsKey(actionDto.OwnerId))
                groups[actionDto.OwnerId] = (new(), new(), new(), new());

            var action = new Action(actionDto.Description, ParseActionType(actionDto.ActionType));
            var (entry, doAct, exit, trans) = groups[actionDto.OwnerId];

            switch (actionDto.ActionType)
            {
                case "ENTRY_ACTION": entry.Add(action); break;
                case "DO_ACTION": doAct.Add(action); break;
                case "EXIT_ACTION": exit.Add(action); break;
                case "TRANSITION_ACTION": trans.Add(action); break;
            }
        }

        return groups;
    }

    private Dictionary<string, State> BuildStates(FSMDTO dto,
        Dictionary<string, (List<Action> Entry, List<Action> Do, List<Action> Exit, List<Action> Transition)> actions)
    {
        var states = new Dictionary<string, State>();

        foreach (var stateDto in dto.States)
        {
            actions.TryGetValue(stateDto.Id, out var acts);
            var entry = acts.Entry ?? new();
            var doAct = acts.Do ?? new();
            var exit = acts.Exit ?? new();

            State state = stateDto.StateType switch
            {
                "INITIAL" => _director.MakeInitialState(new InitialStateBuilder(), stateDto.Id, stateDto.Name),
                "FINAL" => _director.MakeFinalState(new FinalStateBuilder(), stateDto.Id, stateDto.Name),
                "SIMPLE" => _director.MakeSimpleState(new SimpleStateBuilder(), stateDto.Id, stateDto.Name, entry, doAct, exit),
                "COMPOUND" => _director.MakeCompoundState(new CompoundStateBuilder(), stateDto.Id, stateDto.Name, entry, doAct, exit),
                _ => throw new InvalidOperationException($"Unknown state type: {stateDto.StateType}")
            };

            states[stateDto.Id] = state;
        }

        return states;
    }

    private static void SetParentChildRelationships(FSMDTO dto, Dictionary<string, State> states)
    {
        foreach (var stateDto in dto.States.Where(s => s.ParentId != "_"))
        {
            if (!states.TryGetValue(stateDto.ParentId, out var parent))
                throw new InvalidOperationException($"Parent state '{stateDto.ParentId}' not found for '{stateDto.Id}'.");

            if (parent is not CompoundState compound)
                throw new InvalidOperationException($"State '{stateDto.ParentId}' is not a COMPOUND state.");

            compound.AddChild(states[stateDto.Id]);
        }
    }

    private List<Transition> BuildTransitions(FSMDTO dto, Dictionary<string, State> states,
        Dictionary<string, Trigger> triggers,
        Dictionary<string, (List<Action> Entry, List<Action> Do, List<Action> Exit, List<Action> Transition)> actions)
    {
        var transitions = new List<Transition>();

        foreach (var transDto in dto.Transitions)
        {
            if (!states.TryGetValue(transDto.SourceId, out var source))
                throw new InvalidOperationException($"Source state '{transDto.SourceId}' not found.");
            if (!states.TryGetValue(transDto.DestinationId, out var destination))
                throw new InvalidOperationException($"Destination state '{transDto.DestinationId}' not found.");

            Trigger? trigger = transDto.TriggerId is not null
                ? triggers.GetValueOrDefault(transDto.TriggerId)
                : null;

            actions.TryGetValue(transDto.Id, out var transActs);
            var effects = transActs.Transition ?? new();

            var transition = _director.MakeTransition(
                new TransitionBuilder(), transDto.Id,
                source, destination, trigger, transDto.Guard, effects);

            transitions.Add(transition);
        }

        return transitions;
    }

    private static ActionType ParseActionType(string raw) => raw switch
    {
        "ENTRY_ACTION" => ActionType.EntryAction,
        "DO_ACTION" => ActionType.DoAction,
        "EXIT_ACTION" => ActionType.ExitAction,
        "TRANSITION_ACTION" => ActionType.TransitionAction,
        _ => throw new InvalidOperationException($"Unknown action type: {raw}")
    };
}
