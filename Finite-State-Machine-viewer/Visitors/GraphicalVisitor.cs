using System.Drawing;
using System.Drawing.Drawing2D;
using FSM.Model;
using FSM.Model.States;

namespace FSM.Visitors;

public class GraphicalVisitor : IVisitor
{
    private FiniteStateMachine? _fsm;
    private readonly string _outputPath;

    private const int StateWidth = 200;
    private const int StateBaseHeight = 80;

    private const int Padding = 40;
    private const int Columns = 3;

    private const int HorizontalSpacing = 320;
    private const int VerticalSpacing = 180;

    private readonly Dictionary<State, Rectangle> _stateBounds = new();

    private Bitmap? _bitmap;
    private Graphics? _g;

    private int _stateIndex;

    public GraphicalVisitor(string outputPath)
    {
        _outputPath = outputPath;
    }

    // ---------------- FSM ----------------

    public void BeginFsm(FiniteStateMachine fsm)
    {
        _fsm = fsm;
        _stateBounds.Clear();
        _stateIndex = 0;

        int rows = (int)Math.Ceiling(fsm.States.Count / (double)Columns);

        int canvasWidth = Columns * HorizontalSpacing + 2 * Padding;
        int canvasHeight = rows * VerticalSpacing + 200;

        _bitmap = new Bitmap(canvasWidth, canvasHeight);
        _g = Graphics.FromImage(_bitmap);

        _g.SmoothingMode = SmoothingMode.AntiAlias;
        _g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        _g.Clear(Color.White);

        using var titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
        _g.DrawString($"FSM: {fsm.Name}", titleFont, Brushes.DarkBlue, Padding, 10);
    }

    public void EndFsm(FiniteStateMachine fsm)
    {
        if (_g is null || _bitmap is null) return;

        DrawTransitions(fsm);

        _g.Dispose();
        _bitmap.Save(_outputPath, System.Drawing.Imaging.ImageFormat.Png);
        _bitmap.Dispose();
    }

    // ---------------- STATE POSITIONING ----------------

    private Point GetNextPosition()
    {
        int column = _stateIndex % Columns;
        int row = _stateIndex / Columns;

        _stateIndex++;

        return new Point(
            Padding + column * HorizontalSpacing,
            80 + row * VerticalSpacing
        );
    }

    private Rectangle CreateBounds(Point pos, int extraHeight = 0)
    {
        return new Rectangle(
            pos.X,
            pos.Y,
            StateWidth,
            StateBaseHeight + extraHeight
        );
    }

    // ---------------- STATES ----------------

    public void Visit(InitialState state)
    {
        if (_g is null) return;

        var pos = GetNextPosition();
        var bounds = CreateBounds(pos);

        _stateBounds[state] = bounds;

        DrawBasicState(bounds, state.Name, Brushes.Black, isInitial: true);
    }

    public void Visit(FinalState state)
    {
        if (_g is null) return;

        var pos = GetNextPosition();
        var bounds = CreateBounds(pos);

        _stateBounds[state] = bounds;

        DrawBasicState(bounds, state.Name, Brushes.Black, isFinal: true);
    }

    public void Visit(SimpleState state)
    {
        if (_g is null) return;

        var actions = BuildActionsText(state);
        int extraHeight = actions.Count * 16;

        var pos = GetNextPosition();
        var bounds = CreateBounds(pos, extraHeight);

        _stateBounds[state] = bounds;

        using var pen = new Pen(Color.SteelBlue, 2);

        _g.FillRectangle(new SolidBrush(Color.FromArgb(235, 245, 255)), bounds);
        _g.DrawRectangle(pen, bounds);

        using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var actionFont = new Font("Segoe UI", 8);

        _g.DrawString(state.Name, nameFont, Brushes.DarkBlue, bounds.X + 8, bounds.Y + 8);

        int y = bounds.Y + 30;
        foreach (var line in actions)
        {
            _g.DrawString(line, actionFont, Brushes.DarkSlateGray, bounds.X + 8, y);
            y += 16;
        }
    }

    public void Visit(CompoundState state)
    {
        if (_g is null) return;

        int startIndex = _stateIndex;

        foreach (var child in state.Children)
            child.Accept(this);

        int endIndex = _stateIndex;

        // compute bounding box from children
        var childrenRects = _stateBounds
            .Where(kv => state.Children.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToList();

        if (childrenRects.Count == 0) return;

        int minX = childrenRects.Min(r => r.X) - 20;
        int minY = childrenRects.Min(r => r.Y) - 40;
        int maxX = childrenRects.Max(r => r.Right) + 20;
        int maxY = childrenRects.Max(r => r.Bottom) + 20;

        var bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);

        _stateBounds[state] = bounds;

        using var pen = new Pen(Color.DarkOrange, 2)
        {
            DashStyle = DashStyle.Dash
        };

        _g.DrawRectangle(pen, bounds);

        using var font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic);
        _g.DrawString($"[[ {state.Name} ]]", font, Brushes.DarkOrange, bounds.X + 6, bounds.Y + 6);
    }

    public void Visit(Transition transition) { }

    // ---------------- DRAW TRANSITIONS ----------------

    private void DrawTransitions(FiniteStateMachine fsm)
    {
        if (_g is null) return;

        using var pen = new Pen(Color.DarkGreen, 1.5f)
        {
            EndCap = LineCap.ArrowAnchor
        };

        using var font = new Font("Segoe UI", 8);

        foreach (var t in fsm.Transitions)
        {
            if (!_stateBounds.TryGetValue(t.Source, out var src)) continue;
            if (!_stateBounds.TryGetValue(t.Destination, out var dst)) continue;

            Point start = new(
                src.Right,
                src.Top + src.Height / 2
            );

            Point end = new(
                dst.Left,
                dst.Top + dst.Height / 2
            );

            _g.DrawLine(pen, start, end);

            var label = t.GetLabel();
            if (!string.IsNullOrWhiteSpace(label))
            {
                DrawLabel(label, font, start, end);
            }
        }
    }

    private void DrawLabel(string text, Font font, Point start, Point end)
    {
        int x = (start.X + end.X) / 2;
        int y = (start.Y + end.Y) / 2;

        SizeF size = _g!.MeasureString(text, font);

        var bg = new RectangleF(
            x,
            y,
            size.Width + 6,
            size.Height + 4
        );

        _g.FillRectangle(Brushes.White, bg);
        _g.DrawString(text, font, Brushes.DarkGreen, x, y);
    }

    // ---------------- HELPERS ----------------

    private void DrawBasicState(Rectangle bounds, string name, Brush brush, bool isInitial = false, bool isFinal = false)
    {
        using var font = new Font("Segoe UI", 10, FontStyle.Bold);

        _g!.FillEllipse(Brushes.Black,
            bounds.X + 20,
            bounds.Y + 20,
            20,
            20);

        if (isFinal)
        {
            using var pen = new Pen(Color.Black, 2);
            _g.DrawEllipse(pen,
                bounds.X + 14,
                bounds.Y + 14,
                32,
                32);
        }

        _g.DrawString(name, font, Brushes.Gray, bounds.Right + 10, bounds.Y + 20);
    }

    private static List<string> BuildActionsText(State state)
    {
        var lines = new List<string>();

        foreach (var a in state.EntryActions)
            lines.Add($"entry/ {a.Description}");

        foreach (var a in state.DoActions)
            lines.Add($"do/ {a.Description}");

        foreach (var a in state.ExitActions)
            lines.Add($"exit/ {a.Description}");

        return lines;
    }
}