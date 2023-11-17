namespace MatrixSdk;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public sealed class ImageController : IDisposable
{
    private readonly IPEndPoint _destinationEndPoint;
    private byte[]? _sendBuffer;
    private Socket? _socket;
    private ulong _sequenceNumber;

    public ImageController(IPEndPoint destinationEndPoint)
    {
        _destinationEndPoint = destinationEndPoint;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public void Send(ReadOnlySpan<Color> colors, ImageBounds bounds)
    {
        Debug.Assert(bounds.Width * bounds.Height == colors.Length);

        var socket = _socket;
        ObjectDisposedException.ThrowIf(socket is null, this);

        var totalSize = (colors.Length * 3) + 14;

        if (_sendBuffer is null || _sendBuffer.Length < totalSize)
        {
            _sendBuffer = GC.AllocateUninitializedArray<byte>(totalSize);
        }

        var sendBuffer = _sendBuffer.AsSpan();

        // Header
        sendBuffer[0] = (byte)'W';
        sendBuffer[1] = (byte)'R';
        sendBuffer[2] = (byte)'G';
        sendBuffer[3] = (byte)'B';

        // Pixel Count
        BinaryPrimitives.WriteUInt16LittleEndian(
            destination: sendBuffer[4..],
            value: (ushort)colors.Length);

        // Sequence Number
        BinaryPrimitives.WriteUInt64LittleEndian(
            destination: sendBuffer[6..],
            value: Interlocked.Increment(ref _sequenceNumber));

#pragma warning disable CS8321
        int NoTransform(int x, int y) => (y * bounds.Width) + x;
        int HorizontalMirror(int x, int y) => (y * bounds.Width) + (bounds.Width - x - 1);
        int VerticalMirror(int x, int y) => ((bounds.Width - y - 1) * bounds.Width) + x;
        int HorizontalVerticalMirror(int x, int y) => ((bounds.Width - y - 1) * bounds.Width) + (bounds.Width - x - 1);
#pragma warning restore CS8321

        var transform = HorizontalMirror;
        var sendIndex = 14;

        for (var y = 0; y < bounds.Height; y++)
        {
            for (var x = 0; x < bounds.Width; x++)
            {
                var color = colors[transform(x, y)];

                sendBuffer[sendIndex++] = color.R;
                sendBuffer[sendIndex++] = color.G;
                sendBuffer[sendIndex++] = color.B;
            }
        }

        socket.SendTo(sendBuffer, SocketFlags.None, _destinationEndPoint);
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _socket, null)?.Dispose();
    }
}
