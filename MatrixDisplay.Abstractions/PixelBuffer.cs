namespace MatrixDisplay;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.Runtime.Intrinsics.X86;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public sealed class PixelBuffer
{
    public const string DefaultMemoryMappedFileName = "md-mmf1";

    private const uint CommitFlag = 1 << 0;

    private static readonly string[] _allowedPictureFormats = new string[] { ".png", ".jpg", ".jpeg", ".bmp", };
    private static PixelBuffer? _pixelBuffer;

    private readonly MemoryMappedViewAccessor _memoryMappedViewAccessor;
    private int _dirtyState; // 0 = clean, 1 = dirty

    public static PixelBuffer Instance => _pixelBuffer ??= new(DefaultMemoryMappedFileName);

    public unsafe PixelBuffer(string memoryMappedFileName)
    {
        // Memory Mapped File Layout:
        // - 4 bytes for Width
        // - 4 bytes for Height
        // - 4 bytes for Signaling Flags
        // - (width * height * 3) bytes for pixel data
        ArgumentException.ThrowIfNullOrEmpty(memoryMappedFileName);

        var memoryMappedFile = MemoryMappedFile.OpenExisting(memoryMappedFileName, MemoryMappedFileRights.FullControl);

        // Read Header
        using (var memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, 8))
        {
            var pointer = memoryMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
            var span = new ReadOnlySpan<byte>((void*)pointer, 8);

            Width = BinaryPrimitives.ReadInt32LittleEndian(span[0..4]);
            Height = BinaryPrimitives.ReadInt32LittleEndian(span[4..8]);
        }

        _memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(8, 4 + Width * Height * 3, MemoryMappedFileAccess.ReadWrite);

        Rows = new(this);
        Columns = new(this);

        Data.Clear();
        Commit();
    }

    public int Width { get; }

    public int Height { get; }

    public Color this[int x, int y]
    {
        get
        {
            if (x >= Width || x < 0 || y >= Height || y < 0)
            {
                return default;
            }

            return Data[(y * Width) + x];
        }

        set
        {
            if (x >= Width || x < 0 || y >= Height || y < 0)
            {
                return;
            }

            Data[(y * Width) + x] = value;
            MarkDirty();
        }
    }

    public unsafe Span<Color> Data
    {
        get
        {
            return new Span<Color>((void*)(GetPointer() + 4), Height * Width);
        }
    }

    public PixelBufferRowAccessor Rows;
    public PixelBufferColumnAccessor Columns;

    public void Commit()
    {
        if (Interlocked.Exchange(ref _dirtyState, 0) is 1)
        {
            PerformCommit();
        }
    }

    private unsafe void PerformCommit()
    {
        *(byte*)GetPointer() = 1;
    }

    public void MarkDirty()
    {
        _dirtyState = 1;

        if (AutoCommit)
        {
            PerformCommit();
        }
    }

    public Memory<Color> Save()
    {
        return Data.ToArray();
    }

    public void Fade(byte fade)
    {
        Debug.Assert(Sse2.IsSupported);

        var data = Data;

        for (var index = 0; index < data.Length; index++)
        {
            ref var value = ref data[index];

            value = new Color(
                R: (byte)Math.Max(0, value.R - fade),
                G: (byte)Math.Max(0, value.G - fade),
                B: (byte)Math.Max(0, value.B - fade));
        }

        MarkDirty();
    }

    public void Restore(ReadOnlyMemory<Color> buffer)
    {
        buffer.Span.CopyTo(Data);
        MarkDirty();
    }

    public bool AutoCommit { get; set; } = true;

    private nint GetPointer() => _memoryMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle() + 8; // TODO: protect

    public unsafe void DrawImage(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            rect: new Rectangle(0, 0, Width, Height),
            flags: ImageLockMode.ReadOnly,
            format: PixelFormat.Format24bppRgb);

        try
        {
            var sourcePointer = bitmapData.Scan0;

            for (var yOffset = 0; yOffset < Height; yOffset++)
            {
                var source = new Span<Color>(
                    pointer: (void*)sourcePointer,
                    length: Width);

                var destination = Data[(yOffset * Width)..];

                for (var xOffset = 0; xOffset < Width; xOffset++)
                {
                    var (r, g, b) = source[xOffset];
                    destination[xOffset] = new Color(b, g, r);
                }

                sourcePointer += bitmapData.Stride;
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        MarkDirty();
    }

    public void Picture(string name)
    {
        var file = _allowedPictureFormats
            .Select(x => Path.Combine("data", $"{name}{x}"))
            .FirstOrDefault(File.Exists);

        if (file is null)
        {
            throw new FileNotFoundException(name);
        }

        using var bitmap = new Bitmap(file);
        DrawImage(bitmap);
    }

    public Size MeasureText(Font font, string text)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        using var bitmap = new Bitmap(Width, Height);
        using var graphics = Graphics.FromImage(bitmap);

        var size = graphics.MeasureString(text, font);
        return new Size((int)size.Width, (int)size.Height);
    }

    public void DrawText(Font font, string text, int offset, Color? color = null)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        using var solidBrush = new SolidBrush(color is null
            ? System.Drawing.Color.White
            : System.Drawing.Color.FromArgb(color.Value.R, color.Value.G, color.Value.B));

        using var bitmap = new Bitmap(Width, Height);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

            graphics.DrawString(text, font, solidBrush, new PointF(offset, -3));
        }

        DrawImage(bitmap);
    }

    public void Clear()
    {
        Data.Clear();
        MarkDirty();
    }
}
