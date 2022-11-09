namespace MatrixDisplay.Board;

using System.Buffers;
using System.Runtime.InteropServices;

internal readonly ref struct PixelPayload
{
    public ReadOnlySpan<Color> ColorTable { get; init; }

    public ReadOnlySpan<byte> Data { get; init; }

    public void WriteBytes(IBufferWriter<byte> bufferWriter)
    {
        ArgumentNullException.ThrowIfNull(bufferWriter);

        // Header
        Span<byte> span = bufferWriter.GetSpan(3);
        span[0] = 0x01;
        span[1] = (byte)ColorTable.Length;
        span[2] = (byte)Data.Length;
        bufferWriter.Advance(3);

        // Color Table
        bufferWriter.Write(MemoryMarshal.AsBytes(ColorTable));

        // Colors
        bufferWriter.Write(Data);
    }
}