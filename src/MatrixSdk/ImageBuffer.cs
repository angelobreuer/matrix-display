namespace MatrixSdk;

using System.Diagnostics;
using System.Net;
using SkiaSharp;

public sealed class ImageBuffer
{
    private readonly ImageBufferHandle _handle;
    private readonly ImageController _controller;

    public static ImageBuffer Create()
    {
        var endpoint = Environment.GetEnvironmentVariable("MATRIX_SDK_ENDPOINT");
        var bounds = Environment.GetEnvironmentVariable("MATRIX_SDK_BOUNDS");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("MATRIX_SDK_ENDPOINT environment variable is not set.");
        }

        if (string.IsNullOrWhiteSpace(bounds))
        {
            throw new InvalidOperationException("MATRIX_SDK_BOUNDS environment variable is not set.");
        }

        var endPoint = IPEndPoint.Parse(endpoint);

        var width = int.Parse(bounds.Split('x')[0]);
        var height = int.Parse(bounds.Split('x')[1]);

        var imageBounds = new ImageBounds(width, height);
        var controller = new ImageController(endPoint);

        return new ImageBuffer(new ImageBufferHandle(imageBounds), controller);
    }

    public unsafe ImageBuffer(ImageBufferHandle bufferHandle, ImageController controller)
    {
        ArgumentNullException.ThrowIfNull(bufferHandle);
        ArgumentNullException.ThrowIfNull(controller);

        _handle = bufferHandle;
        _controller = controller;

        Rows = new ImageBufferRows(this);
        Columns = new ImageBufferColumns(this);
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
        _controller.Send(Colors, Bounds);
        _handle.IncrementVersion();
    }

    private int ComputeIndex(int x, int y) => (y * _handle.Bounds.Width) + x;

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
