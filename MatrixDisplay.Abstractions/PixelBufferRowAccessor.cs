namespace MatrixDisplay;

public readonly record struct PixelBufferRowAccessor(PixelBuffer Buffer)
{
    public Color this[Index index]
    {
        set
        {
            var offset = index.GetOffset(Buffer.Height);
            SetRow(offset, value);

            Buffer.MarkDirty();
        }
    }

    public Color this[Range range]
    {
        set
        {
            var (offset, length) = range.GetOffsetAndLength(Buffer.Height);

            for (var yOffset = 0; offset < length; yOffset++)
            {
                SetRow(offset + yOffset, value);
            }

            Buffer.MarkDirty();
        }
    }

    private void SetRow(int offset, Color value)
    {
        if (offset >= Buffer.Height)
        {
            return;
        }

        var startPosition = offset * Buffer.Width;
        var endPosition = (offset + 1) * Buffer.Width;

        Buffer.Data[startPosition..endPosition].Fill(value);
    }
}
