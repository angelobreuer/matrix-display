namespace MatrixDisplay;

public interface IDisplayController
{
    void Update(ReadOnlySpan<Color> buffer);
}
