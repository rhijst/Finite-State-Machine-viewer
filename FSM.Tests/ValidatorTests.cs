using FSM.Factory;
using FSM.FileHandling;
using FSM.Validators;

namespace FSM.Tests;

public class ValidatorTests
{
    private static readonly string TestDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFSMs");

    private static FSM.Model.FiniteStateMachine Load(string filename)
    {
        var path = System.IO.Path.Combine(TestDir, filename);
        var handler = new FileHandler(new TextFileReader(), new FileInterpreter(), new FileValidator());
        var dto = handler.ReadAndInterpret(path);
        return new FSMFactory().Create(dto);
    }

    [Fact]
    public void DeterministicValidator_DetectsDuplicateTrigger_Invalid1()
    {
        // Arrange
        var fsm = Load("invalid_deterministic1.fsm");

        // Act
        var result = new DeterministicValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void DeterministicValidator_DetectsSameGuard_Invalid2()
    {
        // Arrange
        var fsm = Load("invalid_deterministic2.fsm");

        // Act
        var result = new DeterministicValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeterministicValidator_DetectsAutomaticTransitionWithOthers_Invalid3()
    {
        // Arrange
        var fsm = Load("invalid_deterministic3.fsm");

        // Act
        var result = new DeterministicValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeterministicValidator_AcceptsDifferentGuards_ValidFsm()
    {
        // Arrange
        var fsm = Load("valid_deterministic.fsm");

        // Act
        var result = new DeterministicValidator().Validate(fsm);

        // Assert
        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void InitialFinalValidator_DetectsIncomingTransitionToInitial()
    {
        // Arrange
        var fsm = Load("invalid_initial.fsm");

        // Act
        var result = new InitialFinalStateValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void InitialFinalValidator_DetectsOutgoingTransitionFromFinal()
    {
        // Arrange
        var fsm = Load("invalid_final.fsm");

        // Act
        var result = new InitialFinalStateValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompoundTransitionValidator_DetectsTransitionToCompoundState()
    {
        // Arrange
        var fsm = Load("invalid_compound.fsm");

        // Act
        var result = new CompoundTransitionValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompoundTransitionValidator_AcceptsTransitionFromCompoundState()
    {
        // Arrange
        // valid_compound.fsm has TRANSITION compound -> final which is FROM compound, not TO
        var fsm = Load("valid_compound.fsm");

        // Act
        var result = new CompoundTransitionValidator().Validate(fsm);

        // Assert
        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void UnreachableStateValidator_DetectsUnreachableState()
    {
        // Arrange
        var fsm = Load("invalid_unreachable.fsm");

        // Act
        var result = new UnreachableStateValidator().Validate(fsm);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void LampFsm_PassesAllValidators()
    {
        // Arrange
        var fsm = Load("example_lamp.fsm");
        var validators = new List<IFSMValidator>
        {
            new DeterministicValidator(),
            new InitialFinalStateValidator(),
            new CompoundTransitionValidator(),
            new UnreachableStateValidator()
        };

        // Act
        var errors = validators.SelectMany(v => v.Validate(fsm).Errors).ToList();

        // Assert
        Assert.Empty(errors);
    }
}
