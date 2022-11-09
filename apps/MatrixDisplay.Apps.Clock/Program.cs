using MatrixDisplay;

var buffer = PixelBuffer.Instance;

buffer.AutoCommit = false;

var colorWhite = new Color(0xFF, 0xFF, 0xFF);
var colorGray = new Color(0x80, 0x80, 0x80);

var positionX = buffer.Width / 2D;
var positionY = buffer.Height / 2D;

var delta = 3.5D;

void Render()
{
    for (var offset = 0.0D; offset < Math.PI * 2D; offset += 0.1D)
    {
        var (sin, cos) = Math.SinCos(offset);
        var x = (delta * cos) + (delta * sin) + positionX;
        var y = (delta * -sin) + (delta * cos) + positionY;

        buffer[(int)x, (int)y] = colorWhite;
    }

    buffer[1, (int)positionY] = colorGray;
    buffer[(int)positionX, 0] = colorGray;
    buffer[buffer.Width - 2, (int)positionY] = colorGray;
    buffer[(int)positionX, buffer.Height - 1] = colorGray;
}

void DrawLine(double angle, double length, Color color)
{
    var x = positionX;
    var y = positionY;

    var (sin, cos) = Math.SinCos(angle);

    while (length-- > 0)
    {
        x += cos;
        y += sin;

        buffer[(int)x, (int)y] = color;
    }
}

while (true)
{
    buffer.Clear();

    var dateTime = DateTime.Now;
    var seconds = (dateTime.Second / 60D * (Math.PI * 2)) + (Math.PI * 1.5);
    var minutes = (dateTime.Minute / 60D * (Math.PI * 2)) + (Math.PI * 1.5);
    var hours = (dateTime.Hour / 12D % 12 * (Math.PI * 2)) + (Math.PI * 1.5);

    Render();

    DrawLine(seconds, 1.3 * delta, Color.Red); // Seconds
    DrawLine(minutes, 1.2 * delta, Color.Green); // Minutes
    DrawLine(hours, 0.8 * delta, Color.Blue); // Hours

    buffer.Commit();
    Thread.Sleep(500);
}
