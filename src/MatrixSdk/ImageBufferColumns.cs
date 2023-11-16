namespace MatrixSdk;

public readonly struct ImageBufferColumns(ImageBuffer Buffer)
{
    public Color this[int ColumnIndex]
    {
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(ColumnIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ColumnIndex, Buffer.Bounds.Width);

            var colors = Buffer.Colors;
            var colorIndex = ColumnIndex;

            for (var rowIndex = 0; rowIndex < Buffer.Bounds.Height; rowIndex++)
            {
                colors[colorIndex] = value;
                colorIndex += Buffer.Bounds.Width;
            }
        }
    }
}
