namespace MatrixSdk.Emulator;

using System;
using System.Windows.Forms;
using SystemDrawingColor = System.Drawing.Color;

internal sealed class MatrixPanel : Panel
{
    private static readonly SystemDrawingColor _backgroundColor = SystemDrawingColor.FromArgb(48, 48, 48);

    private readonly Timer _timer;
    private readonly ImageBufferHandle _bufferHandle;
    private ulong _lastVersion;
    private bool _resized;

    public MatrixPanel()
    {
        _timer = new Timer { Enabled = true, Interval = 1, };
        _timer.Tick += (_, _) => Run();

        _bufferHandle = new ImageBufferHandle(ControllerOptions.LoadOptions());
    }

    private void Run()
    {
        var currentVersion = _bufferHandle.Version;

        if (!_resized && _lastVersion == currentVersion)
        {
            return;
        }

        Refresh();

        _lastVersion = currentVersion;
    }

    protected unsafe override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        if (_resized)
        {
            eventArgs.Graphics.Clear(_backgroundColor);
            _resized = false;
        }

        var (width, height) = _bufferHandle.Bounds;
        var colors = new ReadOnlySpan<Color>((void*)_bufferHandle.Buffer, width * height);

        var ratio = Math.Min(
            val1: eventArgs.ClipRectangle.Width,
            val2: eventArgs.ClipRectangle.Height) * (float)(eventArgs.ClipRectangle.Width / eventArgs.ClipRectangle.Height) / width;

        var canvasWidth = (eventArgs.ClipRectangle.Width - (width * ratio)) / 2f;
        var canvasHeight = (float)eventArgs.ClipRectangle.Height;

        var pointF = new PointF(canvasWidth, (canvasHeight - (height * ratio)) / 2f);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = colors[(y * width) + x];
                using var brush = new SolidBrush(SystemDrawingColor.FromArgb(color.R, color.G, color.B));
                eventArgs.Graphics.FillRectangle(brush, pointF.X + (x * ratio), pointF.Y + (y * ratio), ratio, ratio);
            }
        }
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);

        _resized = true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _timer.Dispose();
        }
    }
}