namespace RemovedorPerfisWindows.Controls;

public sealed class ThemedButton : Button
{
    private bool _isHovered;
    private bool _isPressed;

    public Color DisabledBackColor { get; set; } = SystemColors.Control;
    public Color DisabledForeColor { get; set; } = SystemColors.GrayText;
    public Color DisabledBorderColor { get; set; } = SystemColors.ControlDark;
    public Color ButtonBorderColor { get; set; } = SystemColors.ControlDark;
    public Color HoverBackColor { get; set; } = SystemColors.ControlLight;
    public Color PressedBackColor { get; set; } = SystemColors.ControlDark;

    public ThemedButton()
    {
        SetStyle(
            ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);

        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _isPressed = e.Button == MouseButtons.Left;
        base.OnMouseDown(e);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        base.OnMouseUp(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var backColor = GetCurrentBackColor();
        var foreColor = Enabled ? ForeColor : DisabledForeColor;
        var borderColor = Enabled ? ButtonBorderColor : DisabledBorderColor;

        using var backBrush = new SolidBrush(backColor);
        using var borderPen = new Pen(borderColor);

        pevent.Graphics.FillRectangle(backBrush, ClientRectangle);
        pevent.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        var flags = TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.EndEllipsis;

        TextRenderer.DrawText(pevent.Graphics, Text, Font, ClientRectangle, foreColor, flags);

        if (Focused && ShowFocusCues)
        {
            var focusRectangle = new Rectangle(3, 3, Width - 7, Height - 7);
            ControlPaint.DrawFocusRectangle(pevent.Graphics, focusRectangle, foreColor, backColor);
        }
    }

    private Color GetCurrentBackColor()
    {
        if (!Enabled)
        {
            return DisabledBackColor;
        }

        if (_isPressed)
        {
            return PressedBackColor;
        }

        return _isHovered ? HoverBackColor : BackColor;
    }
}
