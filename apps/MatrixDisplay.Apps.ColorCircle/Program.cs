using MatrixDisplay;

var buffer = PixelBuffer.Instance;

var delta = 3.5D;
var offsetX = 6.0D;
var offsetY = 6.0D;

buffer.AutoCommit = false;

void Paint(double hueOffset = 0.0D)
{
    for (var rad = 0.0D; rad < Math.PI * 2; rad += Math.PI / 36D)
    {
        var (sin, cos) = Math.SinCos(rad);
        var x = (delta * cos) + (delta * sin) + offsetX;
        var y = (delta * -sin) + (delta * cos) + offsetY;

        buffer[(int)x, (int)y] = Color.FromHsv(hueOffset + (rad * 180D / Math.PI), 1D, 1D);
    }
}

const double Speed = 5.0D;

var direction = Speed;

for (double hueOffset = 0.0D; ; hueOffset += direction)
{
    if (hueOffset < 0.0D)
    {
        direction = Speed;
    }
    else if (hueOffset > 500.0D)
    {
        direction = -Speed;
    }

    Paint(hueOffset);
    buffer.Commit();
    Thread.Sleep(16);
}
