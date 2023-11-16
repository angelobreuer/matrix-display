namespace MatrixSdk;
public readonly struct ImageBufferRows(ImageBuffer Buffer)
{
    public Color this[int RowIndex]
    {
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(RowIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(RowIndex, Buffer.Bounds.Height);

            var span = Buffer.Colors.Slice(
                start: RowIndex * Buffer.Bounds.Width,
                length: Buffer.Bounds.Width);

            span.Fill(value);
        }
    }
}
