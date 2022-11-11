using System.Drawing;
using MatrixDisplay;

var text = args.Length is 0
    ? "Hallo Welt"
    : string.Join(' ', args);

const double ScrollSpeed = 2D;

var buffer = PixelBuffer.Instance;
using var font = new Font("Arial", buffer.Height);
var size = buffer.MeasureText(font, text);
var offset = 0D;

while (true)
{
    buffer.DrawText(font, text, (int)Math.Round(offset));
    offset -= ScrollSpeed;

    if (offset < -size.Width)
    {
        offset = size.Width;
    }

    Thread.Sleep(100);
}