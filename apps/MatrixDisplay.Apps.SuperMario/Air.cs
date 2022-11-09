using MatrixDisplay;

sealed class Air : Material
{
    public static readonly Color Color = new Color(0x5C, 0x94, 0xFA);
    public override bool IsSolid => false;
}
