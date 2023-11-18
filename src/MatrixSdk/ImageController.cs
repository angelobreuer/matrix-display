namespace MatrixSdk;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public sealed class ImageController : IDisposable
{
    private readonly Func<int, int, int, int, int> _transform;
    private readonly IPEndPoint _destinationEndPoint;
    private byte[]? _sendBuffer;
    private Socket? _socket;
    private ulong _sequenceNumber;

    public ImageController(ControllerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        static int GetAlternatingX(int x, int y, int width) => y % 2 is 0 ? x : width - 1 - x;

        static int NoTransform(int x, int y, int width, int height) => (y * width) + x;
        static int HorizontalMirror(int x, int y, int width, int height) => (y * width) + (width - x - 1);
        static int VerticalMirror(int x, int y, int width, int height) => ((height - y - 1) * width) + x;
        static int HorizontalVerticalMirror(int x, int y, int width, int height) => ((height - y - 1) * width) + (width - x - 1);

        static int AlternatingNoTransform(int x, int y, int width, int height) => NoTransform(GetAlternatingX(x, y, width), y, width, height);
        static int AlternatingHorizontalMirror(int x, int y, int width, int height) => HorizontalMirror(GetAlternatingX(x, y, width), y, width, height);
        static int AlternatingVerticalMirror(int x, int y, int width, int height) => VerticalMirror(GetAlternatingX(x, y, width), y, width, height);
        static int AlternatingHorizontalVerticalMirror(int x, int y, int width, int height) => HorizontalVerticalMirror(GetAlternatingX(x, y, width), y, width, height);

        _destinationEndPoint = new IPEndPoint(IPAddress.Parse(options.Host), options.Port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        _transform = options.ImageMirroring switch
        {
            ImageMirroringOption.Horizontal => HorizontalMirror,
            ImageMirroringOption.Vertical => VerticalMirror,
            ImageMirroringOption.HorizontalVertical => HorizontalVerticalMirror,
            ImageMirroringOption.Alternating => AlternatingNoTransform,
            ImageMirroringOption.AlternatingHorizontal => AlternatingHorizontalMirror,
            ImageMirroringOption.AlternatingVertical => AlternatingVerticalMirror,
            ImageMirroringOption.AlternatingHorizontalVertical => AlternatingHorizontalVerticalMirror,
            _ => NoTransform,
        };
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

        var sendIndex = 14;

        for (var y = 0; y < bounds.Height; y++)
        {
            for (var x = 0; x < bounds.Width; x++)
            {
                var color = colors[_transform(x, y, bounds.Width, bounds.Height)];

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
