using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;

var files = Directory
    .EnumerateFiles(@"C:\Users\angel\Downloads\Weihnachtsmann (1)\Weihnachs", "*.bmp")
    .OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x)));

using var fileStream = new FileStream("images.pak", FileMode.Create);
using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
var lengthBuffer = GC.AllocateUninitializedArray<byte>(4);

foreach (var file in files)
{
    using var bitmap = Image.FromFile(file);
    using var memoryStream = new MemoryStream();
    bitmap.Save(memoryStream, ImageFormat.Bmp);

    BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, (int)memoryStream.Length);

    memoryStream.Position = 0;
    memoryStream.TryGetBuffer(out var buffer);

    gzipStream.Write(lengthBuffer, 0, lengthBuffer.Length);
    gzipStream.Write(buffer.AsSpan());
}

gzipStream.Flush();
fileStream.Flush();
