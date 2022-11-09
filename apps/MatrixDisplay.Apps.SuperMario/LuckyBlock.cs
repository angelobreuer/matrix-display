using MatrixDisplay;

sealed class LuckyBlock : Material
{
    public static readonly Color Color = new(0xFF, 0xB3, 0x00);

    public override bool IsSolid => true;

    public override void HandleJump(MaterialPhysicsContext context)
    {
        context.Buffer.Span[(context.PositionY * PixelBuffer.Instance.Width) + context.PositionX] = Air.Color;
    }
}
