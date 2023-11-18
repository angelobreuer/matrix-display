namespace Matrix.Tools.Packager;

using System;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Threading;
using MatrixSdk;
using SkiaSharp;

public sealed class PackagePlayout
{
    private readonly byte[] _buffer;
    private readonly string _fileName;
    private readonly GZipStream _stream;
    private readonly int _intervalInMilliseconds;
    private long _lastTickCount;
    private long _delta;

    public PackagePlayout(string fileName, double framesPerSecond = 30, double timeScale = 1D)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        _buffer = GC.AllocateUninitializedArray<byte>(4096);
        _fileName = fileName;

        var fileStream = new FileStream(_fileName, FileMode.Open);
        _stream = new GZipStream(fileStream, CompressionMode.Decompress, leaveOpen: false);

        var interval = TimeSpan.FromSeconds(framesPerSecond / 1000D / timeScale);
        _intervalInMilliseconds = (int)Math.Floor(interval.TotalMilliseconds);

        _lastTickCount = Environment.TickCount64;
    }

    public bool Next(ImageBuffer imageBuffer)
    {
        var spinWait = new SpinWait();
        long framesToAdvance;

        while (true)
        {
            var currentTickCount = Environment.TickCount64;
            _delta += currentTickCount - _lastTickCount;
            _lastTickCount = currentTickCount;

            (framesToAdvance, _delta) = Math.DivRem(_delta, _intervalInMilliseconds);

            if (framesToAdvance is not 0)
            {
                break;
            }

            spinWait.SpinOnce();
        }

        int frameLength;
        do
        {
            if ((frameLength = ReadNext()) is 0)
            {
                return false;
            }
        }
        while (--framesToAdvance > 0);

        using var originalImage = SKBitmap.Decode(_buffer.AsSpan(0, frameLength));

        imageBuffer.DrawImage(originalImage);

        return true;
    }

    private int ReadNext()
    {
        var bytesRead = _stream!.ReadAtLeast(_buffer!.AsSpan(0, 4), 4);

        if (bytesRead is 0)
        {
            return 0;
        }

        var frameLength = BinaryPrimitives.ReadInt32LittleEndian(_buffer);

        bytesRead = _stream!.ReadAtLeast(_buffer.AsSpan(0, frameLength), frameLength);

        if (bytesRead is 0)
        {
            return 0;
        }

        return frameLength;
    }
}
