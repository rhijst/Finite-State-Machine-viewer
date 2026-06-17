using FSM.Factory;
using FSM.FileHandling;
using FSM.Model;
using FSM.Validators;
using FSM.Visitors;

namespace FSM;

public class FSMApplication
{
    private readonly FileHandler _fileHandler;
    private readonly FSMFactory _factory;
    private readonly IReadOnlyList<IFSMValidator> _validators;

    public FSMApplication()
    {
        _fileHandler = new FileHandler(new TextFileReader(), new FileInterpreter(), new FileValidator());
        _factory = new FSMFactory();
        _validators = new List<IFSMValidator>
        {
            new DeterministicValidator(),
            new InitialFinalStateValidator(),
            new CompoundTransitionValidator(),
            new UnreachableStateValidator()
        };
    }

    public void Start()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║         FSM Viewer / Simulator       ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Enter FSM file path (or 'quit'): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            ProcessFile(input);

            Console.WriteLine();
        }
    }

    public void ProcessFile(string path, string outputMode = "menu", string pngPath = "output.png")
    {
        var fsm = LoadFsm(path);
        if (fsm is null) return;

        var errors = ValidateFsm(fsm);
        if (errors.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FSM validation errors:");
            foreach (var e in errors) Console.WriteLine($"  ✗ {e}");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("FSM is valid.");
        Console.ResetColor();
        Console.WriteLine();

        switch (outputMode)
        {
            case "text":
                fsm.Render(new TextVisitor(Console.Out));
                return;
            case "png":
                try { fsm.Render(new GraphicalVisitor(pngPath)); Console.WriteLine($"PNG: {pngPath}"); }
                catch (Exception ex) { Console.WriteLine($"PNG error: {ex.Message}"); }
                return;
        }

        ShowMenu(fsm);
    }

    private FiniteStateMachine? LoadFsm(string path)
    {
        try
        {
            var dto = _fileHandler.ReadAndInterpret(path);
            return _factory.Create(dto);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    private List<string> ValidateFsm(FiniteStateMachine fsm)
    {
        var allErrors = new List<string>();
        foreach (var validator in _validators)
        {
            var result = validator.Validate(fsm);
            allErrors.AddRange(result.Errors);
        }
        return allErrors;
    }

    private void ShowMenu(FiniteStateMachine fsm)
    {
        Console.WriteLine("Output options:");
        Console.WriteLine("  1. Full FSM (text)");
        Console.WriteLine("  2. Single state (text)");
        Console.WriteLine("  3. Single transition (text)");
        Console.WriteLine("  4. Full FSM (PNG)");
        Console.Write("Choice: ");
        var choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1":
                fsm.Render(new TextVisitor(Console.Out));
                break;

            case "2":
                Console.Write("State name: ");
                var stateName = Console.ReadLine()?.Trim();
                var state = fsm.States.FirstOrDefault(s =>
                    s.Name.Equals(stateName, StringComparison.OrdinalIgnoreCase));
                if (state is null) { Console.WriteLine("State not found."); break; }
                var tv = new TextVisitor(Console.Out);
                tv.BeginFsm(fsm);
                state.Accept(tv);
                break;

            case "3":
                Console.Write("Transition id: ");
                var tid = Console.ReadLine()?.Trim();
                var trans = fsm.Transitions.FirstOrDefault(t =>
                    t.Id.Equals(tid, StringComparison.OrdinalIgnoreCase));
                if (trans is null) { Console.WriteLine("Transition not found."); break; }
                var tv2 = new TextVisitor(Console.Out);
                tv2.BeginFsm(fsm);
                trans.Accept(tv2);
                break;

            case "4":
                Console.Write("Output PNG path (e.g. output.png): ");
                var pngPath = Console.ReadLine()?.Trim() ?? "output.png";
                try
                {
                    fsm.Render(new GraphicalVisitor(pngPath));
                    Console.WriteLine($"PNG saved to: {pngPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PNG generation failed: {ex.Message}");
                }
                break;

            default:
                Console.WriteLine("Unknown option.");
                break;
        }
    }
}
