using FSM.Factory;
using FSM.FileHandling;
using FSM.Visitors;

namespace FSM.Tests;

public class TextVisitorTests
{
    private static FSM.Model.FiniteStateMachine LoadLamp()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFSMs", "example_lamp.fsm");
        var handler = new FileHandler(new TextFileReader(), new FileInterpreter(), new FileValidator());
        var dto = handler.ReadAndInterpret(path);
        return new FSMFactory().Create(dto);
    }

    [Fact]
    public void TextVisitor_FullRender_ContainsFsmName()
    {
        // Arrange
        var fsm = LoadLamp();
        var sw = new StringWriter();

        // Act
        fsm.Render(new TextVisitor(sw));

        // Assert
        Assert.Contains("example_lamp", sw.ToString());
    }

    [Fact]
    public void TextVisitor_FullRender_ContainsAllStateNames()
    {
        // Arrange
        var fsm = LoadLamp();
        var sw = new StringWriter();

        // Act
        fsm.Render(new TextVisitor(sw));
        var output = sw.ToString();

        // Assert
        Assert.Contains("powered off", output);
        Assert.Contains("Powered up", output);
        Assert.Contains("Lamp is off", output);
        Assert.Contains("Lamp is on", output);
    }

    [Fact]
    public void TextVisitor_FullRender_ContainsTransitionLabels()
    {
        // Arrange
        var fsm = LoadLamp();
        var sw = new StringWriter();

        // Act
        fsm.Render(new TextVisitor(sw));
        var output = sw.ToString();

        // Assert
        Assert.Contains("turn power on", output);
        Assert.Contains("Push switch", output);
    }

    [Fact]
    public void TextVisitor_SingleState_OnlyRendersthatState()
    {
        // Arrange
        var fsm = LoadLamp();
        var sw = new StringWriter();
        var visitor = new TextVisitor(sw);
        visitor.BeginFsm(fsm);
        var onState = fsm.States.First(s => s.Name == "Lamp is on");

        // Act
        onState.Accept(visitor);

        // Assert
        var output = sw.ToString();
        Assert.Contains("Lamp is on", output);
        Assert.DoesNotContain("Powered up", output);
    }

    [Fact]
    public void TextVisitor_SingleTransition_RendersLabelAndDestination()
    {
        // Arrange
        var fsm = LoadLamp();
        var sw = new StringWriter();
        var visitor = new TextVisitor(sw);
        visitor.BeginFsm(fsm);
        var t1 = fsm.Transitions.Single(t => t.Id == "t1");

        // Act
        t1.Accept(visitor);

        // Assert
        var output = sw.ToString();
        Assert.Contains("turn power on", output);
        Assert.Contains("Lamp is off", output);
    }
}
