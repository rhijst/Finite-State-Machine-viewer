namespace FSM.Model;

public class Trigger
{
    public string Id { get; }
    public string Description { get; }

    public Trigger(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public override string ToString()
    {
        return Description;
    }
}
