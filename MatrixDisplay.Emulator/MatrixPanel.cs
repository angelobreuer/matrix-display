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

        var num = Math.Min(eventArgs.ClipRectangle.Width, eventArgs.ClipRectangle.Height) * (float)(eventArgs.ClipRectangle.Width / eventArgs.ClipRectangle.Height) / 13f;
        var x = (eventArgs.ClipRectangle.Width - (_width * num)) / 2f;
        float num2 = eventArgs.ClipRectangle.Height;

        var pointF = new PointF(x, (num2 - (_buffer.Length / _width * num)) / 2f);
        var num3 = 0;

        while (true)
        {
            var num4 = num3;

            if (num4 >= _buffer.Length / _width)
            {
                break;
            }

            for (var i = 0; i < _width; i++)
            {
                var color = _buffer[(num3 * _width) + i];
                using var brush = new SolidBrush(SystemDrawingColor.FromArgb(color.R, color.G, color.B));
                eventArgs.Graphics.FillRectangle(brush, pointF.X + (i * num), pointF.Y + (num3 * num), num, num);
            }

            num3++;
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

        Invoke(() => Invalidate());
    }
}
