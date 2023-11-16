namespace MatrixDisplay.Apps.ChristmasTree;

using MatrixSdk;

internal interface IEffect
{
    void Run(ImageBuffer buffer, CancellationToken cancellationToken = default);
}
