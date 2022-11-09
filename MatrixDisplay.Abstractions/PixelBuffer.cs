namespace MatrixDisplay;

using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;

public sealed class PixelBuffer
{
    public const string DefaultMemoryMappedFileName = "md-mmf1";

    private const uint CommitFlag = 1 << 0;

    private static readonly string[] _allowedPictureFormats = new string[] { ".png", ".jpg",".jpeg",".bmp", };
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

        Thread.Sleep(1000); // TODO
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
        PerformCommit();
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
            Span<Color> span = new Span<Color>(
                pointer: (void*)bitmapData.Scan0,
                length: Width * Height);

            span.CopyTo(Data);
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
            .Select(x => $"{name}{x}")
            .FirstOrDefault(File.Exists);

        if (file is null)
        {
            throw new FileNotFoundException(name);
        }

        using var bitmap = new Bitmap(file);
        DrawImage(bitmap);
    }

    public void Clear()
    {
        Data.Clear();
        MarkDirty();
    }
}
