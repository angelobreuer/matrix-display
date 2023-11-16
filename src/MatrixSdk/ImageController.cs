namespace MatrixSdk;

using System.Buffers.Binary;
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

    public void Send(ReadOnlySpan<Color> colors)
    {
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

        for (var index = 0; index < colors.Length; index++)
        {
            var color = colors[index];
            var sendIndex = index * 3;

            sendBuffer[sendIndex] = color.R;
            sendBuffer[sendIndex + 1] = color.G;
            sendBuffer[sendIndex + 2] = color.B;
        }

        socket.SendTo(sendBuffer, SocketFlags.None, _destinationEndPoint);
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _socket, null)?.Dispose();
    }
}
