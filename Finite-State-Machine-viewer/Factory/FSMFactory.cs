using FSM.Builders;
using FSM.FileHandling.DTO;
using FSM.Model;
using FSM.Model.States;
using System.Collections.Generic;
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

        foreach (ActionDTO actionDto in dto.Actions)
        {
            if (!groups.ContainsKey(actionDto.OwnerId))
                groups[actionDto.OwnerId] = (new(), new(), new(), new());

            Action action = new Action(actionDto.Description, ParseActionType(actionDto.ActionType));
            var (entry, doAct, exit, trans) = groups[actionDto.OwnerId];

            switch (action.Type)
            {
                case ActionType.EntryAction :       entry.Add(action); break;
                case ActionType.DoAction:           doAct.Add(action); break;
                case ActionType.ExitAction:         exit.Add(action); break;
                case ActionType.TransitionAction:   trans.Add(action); break;
                default: throw new InvalidOperationException($"Unknown action type {action.Type}");
            } 
        }

        return groups;
    }

    private Dictionary<string, State> BuildStates(
        FSMDTO dto,
        Dictionary<string,
        (
            List<Action> Entry,
            List<Action> Do,
            List<Action> Exit,
            List<Action> Transition)> actions
        )
    {
        Dictionary<string, State>? states = new Dictionary<string, State>();

        foreach (var stateDto in dto.States)
        {
            actions.TryGetValue(stateDto.Id, out var acts);
            List<Action> entry = acts.Entry ?? new();
            List<Action> doAct = acts.Do ?? new();
            List<Action> exit = acts.Exit ?? new();

            StateType type = ParseStateType(stateDto.StateType);

            State state = type switch
            {
                StateType.Initial =>
                    _director.MakeInitialState(new InitialStateBuilder(), stateDto.Id, stateDto.Name),

                StateType.Final =>
                    _director.MakeFinalState(new FinalStateBuilder(), stateDto.Id, stateDto.Name),

                StateType.Simple =>
                    _director.MakeSimpleState(new SimpleStateBuilder(), stateDto.Id, stateDto.Name, entry, doAct, exit),

                StateType.Compound =>
                    _director.MakeCompoundState(new CompoundStateBuilder(), stateDto.Id, stateDto.Name, entry, doAct, exit),

                _ => throw new InvalidOperationException($"Unknown state type {type}")
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
            List<Action> effects = transActs.Transition ?? new();

            Transition transition = _director.MakeTransition(
                new TransitionBuilder(), transDto.Id,
                source, destination, trigger, transDto.Guard, effects
                );

            transitions.Add(transition);
        }

        return transitions;
    }

    private static ActionType ParseActionType(string raw)
    {
        return raw switch
        {
            "ENTRY_ACTION" => ActionType.EntryAction,
            "DO_ACTION" => ActionType.DoAction,
            "EXIT_ACTION" => ActionType.ExitAction,
            "TRANSITION_ACTION" => ActionType.TransitionAction,
            _ => throw new InvalidOperationException($"Unknown action type: {raw}"),
        };
    }

    private static StateType ParseStateType(string raw)
    {
        return raw switch
        {
            "INITIAL" => StateType.Initial,
            "SIMPLE" => StateType.Simple,
            "COMPOUND" => StateType.Compound,
            "FINAL" => StateType.Final,
            _ => throw new InvalidOperationException($"Unknown state type: {raw}")
        };
    }
}
