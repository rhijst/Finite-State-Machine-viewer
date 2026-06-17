using FSM.Model;
using FSM.Model.States;

namespace FSM.Visitors;

public class TextVisitor : IVisitor
{
    private readonly TextWriter _output;
    private FiniteStateMachine? _fsm;
    private int _indent;

    private const int LineWidth = 70;

    public TextVisitor(TextWriter output)
    {
        _output = output;
    }

    public void BeginFsm(FiniteStateMachine fsm)
    {
        _fsm = fsm;
        _indent = 0;
        WriteSeparator('#');
        _output.WriteLine($"# Diagram: {fsm.Name}");
        WriteSeparator('#');
    }

    public void EndFsm(FiniteStateMachine fsm) { }

    public void Visit(InitialState state)
    {
        _output.WriteLine($"{Pad()}(O) {state.Name}");
        WriteOutgoingTransitions(state);
    }

    public void Visit(FinalState state)
    {
        WriteOutgoingTransitions(state);
        _output.WriteLine($"{Pad()}[O] {state.Name}");
    }

    public void Visit(SimpleState state)
    {
        WriteSeparator('-');
        _output.WriteLine($"{Pad()}| {state.Name}");
        WriteSeparator('-');
        WriteActions(state);
        WriteOutgoingTransitions(state);
    }

    public void Visit(CompoundState state)
    {
        WriteSeparator('=');
        _output.WriteLine($"{Pad()}|| Compound state: {state.Name}");
        WriteSeparator('-');
        WriteActions(state);

        _indent += 2;
        foreach (var child in state.Children)
            child.Accept(this);
        _indent -= 2;

        WriteSeparator('=');
        WriteOutgoingTransitions(state);
    }

    public void Visit(Transition transition)
    {
        var label = transition.GetLabel();
        var dest = transition.Destination.Name;
        _output.WriteLine($"{Pad()}---{label}---> {dest}");
    }

    private void WriteActions(State state)
    {
        foreach (var a in state.EntryActions)
            _output.WriteLine($"{Pad()}| On Entry / {a.Description}");
        foreach (var a in state.DoActions)
            _output.WriteLine($"{Pad()}| Do / {a.Description}");
        foreach (var a in state.ExitActions)
            _output.WriteLine($"{Pad()}| On Exit / {a.Description}");
        if (state.EntryActions.Any() || state.DoActions.Any() || state.ExitActions.Any())
            WriteSeparator('-');
    }

    private void WriteOutgoingTransitions(State state)
    {
        if (_fsm is null) return;
        foreach (var t in _fsm.GetOutgoingTransitions(state))
            Visit(t);
    }

    private void WriteSeparator(char ch) =>
        _output.WriteLine(Pad() + new string(ch, LineWidth - _indent * 2));

    private string Pad() => new(' ', _indent * 2);
}
