namespace MatrixSdk;

using System.Diagnostics;
using SkiaSharp;

public sealed class ImageBuffer
{
    private readonly ImageBufferHandle _handle;
    private readonly ImageController _controller;

    public static ImageBuffer Create()
    {
        var options = ControllerOptions.LoadOptions();
        var controller = new ImageController(options);

        return new ImageBuffer(new ImageBufferHandle(options), controller);
    }

    public unsafe ImageBuffer(ImageBufferHandle bufferHandle, ImageController controller)
    {
        ArgumentNullException.ThrowIfNull(bufferHandle);
        ArgumentNullException.ThrowIfNull(controller);

        _handle = bufferHandle;
        _controller = controller;

        Rows = new ImageBufferRows(this);
        Columns = new ImageBufferColumns(this);

        Colors.Clear();
        Commit();
    }

    public unsafe Span<Color> Colors => new((void*)_handle.Buffer, _handle.Count);

    public Color this[int X, int Y]
    {
        get => X >= 0 && X < Width && Y >= 0 && Y < Height
            ? Colors[ComputeIndex(X, Y)]
            : default;

        set
        {
            if (X < 0 || X >= Width || Y < 0 || Y >= Height)
            {
                return;
            }

            Colors[ComputeIndex(X, Y)] = value;
        }
    }

    public void Fade(int count)
    {
        foreach (ref var color in Colors)
        {
            color = color with
            {
                R = (byte)Math.Max(0, color.R - count),
                G = (byte)Math.Max(0, color.G - count),
                B = (byte)Math.Max(0, color.B - count),
            };
        }
    }

    public int Width => Bounds.Width;

    public int Height => Bounds.Height;

    public ImageBounds Bounds => _handle.Bounds;

    public ImageBufferColumns Columns;

    public ImageBufferRows Rows;

    public void Clear() => Colors.Clear();

    public void Commit()
    {
        for (int index = 0; index < 20; index++)
        {
            _controller.Send(Colors, Bounds);
            _handle.IncrementVersion();
        }
    }

    private int ComputeIndex(int x, int y) => (y * _handle.Bounds.Width) + x;

    public ReadOnlyMemory<Color> Capture() => Colors.ToArray();

    public void Restore(ReadOnlySpan<Color> colors) => colors.CopyTo(Colors);

    public void Invert()
    {
        foreach (ref var color in Colors)
        {
            color = color with
            {
                R = (byte)(255 - color.R),
                G = (byte)(255 - color.G),
                B = (byte)(255 - color.B),
            };
        }
    }

    public void Threshold(byte value)
    {
        foreach (ref var color in Colors)
        {
            var thresholdValue = (byte)((color.R + color.G + color.B) / 3);

            if (thresholdValue < value)
            {
                color = default;
            }
        }
    }

    public void DrawImage(SKBitmap image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var needsScaling = image.Width == Bounds.Width && image.Height == Bounds.Height;

        using var scaledBitmap = needsScaling ? new SKBitmap(Width, Height) : null;
        var bitmap = needsScaling ? scaledBitmap! : image;

        if (needsScaling)
        {
            image.ScalePixels(bitmap, SKFilterQuality.High);
        }

        var colors = Colors;
        Debug.Assert(bitmap.Pixels.Length == colors.Length);

        for (var index = 0; index < colors.Length; index++)
        {
            var color = bitmap.Pixels[index];
            colors[index] = new Color(color.Red, color.Green, color.Blue);
        }
    }
}
