using FSM.FileHandling;
using FSM.FileHandling.DTO;
using FSM.Model;

namespace FSM.Tests;

public class FileInterpreterTests
{
    private static FSMDTO Interpret(string content)
    {
        var file = new CustomFile(content, "test.fsm", "fsm");
        return new FileInterpreter().Interpret(file);
    }

    [Fact]
    public void Interpret_LampFsm_ParsesCorrectStateCount()
    {
        // Arrange
        var content = @"
STATE initial _ ""powered off"" : INITIAL;
STATE powered _ ""Powered up"" : COMPOUND;
STATE off powered ""Lamp is off"" : SIMPLE;
STATE on powered ""Lamp is on"" : SIMPLE;
STATE final _ ""powered off"" : FINAL;
TRIGGER power_on ""turn power on"";
TRANSITION t1 initial -> off power_on """";
TRANSITION t4 powered -> final power_off """";
";

        // Act
        var dto = Interpret(content);

        // Assert
        Assert.Equal(5, dto.States.Count);
        Assert.Single(dto.Triggers);
        Assert.Equal(2, dto.Transitions.Count);
    }

    [Fact]
    public void Interpret_State_SetsParentCorrectly()
    {
        // Arrange
        var content = @"
STATE initial _ """" : INITIAL;
STATE compound _ ""Compound"" : COMPOUND;
STATE child compound ""Child"" : SIMPLE;
STATE final _ """" : FINAL;
TRANSITION t1 initial -> child """";
TRANSITION t2 child -> final """";
";

        // Act
        var dto = Interpret(content);

        // Assert
        var child = dto.States.First(s => s.Id == "child");
        Assert.Equal("compound", child.ParentId);
    }

    [Fact]
    public void Interpret_Transition_ParsesTriggerAndGuard()
    {
        // Arrange
        var content = @"
STATE s1 _ ""S1"" : SIMPLE;
STATE s2 _ ""S2"" : SIMPLE;
TRIGGER btn ""button"";
TRANSITION t1 s1 -> s2 btn ""x > 5"";
";

        // Act
        var dto = Interpret(content);

        // Assert
        var t = dto.Transitions.Single();
        Assert.Equal("btn", t.TriggerId);
        Assert.Equal("x > 5", t.Guard);
    }

    [Fact]
    public void Interpret_Transition_WithNoTriggerAndGuard_ParsesGuardOnly()
    {
        // Arrange
        var content = @"
STATE s1 _ ""S1"" : SIMPLE;
STATE s2 _ ""S2"" : SIMPLE;
TRANSITION t1 s1 -> s2 ""count >= 3"";
";

        // Act
        var dto = Interpret(content);

        // Assert
        var t = dto.Transitions.Single();
        Assert.Null(t.TriggerId);
        Assert.Equal("count >= 3", t.Guard);
    }

    [Fact]
    public void Interpret_Actions_AreGroupedByOwner()
    {
        // Arrange
        var content = @"
STATE on _ ""on"" : SIMPLE;
ACTION on ""Turn lamp on"" : ENTRY_ACTION;
ACTION on ""Turn lamp off"" : EXIT_ACTION;
TRANSITION t1 on -> on """";
";

        // Act
        var dto = Interpret(content);

        // Assert
        var actions = dto.Actions.Where(a => a.OwnerId == "on").ToList();
        Assert.Equal(2, actions.Count);
        Assert.Contains(actions, a => a.ActionType == ActionType.EntryAction);
        Assert.Contains(actions, a => a.ActionType == ActionType.ExitAction);
    }
}
