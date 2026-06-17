namespace FSM.FileHandling.DTO;

public class FSMDTO
{
    public string Name { get; set; } = string.Empty;
    public List<StateDTO> States { get; set; } = new();
    public List<TriggerDTO> Triggers { get; set; } = new();
    public List<ActionDTO> Actions { get; set; } = new();
    public List<TransitionDTO> Transitions { get; set; } = new();
}

public record StateDTO(string Id, string ParentId, string Name, string StateType);

public record TriggerDTO(string Id, string Description);

public record ActionDTO(string OwnerId, string Description, string ActionType);

public record TransitionDTO(string Id, string SourceId, string DestinationId, string? TriggerId, string? Guard);
