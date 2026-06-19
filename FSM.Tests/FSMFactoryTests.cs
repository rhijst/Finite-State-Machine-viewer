using FSM.Factory;
using FSM.FileHandling;
using FSM.Model.States;

namespace FSM.Tests;

public class FSMFactoryTests
{
    private static readonly string TestDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFSMs");

    private static readonly string LampFsmPath =
        Path.Combine(TestDir, "example_lamp.fsm");

    private static readonly string UserAccountPath =
        Path.Combine(TestDir, "example_user_account.fsm");

    private static FSM.Model.FiniteStateMachine LoadFromFile(string path)
    {
        var handler = new FileHandler(new TextFileReader(), new FileInterpreter(), new FileValidator());
        var dto = handler.ReadAndInterpret(path);
        return new FSMFactory().Create(dto);
    }

    [Fact]
    public void Create_LampFsm_BuildsCompoundStateWithChildren()
    {
        // Arrange
        var fsm = LoadFromFile(LampFsmPath);

        // Act
        var compound = fsm.States.OfType<CompoundState>().Single();

        // Assert
        Assert.Equal("Powered up", compound.Name);
        Assert.Equal(2, compound.Children.Count);
    }

    [Fact]
    public void Create_LampFsm_SetsTransitionEffects()
    {
        // Arrange
        var fsm = LoadFromFile(LampFsmPath);

        // Act
        var t2 = fsm.Transitions.Single(t => t.Id == "t2");

        // Assert
        Assert.Single(t2.Effects);
        Assert.Equal("reset off timer", t2.Effects[0].Description);
    }

    [Fact]
    public void Create_LampFsm_SetsEntryAndExitActionsOnSimpleState()
    {
        // Arrange
        var fsm = LoadFromFile(LampFsmPath);

        // Act
        var onState = fsm.States.OfType<SimpleState>().Single(s => s.Name == "Lamp is on");

        // Assert
        Assert.Single(onState.EntryActions);
        Assert.Single(onState.ExitActions);
    }

    [Fact]
    public void Create_UserAccount_BuildsNestedCompoundStates()
    {
        // Arrange
        var fsm = LoadFromFile(UserAccountPath);

        // Act
        var created = fsm.States.OfType<CompoundState>().Single(s => s.Name == "Created");
        var inactive = created.Children.OfType<CompoundState>().SingleOrDefault(s => s.Name == "Inactive");

        // Assert
        Assert.NotNull(inactive);
    }

    [Fact]
    public void Create_UserAccount_TopLevelStatesDoNotIncludeNested()
    {
        // Arrange
        var fsm = LoadFromFile(UserAccountPath);

        // Act
        var topLevel = fsm.GetTopLevelStates().ToList();

        // Assert
        Assert.DoesNotContain(topLevel, s => s.Name == "Unverified");
        Assert.DoesNotContain(topLevel, s => s.Name == "Verified");
    }
}
