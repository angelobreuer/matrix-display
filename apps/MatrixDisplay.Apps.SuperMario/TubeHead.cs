using MatrixDisplay;

sealed class TubeHead : Material
{
    public static readonly Color Color = new Color(0x01, 0x85, 0x01);
    public override bool IsSolid => true;

    public override void HandleJump(MaterialPhysicsContext context)
    {
        if (GameLogic.CurrentWorld == Worlds.MainWorld)
        {
            GameLogic.ChangeWorld(Worlds.World2);
        }
        else if (GameLogic.CurrentWorld == Worlds.World7)
        {
            GameLogic.ChangeWorld(Worlds.World8);
        }
        else if (GameLogic.CurrentWorld == Worlds.World15)
        {
            GameLogic.ChangeWorld(Worlds.World16);
        }

        PixelBuffer.Instance.Commit();
    }
}
