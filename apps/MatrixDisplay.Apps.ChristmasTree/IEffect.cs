namespace MatrixDisplay.Apps.ChristmasTree;

using MatrixDisplay;

internal interface IEffect
{
    void Run(PixelBuffer buffer, CancellationToken cancellationToken = default);
}
