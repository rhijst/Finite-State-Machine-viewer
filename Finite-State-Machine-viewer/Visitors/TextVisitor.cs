using FSM.Model;
using FSM.Model.States;

namespace FSM.Visitors;

public class TextVisitor : IVisitor
{
    private readonly TextWriter _output;
    private FiniteStateMachine? _fsm;
    private int _indent;

    private const int SeparatorWidth = 70;

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
        _output.WriteLine();
        _indent = 1;
    }

    public void EndFsm(FiniteStateMachine fsm) { }

    public void Visit(InitialState state)
    {
        _output.WriteLine($"{Pad()}O Initial state ({state.Name})");
        _output.WriteLine();
        WriteTransitions(state, trailingBlankLine: true);
    }

    public void Visit(FinalState state)
    {
        _output.WriteLine();
        _output.WriteLine($"{Pad()}(O) Final state ({state.Name})");
    }

    public void Visit(SimpleState state)
    {
        WriteSeparator('-');
        _output.WriteLine($"{Pad()}| {state.Name}");
        WriteSeparator('-');
        WriteActions(state);
        WriteTransitions(state, trailingBlankLine: true);
    }

    public void Visit(CompoundState state)
    {
        WriteSeparator('=');
        _output.WriteLine($"{Pad()}|| Compound state: {state.Name}");
        WriteSeparator('-');
        WriteActions(state);
        _output.WriteLine();

        _indent++;
        foreach (var child in state.Children)
            child.Accept(this);
        _indent--;

        WriteSeparator('=');
        // Transitions from compound have no trailing blank line (Final state follows directly)
        WriteTransitions(state, trailingBlankLine: false);
    }

    public void Visit(Transition transition)
    {
        _output.WriteLine($"{Pad()}---{transition.GetLabel()}---> {transition.Destination.Name}");
    }

    private void WriteTransitions(State state, bool trailingBlankLine)
    {
        if (_fsm is null) return;
        foreach (var t in _fsm.GetOutgoingTransitions(state))
        {
            Visit(t);
            if (trailingBlankLine) _output.WriteLine();
        }
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

    private void WriteSeparator(char ch) =>
        _output.WriteLine(Pad() + new string(ch, SeparatorWidth));

    private string Pad() => new(' ', _indent * 2);
}
