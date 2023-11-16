namespace Matrix.Tools.Packager;

using System;
using System.Buffers.Binary;
using System.IO.Compression;
using MatrixSdk;
using SkiaSharp;

public readonly record struct PackagePlayout
{
    private readonly string _fileName;

    public PackagePlayout(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        _fileName = fileName;
    }

    public void Run(ImageBuffer imageBuffer, double framesPerSecond, double timeScale)
    {
        using var fileStream = new FileStream(_fileName, FileMode.Open);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        var lengthBuffer = GC.AllocateUninitializedArray<byte>(4);
        var interval = TimeSpan.FromSeconds(framesPerSecond / 1000D / timeScale);
        var intervalInMilliseconds = (int)Math.Floor(interval.TotalMilliseconds);

        var lastTickCount = Environment.TickCount64;
        var delta = 0L;

        var frameBuffer = GC.AllocateUninitializedArray<byte>(16 * 1024);
        var preparedFrame = default(SKBitmap?);

        int ReadNext()
        {
            var bytesRead = gzipStream!.ReadAtLeast(lengthBuffer!, 4, throwOnEndOfStream: false);

            if (bytesRead is < 4)
            {
                return 0;
            }

            var frameLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
            gzipStream!.ReadExactly(frameBuffer.AsSpan(0, frameLength));
            return frameLength;
        }

        var spinWait = new SpinWait();

        while (true)
        {
            var currentTickCount = Environment.TickCount64;
            delta += currentTickCount - lastTickCount;
            lastTickCount = currentTickCount;

            var (framesToAdvance, newDelta) = Math.DivRem(delta, intervalInMilliseconds);
            delta = newDelta;

            if (framesToAdvance is 0)
            {
                spinWait.SpinOnce();
                continue;
            }

            spinWait = new SpinWait();

            int frameLength;
            do
            {
                if ((frameLength = ReadNext()) is 0)
                {
                    return;
                }
            }
            while (--framesToAdvance > 0);

            using var originalImage = SKBitmap.Decode(frameBuffer.AsSpan(0, frameLength));

            preparedFrame?.Dispose();
            preparedFrame = originalImage;

            imageBuffer.DrawImage(originalImage);
            imageBuffer.Commit();
        }
    }
}
