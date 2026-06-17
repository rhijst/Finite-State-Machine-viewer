using FSM.Factory;
using FSM.FileHandling;
using FSM.Validators;

namespace FSM.Tests;

public class ValidatorTests
{
    private static readonly string TestDir =
        @"C:\Users\Dannyy\Downloads\Test FSMs V2\Test FSMs";

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
        var fsm = Load("invalid_deterministic1.fsm");
        var result = new DeterministicValidator().Validate(fsm);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void DeterministicValidator_DetectsSameGuard_Invalid2()
    {
        var fsm = Load("invalid_deterministic2.fsm");
        var result = new DeterministicValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeterministicValidator_DetectsAutomaticTransitionWithOthers_Invalid3()
    {
        var fsm = Load("invalid_deterministic3.fsm");
        var result = new DeterministicValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeterministicValidator_AcceptsDifferentGuards_ValidFsm()
    {
        var fsm = Load("valid_deterministic.fsm");
        var result = new DeterministicValidator().Validate(fsm);
        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void InitialFinalValidator_DetectsIncomingTransitionToInitial()
    {
        var fsm = Load("invalid_initial.fsm");
        var result = new InitialFinalStateValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void InitialFinalValidator_DetectsOutgoingTransitionFromFinal()
    {
        var fsm = Load("invalid_final.fsm");
        var result = new InitialFinalStateValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompoundTransitionValidator_DetectsTransitionToCompoundState()
    {
        var fsm = Load("invalid_compound.fsm");
        var result = new CompoundTransitionValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompoundTransitionValidator_AcceptsTransitionFromCompoundState()
    {
        // valid_compound.fsm has TRANSITION compound -> final which is FROM compound, not TO
        var fsm = Load("valid_compound.fsm");
        var result = new CompoundTransitionValidator().Validate(fsm);
        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void UnreachableStateValidator_DetectsUnreachableState()
    {
        var fsm = Load("invalid_unreachable.fsm");
        var result = new UnreachableStateValidator().Validate(fsm);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void LampFsm_PassesAllValidators()
    {
        var fsm = Load("example_lamp.fsm");
        var validators = new List<IFSMValidator>
        {
            new DeterministicValidator(),
            new InitialFinalStateValidator(),
            new CompoundTransitionValidator(),
            new UnreachableStateValidator()
        };

        var errors = validators.SelectMany(v => v.Validate(fsm).Errors).ToList();
        Assert.Empty(errors);
    }
}
