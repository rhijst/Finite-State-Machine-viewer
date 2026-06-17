namespace FSM;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            new FSMApplication().Start();
            return;
        }

        // CLI: <file> [text|png [output.png]]
        var app = new FSMApplication();
        if (args.Length == 1)
        {
            app.ProcessFile(args[0]);
        }
        else
        {
            app.ProcessFile(args[0], args.Length > 1 ? args[1] : "text",
                            args.Length > 2 ? args[2] : "output.png");
        }
    }
}
