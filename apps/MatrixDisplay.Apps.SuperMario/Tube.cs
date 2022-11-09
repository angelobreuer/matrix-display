using MatrixDisplay;

sealed class Tube : Material
{
    public static readonly Color Color = new Color(0x00, 0xA8, 0x00);
    public override bool IsSolid => false;
}
