namespace MatrixDisplay.Board;

using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

public sealed class BoardClient : IDisplayController
{
    private readonly Color[] _colorTable;
    private readonly byte[] _dataTable;
    private readonly Socket _socket;
    private readonly int _width;
    private readonly int _height;

    public BoardClient(IPEndPoint endPoint, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        _colorTable = GC.AllocateUninitializedArray<Color>(255);
        _dataTable = GC.AllocateUninitializedArray<byte>(255);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(endPoint);
        _width = width;
        _height = height;
    }

    public void Update(ReadOnlySpan<Color> buffer)
    {
        var colors = ReadOnlySpan<Color>.Empty;

        for (var index = 0; index < buffer.Length; index++)
        {
            var (row, column) = Math.DivRem(index, _width);

            var color = buffer[(_width * (_height - 1 - row)) + (_width - 1 - column)];
            var tableIndex = colors.IndexOf(color);

            if (tableIndex is -1)
            {
                tableIndex = colors.Length;
                _colorTable[tableIndex] = color;
                colors = _colorTable.AsSpan(0, colors.Length + 1);
            }

            _dataTable[index] = (byte)tableIndex;
        }

        var data = new ReadOnlySpan<byte>(_dataTable, 0, buffer.Length);
        var payload = new PixelPayload { ColorTable = colors, Data = data, };

        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        payload.WriteBytes(arrayBufferWriter);

        _socket.Send(arrayBufferWriter.WrittenSpan, SocketFlags.None);
    }
}
