namespace MatrixDisplay.Board;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

public sealed class BoardClient : IDisplayController
{
    private readonly Color[] _colorTable;
    private readonly byte[] _dataTable;
    private readonly Socket _socket;

    public BoardClient(IPEndPoint endPoint)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        _colorTable = GC.AllocateUninitializedArray<Color>(255);
        _dataTable = GC.AllocateUninitializedArray<byte>(255);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(endPoint);
    }

    public void Update(ReadOnlySpan<Color> buffer)
    {
        var colors = ReadOnlySpan<Color>.Empty;

        for (var index = 0; index < buffer.Length; index++)
        {
            var color = buffer[index];
            var tableIndex = colors.IndexOf(color);

            if (tableIndex is -1)
            {
                tableIndex = colors.Length;
                _colorTable[tableIndex] = color;
                colors = _colorTable.AsSpan(0, colors.Length + 1);
            }

            _dataTable[index] = (byte)tableIndex;
        }

        var data = new ReadOnlySpan<byte>(_dataTable);
        var payload = new PixelPayload { ColorTable =  colors, Data = data, };

        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        payload.WriteBytes(arrayBufferWriter);

        _socket.Send(arrayBufferWriter.WrittenSpan, SocketFlags.None);
    }
}
