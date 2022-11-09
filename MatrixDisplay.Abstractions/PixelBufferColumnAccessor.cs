namespace MatrixDisplay;

public readonly record struct PixelBufferColumnAccessor(PixelBuffer Buffer)
{
    public Color this[int offset]
    {
        set
        {
            var position = offset;
            var span = Buffer.Data;

            do
            {
                span[position] = value;
                position += Buffer.Width;
            }
            while (position < span.Length);

            Buffer.MarkDirty();
        }
    }
}
