using MatrixSdk;

var buffer = ImageBuffer.Create();

var points = new (int X, int Y, bool New)[]
{
    // Segments for "B"
    (X: 0, Y: 1, New: false),
    (X: 0, Y: 2, New: false),
    (X: 0, Y: 3, New: false),
    (X: 0, Y: 4, New: false),
    (X: 0, Y: 5, New: false),
    (X: 0, Y: 6, New: false),
    (X: 0, Y: 7, New: false),
    (X: 1, Y: 7, New: false),
    (X: 2, Y: 7, New: false),
    (X: 3, Y: 6, New: false),
    (X: 3, Y: 5, New: false),
    (X: 2, Y: 4, New: false),
    (X: 1, Y: 4, New: false),
    (X: 3, Y: 3, New: false),
    (X: 3, Y: 2, New: false),
    (X: 2, Y: 1, New: false),
    (X: 1, Y: 1, New: false),

    // Segments for "K"
    (X: 4, Y: 3, New: true),
    (X: 4, Y: 4, New: false),
    (X: 4, Y: 5, New: false),
    (X: 4, Y: 6, New: false),
    (X: 4, Y: 7, New: false),
    (X: 4, Y: 8, New: false),
    (X: 5, Y: 5, New: false),
    (X: 5, Y: 6, New: false),
    (X: 6, Y: 5, New: false),
    (X: 6, Y: 6, New: false),
    (X: 6, Y: 4, New: false),
    (X: 7, Y: 3, New: false),
    (X: 6, Y: 7, New: false),
    (X: 7, Y: 8, New: false),

    // Segments for "W"
    (X: 8, Y: 8, New: true),
    (X: 8, Y: 7, New: false),
    (X: 8, Y: 6, New: false),
    (X: 8, Y: 5, New: false),
    (X: 9, Y: 9, New: false),
    (X: 10, Y: 8, New: false),
    (X: 10, Y: 7, New: false),
    (X: 10, Y: 6, New: false),
    (X: 10, Y: 5, New: false),
    (X: 11, Y: 9, New: false),
    (X: 12, Y: 8, New: false),
    (X: 12, Y: 7, New: false),
    (X: 12, Y: 6, New: false),
    (X: 12, Y: 5, New: false),
};

const double Speed = 3.0D;

for (var hueOffset = 0.0D; ; hueOffset += Speed)
{
    buffer.Clear();

    var offset = 0;
    var step = 4D;

    foreach (var (x, y, isNew) in points.Reverse())
    {
        offset++;
        buffer[x, y] = Color.FromHsv(hueOffset + (offset * step), 1D, 1D);

        if (isNew)
        {
            offset += 16;
        }
    }

    buffer.Commit();

    Thread.Sleep(16);
}