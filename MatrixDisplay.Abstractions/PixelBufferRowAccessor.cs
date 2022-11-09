﻿namespace MatrixDisplay;

public readonly record struct PixelBufferRowAccessor(PixelBuffer Buffer)
{
    public Color this[int offset]
    {
        set
        {
            var startPosition = offset * Buffer.Width;
            var endPosition = (offset + 1) * Buffer.Height;

            Buffer.Data[startPosition..endPosition].Fill(value);
            Buffer.MarkDirty();
        }
    }
}
