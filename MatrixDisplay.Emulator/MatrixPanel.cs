namespace MatrixDisplay.Emulator;

using System;

using SystemDrawingColor = System.Drawing.Color;

internal sealed class MatrixPanel : Panel, IDisplayController
{
    private static readonly SystemDrawingColor _backgroundColor = SystemDrawingColor.FromArgb(48, 48, 48);

    private bool _resized;
    private readonly Color[] _buffer;
    private readonly int _width;

    public MatrixPanel(int width, int height)
    {
        _buffer = new Color[width * height];
        _width = width;
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        if (_resized)
        {
            eventArgs.Graphics.Clear(_backgroundColor);
            _resized = false;
        }

        var ratio = Math.Min(
            val1: eventArgs.ClipRectangle.Width,
            val2: eventArgs.ClipRectangle.Height) * (float)(eventArgs.ClipRectangle.Width / eventArgs.ClipRectangle.Height) / _width;

        var width = (eventArgs.ClipRectangle.Width - (_width * ratio)) / 2f;
        var height = (float)eventArgs.ClipRectangle.Height;

        var pointF = new PointF(width, (height - (_buffer.Length / _width * ratio)) / 2f);

        for (var y = 0; y < _buffer.Length / _width; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var color = _buffer[(y * _width) + x];
                using var brush = new SolidBrush(SystemDrawingColor.FromArgb(color.R, color.G, color.B));
                eventArgs.Graphics.FillRectangle(brush, pointF.X + (x * ratio), pointF.Y + (y * ratio), ratio, ratio);
            }
        }
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);

        _resized = true;

        Refresh();
    }

    public void Update(ReadOnlySpan<Color> buffer)
    {
        buffer.CopyTo(_buffer);

        Invoke(Invalidate);
    }
}
