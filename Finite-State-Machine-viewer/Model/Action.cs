namespace FSM.Model;

public class Action
{
    public string Description { get; }
    public ActionType Type { get; }

    public Action(string description, ActionType type)
    {
        Description = description;
        Type = type;
    }

    public override string ToString()
    { 
        return $"{Type}: {Description}"; 
    }
}
