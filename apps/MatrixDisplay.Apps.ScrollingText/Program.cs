using System.Drawing;
using MatrixDisplay;

const string Text = "Hallo Welt";
const double ScrollSpeed = 2D;

var buffer = PixelBuffer.Instance;
using var font = new Font("Arial", buffer.Height);
var size = buffer.MeasureText(font, Text);

var offset = 0D;

while (true)
{
    buffer.DrawText(font, Text, (int)Math.Round(offset));
    offset -= ScrollSpeed;

    if (offset < -size.Width)
    {
        offset = size.Width;
    }

    Thread.Sleep(100);
}