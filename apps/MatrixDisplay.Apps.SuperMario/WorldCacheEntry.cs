using MatrixDisplay;

sealed record WorldCacheEntry(Memory<Color> Buffer, double? PositionX, double? PositionY)
{
    public double? PositionX { get; set; } = PositionX;

    public double? PositionY { get; set; } = PositionY;
}