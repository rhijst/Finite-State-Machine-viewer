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
    private const int StateHeight = 80;
    private const int Padding = 30;
    private const int ArrowHead = 10;

    private readonly Dictionary<State, Rectangle> _stateBounds = new();
    private Bitmap? _bitmap;
    private Graphics? _g;
    private int _currentY;

    public GraphicalVisitor(string outputPath)
    {
        _outputPath = outputPath;
    }

    public void BeginFsm(FiniteStateMachine fsm)
    {
        _fsm = fsm;
        _stateBounds.Clear();
        _currentY = Padding;

        int totalStates = fsm.States.Count;
        int canvasWidth = 900;
        int canvasHeight = Math.Max(600, totalStates * (StateHeight + Padding) + 100);

        _bitmap = new Bitmap(canvasWidth, canvasHeight);
        _g = Graphics.FromImage(_bitmap);
        _g.SmoothingMode = SmoothingMode.AntiAlias;
        _g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        _g.Clear(Color.White);

        using var titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
        _g.DrawString($"FSM: {fsm.Name}", titleFont, Brushes.DarkBlue, Padding, 10);
        _currentY = 50;
    }

    public void EndFsm(FiniteStateMachine fsm)
    {
        if (_g is null || _bitmap is null) return;

        DrawTransitions(fsm);

        _g.Dispose();
        _bitmap.Save(_outputPath, System.Drawing.Imaging.ImageFormat.Png);
        _bitmap.Dispose();
    }

    public void Visit(InitialState state)
    {
        if (_g is null) return;
        int x = Padding;
        int y = _currentY;
        var bounds = new Rectangle(x, y, StateWidth, StateHeight);
        _stateBounds[state] = bounds;
        _currentY += StateHeight + Padding;

        _g.FillEllipse(Brushes.Black, x + StateWidth / 2 - 12, y + StateHeight / 2 - 12, 24, 24);
        using var font = new Font("Segoe UI", 9);
        _g.DrawString(state.Name, font, Brushes.Gray, x + StateWidth + 5, y + StateHeight / 2 - 7);
    }

    public void Visit(FinalState state)
    {
        if (_g is null) return;
        int x = Padding;
        int y = _currentY;
        var bounds = new Rectangle(x, y, StateWidth, StateHeight);
        _stateBounds[state] = bounds;
        _currentY += StateHeight + Padding;

        int cx = x + StateWidth / 2, cy = y + StateHeight / 2;
        _g.FillEllipse(Brushes.Black, cx - 12, cy - 12, 24, 24);
        using var pen = new Pen(Color.Black, 2);
        _g.DrawEllipse(pen, cx - 18, cy - 18, 36, 36);
        using var font = new Font("Segoe UI", 9);
        _g.DrawString(state.Name, font, Brushes.Gray, x + StateWidth + 5, cy - 7);
    }

    public void Visit(SimpleState state)
    {
        if (_g is null) return;
        int x = Padding;
        int y = _currentY;

        var actionsText = BuildActionsText(state);
        int extraHeight = actionsText.Count * 16;
        int height = StateHeight + extraHeight;

        var bounds = new Rectangle(x, y, StateWidth, height);
        _stateBounds[state] = bounds;
        _currentY += height + Padding;

        using var pen = new Pen(Color.SteelBlue, 2);
        _g.FillRectangle(new SolidBrush(Color.FromArgb(230, 240, 255)), bounds);
        _g.DrawRectangle(pen, bounds);

        using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var actionFont = new Font("Segoe UI", 8);
        _g.DrawString(state.Name, nameFont, Brushes.DarkBlue, x + 8, y + 8);

        int textY = y + 28;
        foreach (var line in actionsText)
        {
            _g.DrawString(line, actionFont, Brushes.DarkSlateGray, x + 8, textY);
            textY += 16;
        }
    }

    public void Visit(CompoundState state)
    {
        if (_g is null || _fsm is null) return;

        int x = Padding;
        int startY = _currentY;

        _currentY += StateHeight;
        int innerX = x + 20;

        foreach (var child in state.Children)
        {
            child.Accept(this);
        }

        int endY = _currentY;
        int height = endY - startY + Padding;

        var bounds = new Rectangle(x, startY, StateWidth + 40, height);
        _stateBounds[state] = bounds;

        using var pen = new Pen(Color.DarkOrange, 2) { DashStyle = DashStyle.Dash };
        _g.DrawRectangle(pen, bounds);

        using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic);
        _g.DrawString($"[[ {state.Name} ]]", nameFont, Brushes.DarkOrange, x + 6, startY + 6);

        _currentY = endY + Padding / 2;
    }

    public void Visit(Transition transition) { }

    private void DrawTransitions(FiniteStateMachine fsm)
    {
        if (_g is null) return;

        using var pen = new Pen(Color.DarkGreen, 1.5f);
        using var labelFont = new Font("Segoe UI", 8);
        int offset = 0;

        foreach (var t in fsm.Transitions)
        {
            if (!_stateBounds.TryGetValue(t.Source, out var srcRect)) continue;
            if (!_stateBounds.TryGetValue(t.Destination, out var dstRect)) continue;

            int srcX = srcRect.Right;
            int srcY = srcRect.Top + srcRect.Height / 2;
            int dstX = dstRect.Right + 20 + offset * 12;
            int dstY = dstRect.Top + dstRect.Height / 2;

            _g.DrawLine(pen, srcX, srcY, dstX, srcY);
            _g.DrawLine(pen, dstX, srcY, dstX, dstY);
            DrawArrowHead(_g, pen, dstX, dstY, true);

            string label = t.GetLabel();
            if (!string.IsNullOrEmpty(label))
                _g.DrawString(label, labelFont, Brushes.DarkGreen, dstX + 2, Math.Min(srcY, dstY) + 2);

            offset = (offset + 1) % 5;
        }
    }

    private void DrawArrowHead(Graphics g, Pen pen, int x, int y, bool pointingDown)
    {
        if (pointingDown)
        {
            g.DrawLine(pen, x, y, x - ArrowHead / 2, y - ArrowHead);
            g.DrawLine(pen, x, y, x + ArrowHead / 2, y - ArrowHead);
        }
        else
        {
            g.DrawLine(pen, x, y, x - ArrowHead, y - ArrowHead / 2);
            g.DrawLine(pen, x, y, x - ArrowHead, y + ArrowHead / 2);
        }
    }

    private static List<string> BuildActionsText(State state)
    {
        var lines = new List<string>();
        foreach (var a in state.EntryActions) lines.Add($"entry/ {a.Description}");
        foreach (var a in state.DoActions) lines.Add($"do/ {a.Description}");
        foreach (var a in state.ExitActions) lines.Add($"exit/ {a.Description}");
        return lines;
    }
}
