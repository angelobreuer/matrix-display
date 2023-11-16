using System.Buffers.Binary;
using System.IO.Compression;
using SkiaSharp;

var files = Directory
    .EnumerateFiles(@"staging", "*")
    .OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x)));

using var fileStream = new FileStream("images.pak", FileMode.Create);
using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
var lengthBuffer = GC.AllocateUninitializedArray<byte>(4);

foreach (var file in files)
{
    using var bitmap = SKBitmap.Decode(file);
    using var encodedData = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);

    BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, encodedData.Span.Length);

    gzipStream.Write(lengthBuffer, 0, lengthBuffer.Length);
    gzipStream.Write(encodedData.Span);
}

gzipStream.Flush();
fileStream.Flush();
