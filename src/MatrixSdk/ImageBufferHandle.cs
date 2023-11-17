namespace MatrixSdk;

using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

public sealed class ImageBufferHandle : IDisposable
{
    [SupportedOSPlatform("windows")]
    private readonly MemoryMappedFile? _memoryMappedFile;

    [SupportedOSPlatform("windows")]
    private readonly MemoryMappedViewAccessor? _memoryMappedViewAccessor;

    private ulong _version;
    private nint _buffer;

    public unsafe ImageBufferHandle(ControllerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var bounds = new ImageBounds(options.Width, options.Height);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(bounds.Width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(bounds.Height, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bounds.Width, 4096);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bounds.Height, 4096);

        var size = bounds.Width * bounds.Height * 4;

        if (OperatingSystem.IsWindows())
        {
            size += 8;

            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(
                mapName: $"matrix-sdk-{options.Host.Replace('.', '-')}-{options.Port}",
                capacity: size,
                access: MemoryMappedFileAccess.ReadWrite,
                options: MemoryMappedFileOptions.None,
                inheritability: HandleInheritability.Inheritable);

            _memoryMappedViewAccessor = _memoryMappedFile.CreateViewAccessor(
                offset: 0,
                size: size,
                access: MemoryMappedFileAccess.ReadWrite);

            _buffer = _memoryMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle() + 8;
        }
        else
        {
            _buffer = (nint)NativeMemory.AlignedAlloc(
                byteCount: (nuint)size,
                alignment: 32);
        }

        Bounds = bounds;
        Count = bounds.Width * bounds.Height;
    }

    public nint Buffer
    {
        get
        {
            var buffer = _buffer;
            ObjectDisposedException.ThrowIf(buffer is 0, this);
            return _buffer;
        }
    }

    public ImageBounds Bounds { get; }

    public int Count { get; }

    public unsafe void IncrementVersion()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        BinaryPrimitives.WriteUInt64LittleEndian(
            destination: new Span<byte>((void*)_memoryMappedViewAccessor!.SafeMemoryMappedViewHandle.DangerousGetHandle(), 8),
            value: Interlocked.Increment(ref _version));
    }

    [SupportedOSPlatform("windows")]
    public unsafe ulong Version
    {
        get
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(
                pointer: (void*)_memoryMappedViewAccessor!.SafeMemoryMappedViewHandle.DangerousGetHandle(),
                length: 8));
        }
    }

    public unsafe void Dispose()
    {
        var buffer = Interlocked.Exchange(ref _buffer, 0);

        if (buffer is not 0)
        {
            if (OperatingSystem.IsWindows())
            {
                Debug.Assert(buffer == _memoryMappedFile!.SafeMemoryMappedFileHandle.DangerousGetHandle() + 8);

                _memoryMappedFile.Dispose();
                _memoryMappedViewAccessor!.Dispose();
            }
            else
            {
                NativeMemory.AlignedFree((void*)buffer);
            }
        }
    }
}
