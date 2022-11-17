using System.Buffers.Binary;
using System.Drawing;
using System.IO.Compression;
using MatrixDisplay;

var buffer = PixelBuffer.Instance;

const int FramesPerSecond = 60;
const double TimeScale = 5D;

var file = args.Length is 0 ? "images.pak" : args[0];
using var fileStream = new FileStream(file, FileMode.Open);
using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

var lengthBuffer = GC.AllocateUninitializedArray<byte>(4);
var interval = TimeSpan.FromSeconds(FramesPerSecond / 1000D / TimeScale);
var intervalInMilliseconds = (int)Math.Floor(interval.TotalMilliseconds);

var lastTickCount = Environment.TickCount64;
var delta = 0L;

var frameBuffer = GC.AllocateUninitializedArray<byte>(16 * 1024);
var preparedFrame = default(Bitmap?);

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

    using var memoryStream = new MemoryStream(frameBuffer, 0, frameLength);
    using var originalImage = new Bitmap(Image.FromStream(memoryStream));

    preparedFrame?.Dispose();
    preparedFrame = originalImage;

    buffer.DrawImage(originalImage);
}