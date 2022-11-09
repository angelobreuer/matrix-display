namespace MatrixDisplay;

using System;
using System.Buffers.Binary;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;

public unsafe sealed class PixelBufferAccessor
{
    private readonly nint _pointer;
    private readonly MemoryMappedFile _memoryMappedFile;
    private readonly MemoryMappedViewAccessor _memoryMappedViewAccessor;
    private readonly int _width;
    private readonly int _height;

    public PixelBufferAccessor(string memoryMappedFileName, int width, int height)
    {
        var size = 4 + 4 + 4 + (width * height *3);

        _width = width;
        _height = height;

        _memoryMappedFile = MemoryMappedFile.CreateOrOpen(
            mapName: memoryMappedFileName,
            capacity: size,
            access: MemoryMappedFileAccess.ReadWrite,
            options: MemoryMappedFileOptions.None,
            inheritability: HandleInheritability.Inheritable);

        _memoryMappedViewAccessor = _memoryMappedFile.CreateViewAccessor(0, size);
        _pointer = _memoryMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();

        // Write width and height
        var span = new Span<byte>((void*)_pointer, 8);
        BinaryPrimitives.WriteInt32LittleEndian(span[0..4], width);
        BinaryPrimitives.WriteInt32LittleEndian(span[4..8], height);
    }

    public Span<Color> Colors => new((void*)(_pointer + 12), _width * _height);

    public bool TryGetNextFrame()
    {
        // FIX: This is not endian portable
        ref var ptr = ref Unsafe.AsRef<int>((void*)(_pointer + 8));
        return Interlocked.CompareExchange(ref ptr, 0, 1) is not 0;
    }
}
