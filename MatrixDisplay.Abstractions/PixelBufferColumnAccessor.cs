namespace MatrixDisplay;

public readonly record struct PixelBufferColumnAccessor(PixelBuffer Buffer)
{
    public Color this[Index index]
    {
        set
        {
            var offset = index.GetOffset(Buffer.Width);
            SetColumn(offset, value);

            Buffer.MarkDirty();
        }
    }

    public Color this[Range range]
    {
        set
        {
            var (offset, length) = range.GetOffsetAndLength(Buffer.Width);

            for (var xOffset = 0; offset < length; xOffset++)
            {
                SetColumn(offset + xOffset, value);
            }

            Buffer.MarkDirty();
        }
    }

    private void SetColumn(int offset, Color value)
    {
        if (offset >= Buffer.Width)
        {
            return;
        }

        var position = offset;
        var span = Buffer.Data;

        do
        {
            span[position] = value;
            position += Buffer.Width;
        }
        while (position < span.Length);
    }
}
